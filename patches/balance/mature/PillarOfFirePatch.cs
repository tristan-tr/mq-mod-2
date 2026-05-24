using HarmonyLib;

namespace mq_mod_2.patches.balance.mature;

[HarmonyPatch(typeof(PillarOfFireObject), nameof(PillarOfFireObject.Init))]
public class PillarOfFirePatch
{
    static void Prefix(PillarOfFireObject __instance)
    {
        __instance.DAMAGE = 6f;
    }
}
