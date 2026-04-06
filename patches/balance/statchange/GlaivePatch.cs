using HarmonyLib;

namespace mq_mod_2.patches.balance.statchange;

[HarmonyPatch(typeof(GlaiveObject), nameof(GlaiveObject.Init))]
public class GlaivePatch
{
    static void Prefix(GlaiveObject __instance, ref float ___POWER)
    {
        __instance.DAMAGE = 8f;
        ___POWER = 38f;
    }
}