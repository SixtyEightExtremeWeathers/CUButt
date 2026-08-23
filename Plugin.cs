using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace CUButt
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "seew.casualtiesunknown.cubutt";
        public const string PluginName = "CUButt";
        public const string PluginVersion = "1.2.0";

        internal static ManualLogSource Log;

        internal static ConfigEntry<float> GlobalMultiplier;
        internal static ConfigEntry<bool> WSMode;
        internal static ConfigEntry<float> PainMultiplier; 

        private Harmony _harmony;

        private async void Awake()
        {
            Log = Logger;
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            LoadConfig();
            await VibrationManager.Initialize();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);

            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        private void Update()
        {
            Timer.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
            VibrationManager.Tick();
        }

        private void LoadConfig()
        {
            WSMode = Config.Bind("General", "Wholesome mode", false, "Wholesome vibrations :3");
            GlobalMultiplier = Config.Bind("Vibration triggers", "Global", 1f, "Any vibration scale.");
            PainMultiplier = Config.Bind("Vibration triggers", "Pain multiplier", 1f, "Multiple vibration by pain.");
            Plugin.Log.LogInfo("Config loaded.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
