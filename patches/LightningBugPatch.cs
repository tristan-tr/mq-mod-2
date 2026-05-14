using HarmonyLib;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(LightningBugObject), nameof(LightningBugObject.Init))]
public class LightningBugPatch
{
    static void Prefix(ref float velocity)
    {
        velocity *= 1.2f;
    }
}