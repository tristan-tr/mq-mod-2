using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(TsunamiObject))]
public static class TsunamiWiderPatch
{
    private const float SIZE_MULTIPLIER = 1.15f;
    
    [HarmonyPatch(nameof(TsunamiObject.SpellObjectStart))]
    [HarmonyPostfix]
    public static void SpellObjectStartPostfix(TsunamiObject __instance)
    {
        Vector3 scale = __instance.transform.localScale;
        scale.x *= SIZE_MULTIPLIER;
        __instance.transform.localScale = scale;
    }

    [HarmonyPatch("FixedUpdate")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixedUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            // SIZE_INCREMENT = 0.05f
            if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float f && f == 0.05f)
            {
                instruction.operand = 0.05f * SIZE_MULTIPLIER; // 0.05 * 1.25
            }
            yield return instruction;
        }
    }

    // Adjusting RADIUS in case it's used for any internal logic (e.g. AI or other components)
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPostfix]
    public static void ConstructorPostfix(TsunamiObject __instance)
    {
        // RADIUS is protected in SpellObject, but TsunamiObject inherits it.
        // We use Traverse to handle protected access.
        var radiusField = Traverse.Create(__instance).Field("RADIUS");
        radiusField.SetValue(radiusField.GetValue<float>() * SIZE_MULTIPLIER);
    }
}
