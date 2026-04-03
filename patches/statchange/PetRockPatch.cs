using HarmonyLib;

namespace mq_mod_2.patches.statchange;

[HarmonyPatch(typeof(PetRockObject), MethodType.Constructor)]
public class PetRockPatch
{
    static void Postfix(ref float ___POWER)
    {
        ___POWER = 30f;
    }

}