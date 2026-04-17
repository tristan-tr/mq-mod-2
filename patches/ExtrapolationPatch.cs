using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches;

/// <summary>
/// Patch to improve network synchronization by adding client-side extrapolation
/// for all networked wizards.
/// </summary>
[HarmonyPatch]
public static class ExtrapolationPatch
{
    private class SyncState
    {
        public double lastSentTimestamp;
        public Vector3 lastReceivedPos;
        public Vector3 velocityPerFrame;
    }

    private static readonly ConditionalWeakTable<NetworkedWizard, SyncState> states = new();

    private static readonly AccessTools.FieldRef<NetworkedWizard, Vector3> correctPlayerPosRef =
        AccessTools.FieldRefAccess<NetworkedWizard, Vector3>("correctPlayerPos");
    private static readonly AccessTools.FieldRef<NetworkedWizard, PhysicsBody> physRef =
        AccessTools.FieldRefAccess<NetworkedWizard, PhysicsBody>("phys");
    private static readonly AccessTools.FieldRef<NetworkedWizard, Vector3> deltaPositionRef =
        AccessTools.FieldRefAccess<NetworkedWizard, Vector3>("deltaPosition");
    private static readonly AccessTools.FieldRef<NetworkedWizard, bool> lerpRef =
        AccessTools.FieldRefAccess<NetworkedWizard, bool>("lerp");
    private static readonly AccessTools.FieldRef<NetworkedWizard, float> deltaTimeRef =
        AccessTools.FieldRefAccess<NetworkedWizard, float>("deltaTime");
    private static readonly AccessTools.FieldRef<NetworkedWizard, float> thresholdRef =
        AccessTools.FieldRefAccess<NetworkedWizard, float>("threshold");

    // --- Extrapolation for NetworkedWizard ---

    [HarmonyPatch(typeof(NetworkedWizard), "OnPhotonSerializeView")]
    [HarmonyPostfix]
    public static void NetworkedWizardOnPhotonSerializeViewPostfix(NetworkedWizard __instance, PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isReading)
        {
            if (!states.TryGetValue(__instance, out var state))
            {
                state = new SyncState();
                states.Add(__instance, state);
            }

            // Using local variable to avoid Harmony003 warning
            double packetTimestamp = info.timestamp;
            state.lastSentTimestamp = packetTimestamp;
            state.lastReceivedPos = correctPlayerPosRef(__instance);
            state.velocityPerFrame = physRef(__instance).velocity;

            // Extrapolate the "correct" position based on network lag.
            // PhotonNetwork.time is the current synchronized time.
            // info.timestamp is when the packet was sent.
            double lag = PhotonNetwork.time - packetTimestamp;
            
            // Limit lag to reasonable values (500ms max) to avoid wild jumps
            if (lag > 0 && lag < 0.5)
            {
                // In MageQuit, PhysicsBody.velocity x and z are units per second, 
                // but y is units per frame.
                Vector3 velocityPerSecond = state.velocityPerFrame;
                velocityPerSecond.y /= Time.fixedDeltaTime;
                Vector3 extrapolation = velocityPerSecond * (float)lag;
                
                // Update the target position
                Vector3 newCorrectPos = state.lastReceivedPos + extrapolation;
                correctPlayerPosRef(__instance) = newCorrectPos;
                
                // Recalculate internal NetworkedWizard flags so Update() uses the new position correctly
                Vector3 newDelta = newCorrectPos - __instance.transform.position;
                deltaPositionRef(__instance) = newDelta;
                
                float dt = deltaTimeRef(__instance);
                float thres = thresholdRef(__instance);
                lerpRef(__instance) = dt == 0f || newDelta.sqrMagnitude > thres * dt * dt;
            }
        }
    }
}
