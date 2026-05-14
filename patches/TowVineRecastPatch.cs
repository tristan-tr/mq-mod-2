using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class TowVineRecastPatch
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    public static void SpellManagerAwakePostfix(SpellManager __instance)
    {
        if (__instance.spell_table != null && __instance.spell_table.TryGetValue(SpellName.TowVine, out var towVine))
        {
            // Ensure AI considers it for recasting
            towVine.uses |= SpellUses.Custom;
        }
    }

    [HarmonyPatch(typeof(TowVine), nameof(TowVine.GetAiAim))]
    [HarmonyPrefix]
    public static bool GetAiAimPrefix(TowVine __instance, ref Vector3? __result, TargetComponent targetComponent, SpellUses use)
    {
        // For recasts, aim doesn't matter much as it pulls the wizard to the object/point, 
        // but we need to return a non-null value so the AI decides to cast it.
        if (__instance.reactivate > 1)
        {
            if (use == SpellUses.Custom)
            {
                __result = targetComponent.transform.forward;
            }
            else
            {
                __result = Vector3.forward;
            }
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(TowVineObject), nameof(TowVineObject.rpcPullToPoint))]
    [HarmonyPrefix]
    public static bool rpcPullToPointPrefix(TowVineObject __instance, SpellName ___spellName, UnitStatus ___wizard, PhysicsBody ___phys, SoundPlayer ___sp)
    {
        // If we just hit the ground, we want to enable optional recast instead of pulling immediately.
        if (___phys != null)
        {
            ___phys.velocity = Vector3.zero;
            if (___phys.rig != null)
            {
                ___phys.rig.isKinematic = true;
            }
        }

        var collider = __instance.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (__instance.anim != null)
        {
            __instance.anim.SetBool("Dig", true);
        }

        if (___sp != null)
        {
            ___sp.PlaySoundComponentInstantiate("event:/sfx/nature/tow-vine-dig", 5f);
        }

        if (__instance.dig != null)
        {
            Object.Instantiate(__instance.dig, __instance.transform.position, Globals.sideways);
        }

        __instance.state = TowVineObject.TowVineState.AttachedToPoint;
        __instance.deathTimer = Time.time + 9f;

        // Manually implement IsConnectedAndNotLocal logic since it is internal to the game
        bool isConnectedAndNotLocal = __instance.photonView != null && !__instance.photonView.isMine && PhotonNetwork.connected;
        if (!isConnectedAndNotLocal && ___wizard != null)
        {
            var spellHandler = ___wizard.GetComponent<SpellHandler>();
            if (spellHandler != null)
            {
                spellHandler.EnableRecast(___spellName);
            }
        }

        return false; // Skip original rpcPullToPoint logic
    }

    [HarmonyPatch(typeof(TowVineObject), nameof(TowVineObject.localCollision))]
    [HarmonyPostfix]
    public static void LocalCollisionPostfix(TowVineObject __instance, PhysicsBody ___phys)
    {
        // Ensure isKinematic is set even for ground/wall hits (go == null or not a unit)
        if (___phys != null && ___phys.rig != null)
        {
            ___phys.rig.isKinematic = true;
        }
    }
}
