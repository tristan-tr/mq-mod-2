using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace mq_mod_2.patches.gamemodes;

[HarmonyPatch]
public static class RandomCategoryOrderPatch
{
    public const SpellSelectionMode RandomCategoryOrder = (SpellSelectionMode)7;
    private static int[] _shuffledCategories;
    private static int _lastSeed = -1;

    public static int GetCategoryForRound(int round)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != RandomCategoryOrder)
            return round;

        UpdateShuffle();
        if (_shuffledCategories == null || round < 0 || round >= _shuffledCategories.Length)
            return round;
        return _shuffledCategories[round];
    }

    private static void UpdateShuffle()
    {
        int seed = GetMatchSeed();
        if (seed == _lastSeed) return;
        _lastSeed = seed;

        _shuffledCategories = new int[] { 0, 1, 2, 3, 4, 5, 6 };
        System.Random rng = new System.Random(seed);
        // Fisher-Yates shuffle
        for (int i = _shuffledCategories.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = _shuffledCategories[i];
            _shuffledCategories[i] = _shuffledCategories[j];
            _shuffledCategories[j] = temp;
        }
        
        Plugin.Logger.LogInfo($"Shuffled categories for seed {seed}: {string.Join(", ", _shuffledCategories)}");
    }

    private static int GetMatchSeed()
    {
        if (Globals.selected_elements == null || Globals.selected_elements.Length == 0) return 0;
        int seed = 0;
        for (int i = 0; i < Math.Min(Globals.selected_elements.Length, 4); i++)
        {
            seed = seed * 31 + (int)Globals.selected_elements[i];
        }
        return seed;
    }

    // Redirect category in GameUtility.GetSpellByRoundAndElement
    [HarmonyPatch(typeof(GameUtility), nameof(GameUtility.GetSpellByRoundAndElement), typeof(Element), typeof(int))]
    [HarmonyPrefix]
    static void GetSpellByRoundAndElementPrefix(Element el, ref int round)
    {
        if (PlayerManager.gameSettings.spellSelectionMode == RandomCategoryOrder)
        {
            round = GetCategoryForRound(round);
        }
    }

    // Redirect category when adding spell to player
    [HarmonyPatch(typeof(SpellManager), nameof(SpellManager.AddSpellToPlayer))]
    [HarmonyPrefix]
    static void AddSpellToPlayerPrefix(ref SpellButton button)
    {
        if (PlayerManager.gameSettings.spellSelectionMode == RandomCategoryOrder)
        {
            // During draft, GetRound() returns the current draft round (0-6)
            int round = GameUtility.GetRound();
            if (round >= 0 && round <= 6)
            {
                button = (SpellButton)GetCategoryForRound(round);
            }
        }
    }

    // Fix VideoSpellPlayer UI
    [HarmonyPatch(typeof(VideoSpellPlayer), nameof(VideoSpellPlayer.ShowDraftLine))]
    [HarmonyPostfix]
    static void ShowDraftLinePostfix(VideoSpellPlayer __instance)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != RandomCategoryOrder) return;
        int round = GameUtility.GetRound();
        int category = GetCategoryForRound(round);
        if (category != round)
        {
            __instance.dock.GetChild(7 + category).gameObject.SetActive(true);
            __instance.dock.GetChild(7 + round).gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(VideoSpellPlayer), nameof(VideoSpellPlayer.SlideOut))]
    [HarmonyPostfix]
    static void SlideOutPostfix(VideoSpellPlayer __instance)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != RandomCategoryOrder) return;
        int round = GameUtility.GetRound();
        int category = GetCategoryForRound(round);
        if (category != round)
        {
            __instance.dock.GetChild(7 + category).gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(VideoSpellPlayer), nameof(VideoSpellPlayer.ShowExistingSpells))]
    [HarmonyPostfix]
    static void ShowExistingSpellsPostfix(VideoSpellPlayer __instance, int owner)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != RandomCategoryOrder) return;
        int round = GameUtility.GetRound();
        int category = GetCategoryForRound(round);
        if (category != round)
        {
            __instance.dock.GetChild(7 + category).GetComponent<DraftTimer>().Reset(owner);
        }
    }

    [HarmonyPatch(typeof(VideoSpellPlayer), "AdjustSpellCount")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> AdjustSpellCountTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].Calls(AccessTools.Method(typeof(GameUtility), nameof(GameUtility.GetRound))))
            {
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomCategoryOrderPatch), nameof(GetCategoryForRound))));
                i++;
            }
        }
        return codes;
    }

    [HarmonyPatch(typeof(VideoSpellPlayer), nameof(VideoSpellPlayer.SlideIn))]
    [HarmonyPostfix]
    static void SlideInPostfix(VideoSpellPlayer __instance)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != RandomCategoryOrder) return;
        int category = GetCategoryForRound(GameUtility.GetRound());
        var buttonNames = (string[])AccessTools.Field(typeof(VideoSpellPlayer), "buttonNames").GetValue(__instance);
        __instance.buttonIcon.buttonName = buttonNames[category];
    }

    [HarmonyPatch(typeof(SpellManager), nameof(SpellManager.GetDraftTargetSpellIndex))]
    [HarmonyPrefix]
    static bool GetDraftTargetSpellIndexPrefix(SpellManager __instance, int[] spellCounts, int currentPlayerNumber, ref int __result)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != RandomCategoryOrder) return true;

        List<SpellName> list = new List<SpellName>();
        List<int> list2 = new List<int>();
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int round = GameUtility.GetRound();
        int category = GetCategoryForRound(round);

        for (int i = 0; i < 4; i++)
        {
            if (spellCounts[i] > 0)
            {
                num2++;
                Element mappedElement = VideoSpellPlayer.GetMappedElement(i);
                // Our patch for GetSpellByRoundAndElement will handle the category mapping
                Spell spell = GameUtility.GetSpellByRoundAndElement(mappedElement, round);
                if (spell == null) continue;
                
                SpellName spellName = spell.spellName;
                list.Add(spellName);
                
                int weightIndex = -1;
                if (__instance.ai_draft_priority.TryGetValue((SpellButton)category, out List<SpellName> priorities))
                {
                    weightIndex = priorities.IndexOf(spellName);
                }
                
                if (weightIndex == -1) weightIndex = 9; // Safety default
                
                int[] weights = (int[])AccessTools.Field(typeof(SpellManager), "ai_draft_weights").GetValue(__instance);
                num += weights[Math.Min(Math.Max(0, weightIndex), weights.Length - 1)];

                if (mappedElement == Element.Ice)
                {
                    if (PlayerManager.players.TryGetValue(currentPlayerNumber, out global::Player p))
                    {
                        foreach (SpellName ownedSpellName in p.spell_library.Values)
                        {
                            if (__instance.spell_table.TryGetValue(ownedSpellName, out Spell s) && s.element == Element.Ice)
                            {
                                num += 60;
                            }
                        }
                    }
                }
                list2.Add(num);
                num3 = i;
            }
        }

        if (num <= 0)
        {
            __result = num3;
            return false;
        }

        int num4 = UnityEngine.Random.Range(0, num);
        for (int j = 0; j < num2; j++)
        {
            if (num4 < list2[j])
            {
                if (__instance.spell_table.TryGetValue(list[j], out Spell s))
                {
                    Element element = s.element;
                    __result = new int[] { 0, 1, 2, 3 }.First(x => VideoSpellPlayer.GetMappedElement(x) == element);
                    return false;
                }
            }
        }
        __result = num3;
        return false;
    }

    [HarmonyPatch(typeof(PlayerSelection), "ErrorCheck")]
    [HarmonyPostfix]
    static void ErrorCheckPostfix(PlayerSelection __instance, ref bool __result)
    {
        if (__result) return;
        if (PlayerManager.gameSettings.spellSelectionMode == RandomCategoryOrder)
        {
            if (PlayerManager.gameSettings.elements.Take(GamePreferences.current.prefs.LastUnlockedIndex + 5).Count(x => x != ElementInclusionMode.Banned) < 4)
            {
                __instance.errorText.text = "Too many elements banned for that spell selection mode.";
                __instance.errorText.color = (Color)AccessTools.Field(typeof(PlayerSelection), "errorColor").GetValue(__instance);
                __result = true;
            }
        }
    }

    // Defensive patch to prevent crash in RoundRecapManager.Update
    [HarmonyPatch(typeof(RoundRecapManager), "Update")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> RoundRecapUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        
        for (int i = 0; i < codes.Count; i++)
        {
            // Look for any Enumerable.First call with 2 parameters (the source and the predicate)
            if (codes[i].opcode == OpCodes.Call && codes[i].operand is System.Reflection.MethodInfo method && 
                method.DeclaringType == typeof(Enumerable) && method.Name == "First" && method.GetParameters().Length == 2)
            {
                // Find the corresponding FirstOrDefault method using reflection
                var genericArgs = method.GetGenericArguments();
                var firstOrDefault = typeof(Enumerable).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "FirstOrDefault" && 
                                        m.IsGenericMethod && 
                                        m.GetParameters().Length == 2)
                    ?.MakeGenericMethod(genericArgs);

                if (firstOrDefault != null)
                {
                    codes[i].operand = firstOrDefault;
                    Plugin.Logger.LogInfo("Successfully replaced Enumerable.First with FirstOrDefault in RoundRecapManager.Update");
                }
                else
                {
                    Plugin.Logger.LogWarning("Failed to find corresponding Enumerable.FirstOrDefault method via reflection.");
                }
            }
        }
        return codes;
    }
}
