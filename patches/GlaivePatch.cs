using HarmonyLib;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class GlaivePatch
{
    [HarmonyPatch(typeof(GlaiveObject), nameof(GlaiveObject.Init))]
    [HarmonyPrefix]
    static void InitPrefix(GlaiveObject __instance, ref float ___POWER)
    {
        __instance.DAMAGE = 8f;
        ___POWER = 38f;
    }

    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null && __instance.spell_table.TryGetValue(SpellName.Glaive, out Spell glaive))
        {
            glaive.cooldown = 3f;
        }
    }
}