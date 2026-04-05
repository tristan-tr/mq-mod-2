using HarmonyLib;

namespace mq_mod_2.patches.balance.statchange;

[HarmonyPatch(typeof(GeyserObject), MethodType.Constructor)]
public class GeyserPatch
{
    static void Postfix(GeyserObject __instance, ref float ___RADIUS)
    {
        ___RADIUS = 8f;
    }
}