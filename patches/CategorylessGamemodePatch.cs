using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(SelectionMenu))]
public static class CategorylessGamemodePatch
{
    public const SpellSelectionMode Categoryless = (SpellSelectionMode)6;

    [HarmonyPatch(nameof(SelectionMenu.ChangeSpellSelectionMode))]
    [HarmonyPrefix]
    static bool ChangeSpellSelectionModePrefix(SelectionMenu __instance, bool up)
    {
        SpellSelectionMode spellSelectionMode = PlayerManager.gameSettings.spellSelectionMode;
        // Total 7 modes now (0 to 6)
        spellSelectionMode = (SpellSelectionMode)(((int)spellSelectionMode + (up ? 1 : 6)) % 7);
        PlayerManager.gameSettings.spellSelectionMode = spellSelectionMode;
        
        // Use reflection to call private methods
        AccessTools.Method(typeof(SelectionMenu), "ShowSpellSelectionMode").Invoke(__instance, null);
        AccessTools.Method(typeof(SelectionMenu), "ResetPreset").Invoke(__instance, null);
        
        return false;
    }

    [HarmonyPatch("ShowSpellSelectionMode")]
    [HarmonyPrefix]
    static bool ShowSpellSelectionModePrefix(SelectionMenu __instance)
    {
        if (PlayerManager.gameSettings.spellSelectionMode == Categoryless)
        {
            __instance.spellSelectionText.text = "Categoryless";
            __instance.descriptionText.text = "All spells are available in every round. No categories, no limits.";
            
            if (__instance.online && Globals.online_lobby_canvas != null && Globals.online_lobby_canvas.uiCursor != null)
            {
                Globals.online_lobby_canvas.uiCursor.UpdateActions(0);
            }
            return false; // Skip original method to avoid exception
        }
        return true; // Run original for other modes
    }
}
