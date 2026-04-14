using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.balance.statchange;

[HarmonyPatch]
public static class CompoundPatch
{
    [HarmonyPatch(typeof(CompoundObject), nameof(CompoundObject.Init))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> CompoundObjectInitTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        // Change damage scaling from 3f to 4f
        // Logic: float num2 = this.POWER + (float)num * 12f;
        // Logic: float num3 = this.DAMAGE + (float)num * 3f;
        matcher.Start().MatchForward(false,
                new CodeMatch(i => i.LoadsConstant(3f)),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(OpCodes.Add)
            )
            .ThrowIfInvalid("Could not find damage scaling (num * 3f)")
            .SetOperandAndAdvance(4f);

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(CompoundObject), nameof(CompoundObject.Init))]
    [HarmonyPrefix]
    public static void CompoundObjectInitPrefix(CompoundObject __instance)
    {
        __instance.DAMAGE = 8f;
    }
}
