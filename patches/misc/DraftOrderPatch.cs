using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace mq_mod_2.patches.misc;

[HarmonyPatch]
public static class DraftOrderPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<MethodBase> TargetMethods()
    {
        var methods = new List<MethodBase>();
        
        // Search in GameUtility and all its nested types (anonymous delegates are often in nested types)
        var types = new List<Type> { typeof(GameUtility) };
        types.AddRange(typeof(GameUtility).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public));

        foreach (var type in types)
        {
            foreach (var m in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                // Look for methods that are likely the anonymous delegates in ReorderPlayers
                if (m.Name.Contains("ReorderPlayers") && m.ReturnType == typeof(int))
                {
                    var parameters = m.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(int))
                    {
                        methods.Add(m);
                    }
                }
            }
        }

        if (methods.Count == 0)
        {
            Plugin.Logger.LogError("DraftOrderPatch: Could not find any target methods for ReorderPlayers.");
        }
        else
        {
            Plugin.Logger.LogInfo($"DraftOrderPatch: Found {methods.Count} target methods.");
        }

        return methods;
    }

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        bool found = false;

        for (int i = 0; i < codes.Count; i++)
        {
            // The tie-breaker for draft order uses (damageDealt + healingApplied).CompareTo(...)
            // Since damage and healing are floats, the comparison is float.CompareTo(float).
            // Other comparisons in the sorting delegate (SumKills, player index) use int.CompareTo(int).
            
            if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && 
                mi.Name == "CompareTo" && mi.DeclaringType == typeof(float))
            {
                // Inserting a Neg instruction after CompareTo will reverse its result (-1 becomes 1, 1 becomes -1).
                // This effectively reverses the sorting order for this specific tie-breaker.
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Neg));
                found = true;
                
                Plugin.Logger.LogInfo($"DraftOrderPatch: Applied Neg to {mi.DeclaringType.Name}.{mi.Name}");
            }
        }

        if (!found)
        {
            Plugin.Logger.LogError("DraftOrderPatch: Could not find the float.CompareTo call to reverse.");
        }

        return codes.AsEnumerable();
    }
}
