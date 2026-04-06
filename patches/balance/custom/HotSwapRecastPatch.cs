using HarmonyLib;
using UnityEngine;

// ai slop

[HarmonyPatch(typeof(HotSwapObject))]
public static class HotSwapRecastPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Teleport")]
    public static bool TeleportPrefix(HotSwapObject __instance)
    {
        // If we are still a projectile, it means we just hit the ground.
        // Instead of teleporting immediately, we want to enable recast.
        if (__instance.state == HotSwapObject.HotSwapState.Projectile)
        {
            __instance.localCollision(null);
            return false; // Skip original Teleport
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("localCollision")]
    public static void LocalCollisionPostfix(HotSwapObject __instance, GameObject go, PhysicsBody ___phys)
    {
        // Ensure state and isKinematic are set even if go is null (ground hit)
        __instance.state = HotSwapObject.HotSwapState.AttachedToObject;
        
        if (___phys != null && ___phys.rig != null)
        {
            ___phys.rig.isKinematic = true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("FixedUpdate")]
    public static void FixedUpdatePrefix(HotSwapObject __instance, out bool __state, Transform ___target, UnitStatus ___enemyStatus)
    {
        __state = false;
        // If we hit the ground and are waiting for recast, state is AttachedToObject and target is null.
        // FixedUpdate would normally set deathTimer to 0 if target is null in this state.
        // We temporarily change the state to skip that logic.
        if (__instance.state == HotSwapObject.HotSwapState.AttachedToObject)
        {
            if (___target == null && ___enemyStatus == null)
            {
                __instance.state = HotSwapObject.HotSwapState.TeleportImmediately; // Switch is skip-only for this state
                __state = true;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("FixedUpdate")]
    public static void FixedUpdatePostfix(HotSwapObject __instance, bool __state)
    {
        if (__state)
        {
            __instance.state = HotSwapObject.HotSwapState.AttachedToObject;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("Swap")]
    public static bool SwapPrefix(HotSwapObject __instance, float force, Transform ___target, UnitStatus ___enemyStatus)
    {
        // If we recast and there's no target, we perform the teleport.
        if (___target == null)
        {
            if (___enemyStatus == null) // Ground hit
            {
                __instance.Teleport();
            }
            // If enemyStatus is NOT null, it means we were attached to an enemy that is now gone.
            // In that case, we don't want to teleport or swap.
            return false;
        }
        return true;
    }
}
