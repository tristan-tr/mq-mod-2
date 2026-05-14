using HarmonyLib;
using UnityEngine;

namespace MqMod.Patches
{
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
}
