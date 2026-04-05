using HarmonyLib;

namespace mq_mod_2.patches.balance.statchange;

[HarmonyPatch(typeof(GlaiveObject), MethodType.Constructor)]
public class GlaivePatch
{
    static void Postfix(GlaiveObject __instance, ref float ___POWER)
    {
        __instance.DAMAGE = 8f;
        ___POWER = 38f;
    }
}