using System.Linq;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.qol;

/**
 * Makes your wizard transparent to yourself, and invisible to everyone else when playing online mode and hitting chameleon.
 */
public class ChameleonInvisibilityPatch
{
    static bool HasHitChameleon(WizardStatus wizardStatus)
    {
        int ownerId = wizardStatus.GetComponent<Identity>().owner;

        // Check if the owner has any Chameleon entries
        if (Globals.spell_manager.currentChameleon.TryGetValue(ownerId, out var chameleonDict))
        {
            return chameleonDict.ContainsKey(SpellName.Chameleon);
        }

        return false;
    }

    // should not be true for clones since default chameleon behaviour does not turn clones invis
    static bool IsLocalPlayer(WizardStatus wizardStatus)
    {
        int localOwner = wizardStatus.GetComponent<Identity>().localOwner;
        int? only_local_player_id = BattleManager.only_local_player_id;

        if (only_local_player_id.HasValue && localOwner == only_local_player_id.Value)
        {
            // this wizardstatus belongs to the localplayer
            
            // exclude clones
            var wc = wizardStatus.GetComponent<WizardController>();
            if (wc != null && !wc.isClone)
            {
                return true;
            }
        }

        return false;
    }

    static bool IsPatchActive(WizardStatus wizardStatus)
    {
        return Globals.online && IsLocalPlayer(wizardStatus) && HasHitChameleon(wizardStatus);
    }

    static float GetAlpha(bool hide)
    {
        return hide ? 0.5f : 1.0f;
    }
    
    [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.HideWizard))]
    class HideWizardPatch
    {
        static bool Prefix(ref bool hide, WizardStatus __instance)
        {
            if (IsPatchActive(__instance))
            {
                // Make transparent/untransparent instead
                if (hide)
                {
                    Renderer[] array = __instance.materialColors.Keys.ToArray();
                    foreach (var renderer in array)
                    {
                        if(renderer == null) continue;

                        foreach (var mat in renderer.materials)
                        {
                            mat.shader = Shader.Find("Standard");
                            
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.renderQueue = 3000;
                            
                            mat.DOFade(GetAlpha(hide), 0.5f);
                        }
                    }
                }
                else
                {
                    __instance.ResetMaterials(0.5f);
                }

                return !hide; // skip making invisible, otherwise run
            }

            return true; // dont skip
        }
    }

    [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.HideStatusBar))]
    class HideStatusBarPatch
    {
        static bool Prefix(ref bool hide, WizardStatus __instance)
        {
            if (IsPatchActive(__instance))
            {
                // never hide status bar
                return !hide; // skip making invisible, otherwise run
            }
            
            return true; // dont skip
        }
    }
}