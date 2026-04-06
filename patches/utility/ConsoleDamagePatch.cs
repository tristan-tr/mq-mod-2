using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.utility;

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
            .Select(t => AccessTools.Method(t, "rpcApplyDamage", new Type[] { typeof(float), typeof(int), typeof(int) }))
            .Where(m => m != null);
    }

    [HarmonyPrefix]
    static void Prefix(object __instance, float damage, int owner, int source)
    {
        try
        {
            string victimName = "Unknown";
            if (__instance is MonoBehaviour mb)
            {
                var identity = mb.GetComponent<Identity>();
                if (identity != null)
                {
                    if (PlayerManager.players.TryGetValue(identity.owner, out var victimPlayer))
                    {
                        victimName = victimPlayer.name;
                    }
                    else
                    {
                        victimName = $"Player {identity.owner}";
                    }
                }
                else
                {
                    victimName = mb.gameObject.name;
                }
            }

            string attackerName = "Unknown";
            if (PlayerManager.players.TryGetValue(owner, out var attackerPlayer))
            {
                attackerName = attackerPlayer.name;
            }
            else if (owner == 0)
            {
                attackerName = "Environment/Self";
            }
            else
            {
                attackerName = $"Player {owner}";
            }

            string spellNameStr = "Unknown Source";
            if (Enum.IsDefined(typeof(SpellName), source))
            {
                spellNameStr = ((SpellName)source).ToString();
            }
            else if (source == -1)
            {
                spellNameStr = "Collision/Other";
            }
            else
            {
                spellNameStr = $"Source {source}";
            }

            Plugin.Logger.LogInfo($"[Damage] {attackerName} dealt {damage:F1} damage to {victimName} via {spellNameStr}");
        }
        catch (Exception e)
        {
            // Fail silently to not break the game's damage logic
            Plugin.Logger.LogError($"Error in ConsoleDamagePatch: {e}");
        }
    }
}
