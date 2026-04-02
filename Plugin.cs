using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace mq_mod_2;

[BepInPlugin(Metadata.PLUGIN_GUID, Metadata.PLUGIN_NAME, Metadata.PLUGIN_VERSION)]
[BepInProcess("MageQuit.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {Metadata.PLUGIN_GUID} is loaded!");

        var harmony = new Harmony(Metadata.PLUGIN_GUID);
        harmony.PatchAll();

        foreach (var method in harmony.GetPatchedMethods())
        {
            Logger.LogInfo($"Successfully patched: {method.DeclaringType?.Name}.{method.Name}");
        }
    }
}
