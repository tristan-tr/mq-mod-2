using HarmonyLib;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(GeyserObject), nameof(GeyserObject.Init))]
public class GeyserPatch
{
    static void Prefix(ref float ___RADIUS)
    {
        ___RADIUS = 7f;
    }
}