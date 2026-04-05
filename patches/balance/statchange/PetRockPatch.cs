using HarmonyLib;

namespace mq_mod_2.patches.balance.statchange;

[HarmonyPatch(typeof(PetRockObject), nameof(PetRockObject.Init))]
public class PetRockPatch
{
    static void Prefix(ref float velocity)
    {
        velocity *= 1.2f;
    }
}