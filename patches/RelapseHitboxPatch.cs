using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class RelapseHitboxPatch
{
    [HarmonyPatch(typeof(Relapse), nameof(Relapse.Initialize))]
    [HarmonyPrefix]
    public static bool RelapseInitializePrefix(Identity identity, Vector3 position, Quaternion rotation, float curve, int spellIndex, bool selfCast, SpellName spellNameForCooldown)
    {
        // Instantiate at position (caster) instead of 4f forward to make it 360 around caster, like Geyser
        var go = GameUtility.Instantiate("Objects/Relapse", position, rotation, 0);
        if (go != null)
        {
            var relapseObject = go.GetComponent<RelapseObject>();
            if (relapseObject != null)
            {
                relapseObject.Init(identity);
            }
        }
        return false;
    }

    [HarmonyPatch(typeof(RelapseObject), nameof(RelapseObject.Init))]
    [HarmonyPrefix]
    public static void RelapseObjectInitPrefix(ref float ___RADIUS)
    {
        ___RADIUS = 6f;
    }
}
