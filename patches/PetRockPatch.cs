using HarmonyLib;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(PetRockObject), nameof(PetRockObject.Init))]
public class PetRockPatch
{
    static void Prefix(ref float velocity)
    {
        velocity *= 1.2f;
    }
}