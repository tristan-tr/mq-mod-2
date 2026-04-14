using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using mq_mod_2.utils;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public class ConsoleDamagePatch
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        var types = new[] {
            typeof(WizardStatus),
            typeof(TargetStatus),
            typeof(SpitfireStatus),
            typeof(Puck),
            typeof(CrystalStatus)
        };
        return types
            .Select(t => AccessTools.Method(t, nameof(WizardStatus.rpcApplyDamage), new Type[] { typeof(float), typeof(int), typeof(int) }))
            .Where(m => m != null);
    }

    [HarmonyPrefix]
    static void Prefix(object __instance, float damage, int owner, int source)
    {
        try
        {
            string victimName = "Unknown Victim";
            if (__instance is MonoBehaviour mb)
            {
                var identity = mb.GetComponent<Identity>();
                victimName = identity != null ? PatchUtils.GetPlayerName(identity.owner) : mb.gameObject.name;
            }

            string attackerName = PatchUtils.GetPlayerName(owner);
            string spellNameStr = PatchUtils.GetSpellName(source);

            Plugin.Logger.LogInfo($"[Damage] {attackerName} dealt {damage:F1} damage to {victimName} via {spellNameStr}");
        }
        catch (Exception e)
        {
            // Fail silently to not break the game's damage logic
            Plugin.Logger.LogError($"Error in ConsoleDamagePatch: {e}");
        }
    }
}
