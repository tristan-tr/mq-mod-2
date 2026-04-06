using HarmonyLib;

namespace mq_mod_2.patches.balance.custom;

[HarmonyPatch]
public static class TsunamiSpeedPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        foreach (Spell spell in __instance.GetComponents<Spell>())
        {
            if (spell.spellName == SpellName.Tsunami)
            {
                spell.initialVelocity *= 1.15f;
                Plugin.Logger.LogInfo($"Tsunami initialVelocity increased to {spell.initialVelocity}");
            }
        }
    }

    [HarmonyPatch(typeof(TsunamiObject), nameof(TsunamiObject.SpellObjectStart))]
    [HarmonyPostfix]
    public static void TsunamiObjectStartPostfix(TsunamiObject __instance)
    {
        __instance.velocity *= 1.15f;
    }
}
