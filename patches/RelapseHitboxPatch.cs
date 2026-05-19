using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class RelapseHitboxPatch
{
    [HarmonyPatch(typeof(SpellManager), nameof(SpellManager.Awake))]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null &&
            __instance.spell_table.TryGetValue(SpellName.Relapse, out var relapse) &&
            __instance.spell_table.TryGetValue(SpellName.Geyser, out var geyser))
        {
            relapse.animationName = geyser.animationName;
            relapse.windUp = geyser.windUp;
            relapse.windDown = geyser.windDown;
            relapse.spellRadius = 5f;
        }
    }

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
        ___RADIUS = 5f;
    }
}
