using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MqMod.Patches.Balance.Custom
{
    [HarmonyPatch]
    public static class FlashFloodDesyncPatch
    {
        private class FlashFloodRecord
        {
            public float timestamp;
            public Vector3 oldPosition;
        }

        private static ConditionalWeakTable<GameObject, FlashFloodRecord> records = new ConditionalWeakTable<GameObject, FlashFloodRecord>();

        [HarmonyPatch(typeof(FlashFloodObject), "localSpellObjectStart")]
        [HarmonyPrefix]
        public static void FlashFloodStartPrefix(GameObject wizard)
        {
            if (wizard == null) return;
            
            var record = records.GetOrCreateValue(wizard);
            record.timestamp = Time.time;
            record.oldPosition = wizard.transform.position;
        }

        [HarmonyPatch(typeof(WizardStatus), "rpcApplyDamage")]
        [HarmonyPrefix]
        public static bool rpcApplyDamagePrefix(WizardStatus __instance, float damage, int owner, int source)
        {
            if (!__instance.photonView.isMine) return true;

            if (records.TryGetValue(__instance.gameObject, out var record))
            {
                // The window is 0.3s to 0.4s after cast start.
                // Since localSpellObjectStart happens at 0.3s (end of windup),
                // we check for 0.1s after that.
                if (Time.time - record.timestamp < 0.11f) // 0.11 to account for precision
                {
                    if (IsNearOldPosition(__instance.transform.position, record.oldPosition, owner))
                    {
                        return false; // Ignore damage
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PhysicsBody), "rpcAddForce")]
        [HarmonyPrefix]
        public static bool rpcAddForcePrefix(PhysicsBody __instance, Vector3 impulse)
        {
            PhotonView pv = __instance.GetComponent<PhotonView>();
            if (pv == null || !pv.isMine) return true;

            if (records.TryGetValue(__instance.gameObject, out var record))
            {
                if (Time.time - record.timestamp < 0.11f)
                {
                    if (IsNearOldPosition(__instance.transform.position, record.oldPosition, -1))
                    {
                        return false; // Ignore knockback
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PhysicsBody), "rpcSetVelocity")]
        [HarmonyPrefix]
        public static bool rpcSetVelocityPrefix(PhysicsBody __instance, Vector3 velocity)
        {
            PhotonView pv = __instance.GetComponent<PhotonView>();
            if (pv == null || !pv.isMine) return true;

            if (records.TryGetValue(__instance.gameObject, out var record))
            {
                if (Time.time - record.timestamp < 0.11f)
                {
                    if (IsNearOldPosition(__instance.transform.position, record.oldPosition, -1))
                    {
                        return false; // Ignore knockback
                    }
                }
            }
            return true;
        }

        private static bool IsNearOldPosition(Vector3 currentPos, Vector3 oldPos, int owner)
        {
            // Only apply if the blink distance was significant
            if (Vector3.Distance(currentPos, oldPos) < 5f) return false;

            bool foundNearOld = false;
            bool foundNearNew = false;

            SpellObject[] spellObjects = Object.FindObjectsOfType<SpellObject>();
            foreach (var spellObj in spellObjects)
            {
                if (spellObj == null) continue;
                
                if (owner != -1)
                {
                    Identity id = spellObj.GetComponent<Identity>();
                    if (id == null || id.owner != owner) continue;
                }

                float distToOld = Vector3.Distance(spellObj.transform.position, oldPos);
                float distToNew = Vector3.Distance(spellObj.transform.position, currentPos);

                if (distToOld < 4f) foundNearOld = true;
                if (distToNew < 4f) foundNearNew = true;
            }

            // If we found a projectile near the old position but NOT near the current position,
            // it's almost certainly a desync hit.
            return foundNearOld && !foundNearNew;
        }
    }
}
