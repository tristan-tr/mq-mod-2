using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class ChainLightningPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null && __instance.spell_table.TryGetValue(SpellName.ChainLightning, out Spell chainLightning))
        {
            chainLightning.windUp = 0.75f;
            
            if (chainLightning.additionalCasts != null)
            {
                foreach (var subSpell in chainLightning.additionalCasts)
                {
                    subSpell.windUp = 0.75f;
                }
            }
        }
        
        if (AiEventHandler.aiJeffCounters != null && AiEventHandler.aiJeffCounters.ContainsKey(SpellName.ChainLightning))
        {
            AiEventHandler.aiJeffCounters[SpellName.ChainLightning] = new float[] { 0.75f };
        }
    }
}
