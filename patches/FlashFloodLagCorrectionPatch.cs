using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(FlashFloodObject), "localSpellObjectStart")]
public static class FlashFloodLagCorrectionPatch
{
    [HarmonyPrefix]
    public static bool Prefix(FlashFloodObject __instance, int owner, int spellIndex, GameObject wizard, SpellName spellName)
    {
        if (wizard == null) return true;

        NetworkedWizard component = wizard.GetComponent<NetworkedWizard>();
        
        // Access private fields using Traverse or AccessTools
        var traverse = Traverse.Create(__instance);
        PhysicsBody wizardPhys = wizard.GetComponent<PhysicsBody>();
        traverse.Field("phys").SetValue(wizardPhys);
        
        RaycastHit[] raycastHits = traverse.Field("raycastHits").GetValue<RaycastHit[]>();

        CrystalObject.GetOutOfPreserveCrystals(owner, wizard.transform);
        
        wizard.GetComponent<SpellHandler>().RefreshPrimary();

        if (spellIndex < 0)
        {
            // LAG CORRECTION: Use __instance.transform.position instead of wizard.transform.position
            // __instance.transform.position is the position where the spell was cast, 
            // which is synchronized across all clients.
            Vector3 castPosition = __instance.transform.position;
            
            Vector3 vector = castPosition + __instance.transform.rotation * Vector3.forward * 20f + Vector3.up * 30f;
            RaycastHit? raycastHit = GameUtility.Raycast(vector, Vector3.down, raycastHits, float.PositiveInfinity, __instance.isGround);
            if (raycastHit.HasValue)
            {
                vector += Vector3.down * (raycastHit.Value.distance - 1.3f);
            }

            Object.Instantiate(__instance.fromPrefab, castPosition, Globals.sideways);
            Object.Instantiate(__instance.toPrefab, vector, Globals.sideways);

            if (!FlashFlood.lastPosition.ContainsKey(wizard))
            {
                FlashFlood.lastPosition[wizard] = new Dictionary<SpellName, Vector3>();
            }
            
            // LAG CORRECTION: Store the synchronized cast position
            FlashFlood.lastPosition[wizard][spellName] = castPosition;
            
            if (component != null)
            {
                component.SetCorrectPosition(vector);
            }
        }
        else
        {
            // For recast, we still use wizard's current position for the "from" effect
            Object.Instantiate(__instance.fromPrefab, wizard.transform.position, Globals.sideways);
            
            Vector3 returnPos = FlashFlood.lastPosition.ContainsKey(wizard) && FlashFlood.lastPosition[wizard].ContainsKey(spellName) 
                ? FlashFlood.lastPosition[wizard][spellName] 
                : wizard.transform.position; // Fallback if not found

            Object.Instantiate(__instance.toPrefab, returnPos, Globals.sideways);
            
            if (component != null)
            {
                component.SetCorrectPosition(returnPos);
            }
        }

        var aiEventHandler = Traverse.Create(typeof(Globals)).Field("ai_event_handler").GetValue<AiEventHandler>();
        if (aiEventHandler != null)
        {
            aiEventHandler.OnTeleport(owner);
        }
        Object.Destroy(__instance.gameObject, 0.5f);

        return false; // Skip original method
    }
}
