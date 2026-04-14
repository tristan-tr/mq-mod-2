using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace mq_mod_2.utils;

public static class CodeInstructionExtensions
{
    /// <summary>
    /// Transpiler helper to replace all calls of one method with another.
    /// </summary>
    public static IEnumerable<CodeInstruction> ReplaceMethodCall(this IEnumerable<CodeInstruction> instructions, MethodInfo oldMethod, MethodInfo newMethod)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(oldMethod))
            {
                yield return new CodeInstruction(OpCodes.Call, newMethod);
            }
            else
            {
                yield return instruction;
            }
        }
    }
    
    /// <summary>
    /// A helper for transpilers that uses CodeMatcher to replace all occurrences of a constant.
    /// </summary>
    public static IEnumerable<CodeInstruction> ReplaceConstant(this IEnumerable<CodeInstruction> instructions, float oldVal, float newVal)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(i => i.LoadsConstant(oldVal)))
            .Repeat(matcher => matcher.SetOperandAndAdvance(newVal))
            .InstructionEnumeration();
    }

    /// <summary>
    /// A helper for transpilers that uses CodeMatcher to replace all occurrences of a constant.
    /// </summary>
    public static IEnumerable<CodeInstruction> ReplaceConstant(this IEnumerable<CodeInstruction> instructions, int oldVal, int newVal)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(i => i.LoadsConstant(oldVal)))
            .Repeat(matcher => matcher.SetOperandAndAdvance(newVal))
            .InstructionEnumeration();
    }
}
