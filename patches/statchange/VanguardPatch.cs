using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace mq_mod_2.patches.statchange;

[HarmonyPatch(typeof(VanguardObject), "rpcCollision")]
public static class VanguardPatch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // absorb up to 100 damage instead of 60
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && (float)i.operand == 60f))
            .Repeat(matcher => matcher.SetOperandAndAdvance(100f))
            .InstructionEnumeration();
    }
}
