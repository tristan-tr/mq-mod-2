using System.Collections.Generic;
using HarmonyLib;
using mq_mod_2.utils;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(VanguardObject), nameof(VanguardObject.rpcCollision))]
public static class VanguardPatch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // absorb up to 100 damage instead of 60
        return TranspilerUtils.ReplaceConstant(instructions, 60f, 100f);
    }
}
