using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace mq_mod_2.patches.balance.custom;

[HarmonyPatch(typeof(SpellHandler), "RefreshPrimary")]
public static class FlashFloodNerfPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SpellHandler __instance, WizardController ___wc, InputBase ___input, Player ___player, Identity ___id)
    {
        Dictionary<SpellName, Cooldown> dictionary = (___wc.isClone ? ___input.cooldowns : ___player.cooldowns);
        
        // Use PlayerManager to get the spell library for the current owner
        if (PlayerManager.players.ContainsKey(___id.owner))
        {
            Dictionary<SpellButton, SpellName> spell_library = PlayerManager.players[___id.owner].spell_library;
            if (spell_library.ContainsKey(SpellButton.Primary))
            {
                SpellName spellName = spell_library[SpellButton.Primary];
                if (dictionary.ContainsKey(spellName))
                {
                    Cooldown cd = dictionary[spellName];
                    
                    // Nerf: only reduce by 1 second instead of resetting
                    // If it's on cooldown, reduce the timer.
                    if (cd.cooldownTimer > Time.time)
                    {
                        cd.cooldownTimer -= 1f;
                        
                        // If it's now ready, reset both timers to 0.
                        if (cd.cooldownTimer <= Time.time)
                        {
                            cd.ResetCooldown();
                        }
                    }
                    // Else: if already available, we don't need to do anything.
                    // (The original code would have called ResetCooldown() which sets 0 to 0)
                }
            }
        }
        
        return false; // Skip the original method which resets the primary cooldown
    }
}
