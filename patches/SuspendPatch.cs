using HarmonyLib;

namespace mq_mod_2.patches;

public static class SuspendPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.RegisterCooldown))]
    [HarmonyPrefix]
    public static void RegisterCooldownPrefix(SpellName spellName, ref float cooldown)
    {
        if (spellName == SpellName.Suspend)
        {
            cooldown -= 1f;
        }
    }

    [HarmonyPatch(typeof(SuspendObject), MethodType.Constructor)]
    [HarmonyPostfix]
    public static void SuspendObjectConstructorPostfix(SuspendObject __instance)
    {
        AccessTools.Field(typeof(SpellObject), "POWER").SetValue(__instance, 25f);
    }
}
