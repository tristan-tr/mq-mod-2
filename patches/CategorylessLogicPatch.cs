using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class CategorylessLogicPatch
{
    private static Dictionary<Element, SpellName> _currentRoundSpells = new();
    private static int _lastCachedRound = -1;
    private static int _lastCachedMatchSeed = -1;

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

    private static void UpdateCache(int round)
    {
        int matchSeed = GetMatchSeed();
        if (_lastCachedRound == round && _lastCachedMatchSeed == matchSeed) return;
        
        _currentRoundSpells.Clear();
        _lastCachedRound = round;
        _lastCachedMatchSeed = matchSeed;

        foreach (Element el in Enum.GetValues(typeof(Element)))
        {
            if (el == Element.None) continue;
            
            // Deterministic seed for this match, round, and element
            int seed = matchSeed + round * 1000 + (int)el;
            System.Random rng = new System.Random(seed);
            
            var spells = Globals.spell_manager.spell_table.Values
                .Where(s => s.element == el && s.spellButton != SpellButton.None)
                .OrderBy(s => s.spellName.ToString())
                .ToList();
            
            if (spells.Count > 0)
            {
                _currentRoundSpells[el] = spells[rng.Next(spells.Count)].spellName;
            }
        }
    }

    [HarmonyPatch(typeof(GameUtility), nameof(GameUtility.GetSpellByRoundAndElement), typeof(Element), typeof(int))]
    [HarmonyPrefix]
    static bool GetSpellByRoundAndElementPrefix(Element el, int round, ref Spell __result)
    {
        if (round < 0 || round > 6) return true;
        if (PlayerManager.gameSettings.spellSelectionMode == CategorylessGamemodePatch.Categoryless)
        {
            UpdateCache(round);
            if (_currentRoundSpells.TryGetValue(el, out SpellName spellName))
            {
                __result = Globals.spell_manager.spell_table[spellName];
                return false;
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(SpellManager), nameof(SpellManager.GetDraftTargetSpellIndex))]
    [HarmonyPrefix]
    static bool GetDraftTargetSpellIndexPrefix(SpellManager __instance, int[] spellCounts, int currentPlayerNumber, ref int __result)
    {
        if (PlayerManager.gameSettings.spellSelectionMode != CategorylessGamemodePatch.Categoryless) return true;

        List<SpellName> list = new List<SpellName>();
        List<int> list2 = new List<int>();
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int round = GameUtility.GetRound();
        UpdateCache(round);

        for (int i = 0; i < 4; i++)
        {
            if (spellCounts[i] > 0)
            {
                num2++;
                Element mappedElement = VideoSpellPlayer.GetMappedElement(i);
                if (_currentRoundSpells.TryGetValue(mappedElement, out SpellName spellName))
                {
                    list.Add(spellName);
                    Spell spell = __instance.spell_table[spellName];
                    
                    // Use the spell's actual button for weighting, or default to 0 if not in list
                    int weightIndex = __instance.ai_draft_priority[spell.spellButton].IndexOf(spellName);
                    if (weightIndex == -1) weightIndex = 9; // Lowest weight if not found

                    // Access private field ai_draft_weights via reflection for safety, 
                    // though it's likely accessible if we are in the same assembly or using public.
                    // Based on decompiled code it's private.
                    int[] weights = (int[])AccessTools.Field(typeof(SpellManager), "ai_draft_weights").GetValue(__instance);
                    num += weights[Math.Min(weightIndex, weights.Length - 1)];

                    if (mappedElement == Element.Ice)
                    {
                        foreach (SpellName ownedSpellName in PlayerManager.players[currentPlayerNumber].spell_library.Values)
                        {
                            if (__instance.spell_table[ownedSpellName].element == Element.Ice)
                            {
                                num += 60;
                            }
                        }
                    }
                    list2.Add(num);
                    num3 = i;
                }
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
                Element element = __instance.spell_table[list[j]].element;
                __result = new int[] { 0, 1, 2, 3 }.First(x => VideoSpellPlayer.GetMappedElement(x) == element);
                return false;
            }
        }
        __result = num3;
        return false;
    }
}
