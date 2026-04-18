using System.Collections.Generic;
using HarmonyLib;
using mq_mod_2.utils;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class RocketStatsPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table.TryGetValue(SpellName.Rocket, out Spell rocket))
        {
            rocket.cooldown = 4f;
        }
    }

    [HarmonyPatch(typeof(RocketObject), "Awake")]
    [HarmonyPostfix]
    public static void RocketObjectAwakePostfix(RocketObject __instance)
    {
        __instance.DAMAGE = 8f;
    }

    [HarmonyPatch(typeof(RocketObject), "OnCollisionEnter")]
    [HarmonyPostfix]
    public static void RocketObjectOnCollisionEnterPostfix(RocketObject __instance, List<GameObject> ___alreadyHit)
    {
        if (!__instance.photonView.IsMine()) return;

        int count = ___alreadyHit.Count;
        if (count == 1)
        {
            __instance.DAMAGE = 10f;
        }
        else if (count == 2)
        {
            __instance.DAMAGE = 12f;
        }
    }
}
