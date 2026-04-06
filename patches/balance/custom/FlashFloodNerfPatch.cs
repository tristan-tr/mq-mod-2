using HarmonyLib;

namespace mq_mod_2.patches.balance.custom;

[HarmonyPatch(typeof(SpellHandler), "RefreshPrimary")]
public static class FlashFloodNerfPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return false; // Skip the original method which resets the primary cooldown
    }
}
