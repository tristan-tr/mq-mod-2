using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace mq_mod_2.patches.balance.custom;

[HarmonyPatch(typeof(SpellHandler), nameof(SpellHandler.RefreshPrimary))]
public static class FlashFloodNerfPatch
{
    [HarmonyPrefix]
    public static bool Prefix(WizardController ___wc, Identity ___id, Player ___player, InputBase ___input)
    {
        Dictionary<SpellName, Cooldown> dictionary = (___wc.isClone ? ___input.cooldowns : ___player.cooldowns);
        Dictionary<SpellButton, SpellName> spell_library = PlayerManager.players[___id.owner].spell_library;
        
        if (spell_library.TryGetValue(SpellButton.Primary, out SpellName spellName))
        {
            if (dictionary.TryGetValue(spellName, out Cooldown cooldown))
            {
                // Reduce primary cooldown by 1 second instead of resetting it
                cooldown.cooldownTimer -= 1f;
            }
        }
        
        return false; // Skip the original method which resets the primary cooldown
    }
}
