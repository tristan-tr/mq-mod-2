using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.balance.custom
{
    [HarmonyPatch(typeof(TetherballObject), nameof(TetherballObject.localImpact))]
    public static class TetherballCooldownPatch
    {
        public static void Postfix(GameObject[] enemies)
        {
            if (enemies == null) return;

            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;

                // Only apply on the client that owns the wizard to avoid double-applying in online mode
                PhotonView pv = enemy.GetComponent<PhotonView>();
                if (Globals.online && pv != null && !pv.isMine && PhotonNetwork.connected)
                {
                    continue;
                }

                Identity id = enemy.GetComponent<Identity>();
                WizardController wc = enemy.GetComponent<WizardController>();

                if (wc != null && !wc.isClone && id != null && PlayerManager.players.TryGetValue(id.owner, out Player player))
                {
                    foreach (Cooldown cd in player.cooldowns.Values)
                    {
                        cd.cooldownTimer = Mathf.Max(cd.cooldownTimer, Time.time) + 2f;
                    }
                }
            }
        }
    }
}
