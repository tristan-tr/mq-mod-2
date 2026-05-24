using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace mq_mod_2.patches.gamemodes;

[HarmonyPatch(typeof(SelectionMenu))]
public static class CategorylessGamemodePatch
{
    public const SpellSelectionMode Categoryless = (SpellSelectionMode)6;

    [HarmonyPatch(nameof(SelectionMenu.ChangeSpellSelectionMode))]
    [HarmonyPrefix]
    static bool ChangeSpellSelectionModePrefix(SelectionMenu __instance, bool up)
    {
        SpellSelectionMode spellSelectionMode = PlayerManager.gameSettings.spellSelectionMode;
        // Total 8 modes now (0 to 7)
        spellSelectionMode = (SpellSelectionMode)(((int)spellSelectionMode + (up ? 1 : 7)) % 8);
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
            return false;
        }
        if (PlayerManager.gameSettings.spellSelectionMode == RandomCategoryOrderPatch.RandomCategoryOrder)
        {
            __instance.spellSelectionText.text = "Random Category Order";
            __instance.descriptionText.text = "The order of draft categories is randomized each match. Spells are assigned to their correct slots.";
            
            if (__instance.online && Globals.online_lobby_canvas != null && Globals.online_lobby_canvas.uiCursor != null)
            {
                Globals.online_lobby_canvas.uiCursor.UpdateActions(0);
            }
            return false;
        }
        return true; // Run original for other modes
    }
}
