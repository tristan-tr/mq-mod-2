using HarmonyLib;
using System;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class ScorchPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null && __instance.spell_table.TryGetValue(SpellName.Scorch, out Spell scorch))
        {
            // After 1st scorch: delay increased by 1x (2x total)
            float baseDelay = scorch.windUp + scorch.windDown;
            scorch.windDown = (baseDelay * 2f) - scorch.windUp;
            scorch.cooldown *= 2f;

            if (scorch.additionalCasts != null)
            {
                // Increase window for 2nd shot
                if (scorch.additionalCasts.Length > 0)
                {
                    scorch.additionalCasts[0].activationWindow *= 2f;
                }

                for (int i = 0; i < scorch.additionalCasts.Length; i++)
                {
                    // i=0 is 2nd shot, i=1 is 3rd shot, i=2 is 4th shot
                    
                    if (i < scorch.additionalCasts.Length - 1)
                    {
                        // Increase delay AFTER this shot (Shot i+2)
                        // After 2nd scorch: 3x total. After 3rd scorch: 4x total.
                        float multiplier = i + 3f; 
                        
                        float currentDelay = scorch.additionalCasts[i].windUp + scorch.additionalCasts[i].windDown;
                        scorch.additionalCasts[i].windDown = (currentDelay * multiplier) - scorch.additionalCasts[i].windUp;
                        scorch.additionalCasts[i].cooldown *= multiplier;

                        // Increase the window for the NEXT shot
                        if (i + 1 < scorch.additionalCasts.Length)
                        {
                            scorch.additionalCasts[i + 1].activationWindow *= multiplier;
                        }
                    }
                    else
                    {
                        // Last shot (Shot 4 if length is 3)
                        // "The final cooldown if you shoot all 4 should be the same"
                    }
                }
            }
        }
    }
}
