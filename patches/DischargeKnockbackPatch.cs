using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(PhysicsBody), nameof(PhysicsBody.rpcAddForce))]
public class DischargeKnockbackPatch
{
    public static void Prefix(ref Vector3 impulse, PhysicsBody __instance)
    {
        WizardStatus ws = __instance.GetComponent<WizardStatus>();
        if (ws == null)
            return;
        
        if (ws.dischargeCount > 0)
            impulse *= 0.5f;
    }
}