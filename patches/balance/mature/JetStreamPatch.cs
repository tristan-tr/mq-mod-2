using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.balance.mature;

[HarmonyPatch]
public static class JetStreamPatch
{
    [HarmonyPatch(typeof(JetStreamObject), nameof(JetStreamObject.Init))]
    [HarmonyPrefix]
    public static void JetStreamObject_Init_Prefix(ref float curve)
    {
        curve *= 1.5f;
    }

    [HarmonyPatch(typeof(Spell), nameof(Spell.CalculateStaticValues))]
    [HarmonyPostfix]
    public static void Spell_CalculateStaticValues_Postfix(Spell __instance)
    {
        if (__instance is JetStream)
        {
            Spell.sCurveMultiplier *= 1.5f;
        }
    }
}
