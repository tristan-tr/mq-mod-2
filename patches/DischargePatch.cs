using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using mq_mod_2.utils;

namespace mq_mod_2.patches;

public static class DischargePatch
{
    // [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.rpcApplyDamage))]
    // [HarmonyTranspiler]
    // public static IEnumerable<CodeInstruction> rpcApplyDamageTranspiler(IEnumerable<CodeInstruction> instructions)
    // {
    //     // Change 0.5f (50% reduction) to 0.33333334f (2/3rds reduction)
    //     return instructions.ReplaceConstant(0.5f, 0.33333334f);
    // }

    [HarmonyPatch(typeof(PhysicsBody), nameof(PhysicsBody.rpcAddForce))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> rpcAddForceTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .Start()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarga, 1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DischargePatch), nameof(ModifyImpulse)))
            )
            .InstructionEnumeration();
    }

    public static void ModifyImpulse(PhysicsBody instance, ref Vector3 impulse)
    {
        var status = instance.GetComponent<WizardStatus>();
        if (status != null && status.dischargeCount > 0)
        {
            impulse *= 0.5f;
        }
    }
}
