using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.balance.mature
{
    [HarmonyPatch(typeof(FrogOfLifeObject), "Heal")]
    public static class FrogOfLifePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var addForceOwnerMethod = typeof(PhysicsBody).GetMethod(nameof(PhysicsBody.AddForceOwner), new[] { typeof(Vector3) });

            if (addForceOwnerMethod == null)
            {
                Plugin.Logger.LogError("Could not find PhysicsBody.AddForceOwner method");
                return instructions;
            }

            int removedCount = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if ((codes[i].opcode == OpCodes.Callvirt || codes[i].opcode == OpCodes.Call) && (MethodInfo)codes[i].operand == addForceOwnerMethod)
                {
                    // AddForceOwner(Vector3 impulse) is called on PhysicsBody instance.
                    // Stack: [PhysicsBody, Vector3]
                    // We need to pop both.
                    
                    var firstPop = new CodeInstruction(OpCodes.Pop);
                    var secondPop = new CodeInstruction(OpCodes.Pop);
                    
                    // Move labels from the original call to our first pop
                    if (codes[i].labels.Count > 0)
                    {
                        firstPop.labels.AddRange(codes[i].labels);
                        codes[i].labels.Clear();
                    }
                    
                    codes[i] = firstPop;
                    codes.Insert(i + 1, secondPop);
                    i++; // Skip the inserted instruction
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                Plugin.Logger.LogInfo($"Successfully removed {removedCount} AddForceOwner calls from FrogOfLifeObject.Heal");
            }
            else
            {
                Plugin.Logger.LogWarning("Did not find any AddForceOwner calls in FrogOfLifeObject.Heal");
            }

            return codes;
        }
    }
}
