using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using mq_mod_2.utils;

namespace mq_mod_2.patches;

public static class VanguardPatch
{
    [HarmonyPatch(typeof(VanguardObject), nameof(VanguardObject.rpcCollision))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> rpcCollisionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // absorb up to 120 damage instead of 60
        return instructions.ReplaceConstant(60f, 120f);
    }

    [HarmonyPatch(typeof(VanguardObject), "FixedUpdate")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixedUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var curveField = AccessTools.Field(typeof(SpellObject), nameof(SpellObject.curve));
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldfld, curveField))
            .Repeat(matcher => matcher
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, 1.5f))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Mul)))
            .InstructionEnumeration();
    }
}
