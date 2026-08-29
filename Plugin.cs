using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System;

namespace CUButt
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "seew.casualtiesunknown.cubutt";
        public const string PluginName = "CUButt";
        public const string PluginVersion = "1.1.3";

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

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);

            ConsoleCommands.Register();
            await VibrationManager.Initialize();

            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        private async void Update()
        {
            await VibrationManager.Tick();
        }

        private void LoadConfig()
        {
            WSMode = Config.Bind("General", "WholesomeMode", false, "Wholesome vibrations :3");
            GlobalMultiplier = Config.Bind("Vibration triggers", "GlobalMultiplier", 1f, "All vibrations are multiplied by this number.");
            PainMultiplier = Config.Bind("Vibration triggers", "PainMultiplier", 1f, "Pain vibrations are multiplied by this number.");
            Plugin.Log.LogInfo("Config loaded.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
