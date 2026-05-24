using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace mq_mod_2.patches.balance.mature;

[HarmonyPatch]
public static class SunderPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null && __instance.spell_table.TryGetValue(SpellName.Sunder, out Spell sunder))
        {
            sunder.windUp = 0.7f;
            sunder.windDown = 0.5f;
            
            // Also update sub-spells if any (just in case)
            if (sunder.additionalCasts != null)
            {
                foreach (var subSpell in sunder.additionalCasts)
                {
                    subSpell.windUp = 0.4f;
                    subSpell.windDown = 0.7f;
                }
            }
        }
        
        // Update AI counters
        if (AiEventHandler.aiJeffCounters != null && AiEventHandler.aiJeffCounters.ContainsKey(SpellName.Sunder))
        {
            AiEventHandler.aiJeffCounters[SpellName.Sunder] = new float[] { 0.4f };
        }
    }
}
