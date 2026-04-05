using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace mq_mod_2.patches.balance.custom;

[HarmonyPatch(typeof(WizardStatus), "rpcApplyDamage")]
public class BubbleBreakerNoLavaTriggerPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        
        // Find: if (this.bubbleBreakerCount > 0)
        // IL:
        // ldarg.0
        // ldfld int32 WizardStatus::bubbleBreakerCount
        // ldc.i4.0
        // ble.s <branch_target>
        matcher.MatchForward(false,
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WizardStatus), "bubbleBreakerCount")),
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(i => i.opcode == OpCodes.Ble || i.opcode == OpCodes.Ble_S));

        if (matcher.IsInvalid)
        {
            Plugin.Logger.LogError("Could not find bubbleBreakerCount check in rpcApplyDamage");
            return instructions;
        }

        matcher.Advance(3); // Move to the branch instruction
        var branchTarget = matcher.Operand; // The label for ble
        
        // After the existing check, insert: if (source == -4) goto <branch_target>
        // This effectively turns it into: if (bubbleBreakerCount > 0 && source != -4)
        //
        // source 4 is lava
        matcher.Advance(1) // Move past the branch
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_3), // source
                new CodeInstruction(OpCodes.Ldc_I4, -4),
                new CodeInstruction(OpCodes.Beq, branchTarget)
            );

        return matcher.InstructionEnumeration();
    }
}
