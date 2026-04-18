using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// ai slop

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class HotSwapRecastPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null && __instance.spell_table.TryGetValue(SpellName.HotSwap, out var hotswap))
        {
            // Add Custom and Attack flags so AI considers it for recasting
            hotswap.uses |= SpellUses.Custom | SpellUses.Attack;
        }
    }

    [HarmonyPatch(typeof(HotSwap), nameof(HotSwap.AvailableOverride))]
    [HarmonyPrefix]
    public static bool AvailableOverridePrefix(HotSwap __instance, AiController ai, int owner, SpellUses use, int reactivate, ref bool __result)
    {
        // Always allow recast if it's available
        if (reactivate > 1)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(HotSwap), nameof(HotSwap.GetAiAim))]
    [HarmonyPrefix]
    public static bool GetAiAimPrefix(HotSwap __instance, ref Vector3? __result, TargetComponent targetComponent, SpellUses use)
    {
        // For recasts, aim doesn't matter much as it teleports to the object, 
        // but we need to return a non-null value so the AI decides to cast it.
        if (__instance.reactivate > 1)
        {
            if (use == SpellUses.Custom)
            {
                __result = targetComponent.transform.forward;
            }
            else
            {
                __result = Vector3.forward;
            }
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(HotSwapObject), nameof(HotSwapObject.Teleport))]
    [HarmonyPrefix]
    public static bool TeleportPrefix(HotSwapObject __instance)
    {
        // If we are still a projectile, it means we just hit the ground.
        // Instead of teleporting immediately, we want to enable recast.
        if (__instance.state == HotSwapObject.HotSwapState.Projectile)
        {
            if (Globals.online && __instance.photonView != null && __instance.photonView.isMine && PhotonNetwork.connected)
            {
                __instance.photonView.RPC("rpcCollision", PhotonTargets.All, new object[] { -1 });
            }
            else
            {
                __instance.localCollision(null);
            }
            return false; // Skip original Teleport
        }
        return true;
    }

    [HarmonyPatch(typeof(HotSwapObject), nameof(HotSwapObject.localCollision))]
    [HarmonyPostfix]
    public static void LocalCollisionPostfix(HotSwapObject __instance, GameObject go, PhysicsBody ___phys)
    {
        // Ensure state and isKinematic are set even if go is null (ground hit)
        __instance.state = HotSwapObject.HotSwapState.AttachedToObject;
        
        if (___phys != null && ___phys.rig != null)
        {
            ___phys.rig.isKinematic = true;
        }
    }

    [HarmonyPatch(typeof(HotSwapObject), "FixedUpdate")]
    [HarmonyPrefix]
    public static void FixedUpdatePrefix(HotSwapObject __instance, out bool __state, Transform ___target, UnitStatus ___enemyStatus)
    {
        __state = false;
        // If we hit the ground and are waiting for recast, state is AttachedToObject and target is null.
        // FixedUpdate would normally set deathTimer to 0 if target is null in this state.
        // We temporarily change the state to skip that logic.
        if (__instance.state == HotSwapObject.HotSwapState.AttachedToObject)
        {
            if (___target == null && ___enemyStatus == null)
            {
                __instance.state = HotSwapObject.HotSwapState.TeleportImmediately; // Switch is skip-only for this state
                __state = true;
            }
        }
    }

    [HarmonyPatch(typeof(HotSwapObject), "FixedUpdate")]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(HotSwapObject __instance, bool __state)
    {
        if (__state)
        {
            __instance.state = HotSwapObject.HotSwapState.AttachedToObject;
        }
    }

    [HarmonyPatch(typeof(HotSwapObject), nameof(HotSwapObject.Swap))]
    [HarmonyPrefix]
    public static bool SwapPrefix(HotSwapObject __instance, float force, Transform ___target, UnitStatus ___enemyStatus)
    {
        // If we recast and there's no target, we perform the teleport.
        if (___target == null)
        {
            if (___enemyStatus == null) // Ground hit
            {
                __instance.Teleport();
            }
            // If enemyStatus is NOT null, it means we were attached to an enemy that is now gone.
            // In that case, we don't want to teleport or swap.
            return false;
        }
        return true;
    }
}
