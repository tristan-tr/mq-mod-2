using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches
{
    [HarmonyPatch(typeof(TimeBombObject))]
    public static class TimeBombSlowPatch
    {
        private const float SLOW_FACTOR = 0.6f;

        [HarmonyPatch(nameof(TimeBombObject.localCollision))]
        [HarmonyPostfix]
        public static void Postfix_localCollision(TimeBombObject __instance, GameObject enemy)
        {
            if (enemy == null) return;

            // Check if we already applied the slow to avoid double-stacking from the same bomb
            if (__instance.gameObject.GetComponent<TimeBombSlowHelper>() != null) return;

            WizardController wc = enemy.GetComponent<WizardController>();
            if (wc != null)
            {
                // Apply slow
                wc.MOVEMENT_SPEED *= SLOW_FACTOR;
                
                // Add a helper to restore speed when the bomb is destroyed
                var helper = __instance.gameObject.AddComponent<TimeBombSlowHelper>();
                helper.target = wc;
                helper.slowFactor = SLOW_FACTOR;
            }
        }

        [HarmonyPatch(nameof(TimeBombObject.rpcSpellObjectDeath))]
        [HarmonyPrefix]
        public static void Prefix_rpcSpellObjectDeath(TimeBombObject __instance)
        {
            var helper = __instance.gameObject.GetComponent<TimeBombSlowHelper>();
            if (helper != null)
            {
                helper.Restore();
            }
        }

        /// <summary>
        /// Helper component to ensure MOVEMENT_SPEED is restored when the TimeBombObject is destroyed.
        /// </summary>
        private class TimeBombSlowHelper : MonoBehaviour
        {
            public WizardController target;
            public float slowFactor;
            private bool restored = false;

            public void Restore()
            {
                if (!restored && target != null)
                {
                    target.MOVEMENT_SPEED /= slowFactor;
                    restored = true;
                }
            }

            private void OnDestroy()
            {
                Restore();
            }
        }
    }
}
