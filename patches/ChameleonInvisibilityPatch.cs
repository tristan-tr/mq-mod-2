using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using mq_mod_2.utils;
using UnityEngine;

namespace mq_mod_2.patches;

/**
 * Makes your wizard transparent to yourself, and invisible to everyone else when playing online mode and hitting chameleon.
 * Patches ChameleonObject to use custom hide logic.
 */
[HarmonyPatch]
public class ChameleonInvisibilityPatch
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Material, Shader> _originalShaders = new();

    public static void CustomHideWizard(WizardStatus instance, bool hide)
    {
        if (Globals.online && PatchUtils.IsLocalPlayer(instance))
        {
            Renderer[] array = instance.materialColors.Keys.ToArray();
            foreach (var renderer in array)
            {
                if (renderer == null) continue;

                foreach (var mat in renderer.materials)
                {
                    mat.DOKill();
                    if (hide)
                    {
                        if (!_originalShaders.TryGetValue(mat, out _))
                        {
                            _originalShaders.Add(mat, mat.shader);
                        }

                        mat.shader = Shader.Find("Standard");

                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;

                        mat.DOFade(0.5f, 0.5f);
                    }
                    else
                    {
                        if (_originalShaders.TryGetValue(mat, out var originalShader))
                        {
                            mat.shader = originalShader;
                            
                            // If the original was Standard, we need to manually reset blend modes
                            if (originalShader.name == "Standard")
                            {
                                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                                mat.SetInt("_ZWrite", 1);
                                mat.DisableKeyword("_ALPHATEST_ON");
                                mat.DisableKeyword("_ALPHABLEND_ON");
                                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                mat.renderQueue = -1;
                            }
                        }
                    }
                }
            }

            if (!hide)
            {
                instance.ResetMaterials(0.5f);
                instance.HideWizard(false);
            }
        }
        else
        {
            instance.HideWizard(hide);
        }
    }

    public static void CustomHideStatusBar(WizardStatus instance, bool hide)
    {
        if (Globals.online && PatchUtils.IsLocalPlayer(instance) && hide)
        {
            return; // skip hiding
        }
        instance.HideStatusBar(hide);
    }

    [HarmonyPatch(typeof(ChameleonObject), nameof(ChameleonObject.rpcCopy))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> TranspileRpcCopy(IEnumerable<CodeInstruction> instructions)
    {
        return ReplaceHideCalls(instructions);
    }

    [HarmonyPatch(typeof(ChameleonObject), "OnDestroy")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> TranspileOnDestroy(IEnumerable<CodeInstruction> instructions)
    {
        return ReplaceHideCalls(instructions);
    }

    static IEnumerable<CodeInstruction> ReplaceHideCalls(IEnumerable<CodeInstruction> instructions)
    {
        var hideWizardMethod = AccessTools.Method(typeof(WizardStatus), nameof(WizardStatus.HideWizard));
        var customHideWizardMethod = AccessTools.Method(typeof(ChameleonInvisibilityPatch), nameof(CustomHideWizard));
        
        var hideStatusBarMethod = AccessTools.Method(typeof(WizardStatus), nameof(WizardStatus.HideStatusBar));
        var customHideStatusBarMethod = AccessTools.Method(typeof(ChameleonInvisibilityPatch), nameof(CustomHideStatusBar));

        return TranspilerUtils.ReplaceMethodCall(
            TranspilerUtils.ReplaceMethodCall(instructions, hideWizardMethod, customHideWizardMethod),
            hideStatusBarMethod, customHideStatusBarMethod
        );
    }
}
