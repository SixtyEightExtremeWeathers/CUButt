using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;

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
        internal static Dictionary<string, ConfigEntry<float>> Multipliers = new Dictionary<string, ConfigEntry<float>>();

        private Harmony _harmony;

        private async void Awake()
        {
            Log = Logger;
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            LoadThreads();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);

            ConsoleCommands.Register();
            await VibrationManager.Initialize();

            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        private void Start()
        {
            LoadConfig();
        }

        private async void Update()
        {
            await VibrationManager.Tick();
        }

        private void LoadConfig()
        {
            WSMode = Config.Bind("General", "WholesomeMode", false, "Wholesome vibrations :3");
            GlobalMultiplier = Config.Bind("Vibration triggers", "GlobalMultiplier", 1f, "All vibrations are multiplied by this number.");

            Multipliers = new Dictionary<string, ConfigEntry<float>>();

            foreach (var threadKey in ThreadManager.Threads.Keys)
            {   
                var multiplierConfig = Config.Bind(
                    "Vibration triggers",
                    $"{char.ToUpper(threadKey[0]) + threadKey.Substring(1)}Multiplier",
                    1f,
                    $"{char.ToUpper(threadKey[0]) + threadKey.Substring(1)} vibrations are multiplied by this number.");

                Multipliers.Add(threadKey, multiplierConfig);
            }

            Plugin.Log.LogInfo("Config loaded.");
        }

        private void LoadThreads()
        {
            ThreadManager.AddThread("pain", 0);
            ThreadManager.AddThread("wholesome", 99);
            ThreadManager.AddThread("death", 100);
            ThreadManager.AddThread("bleeding", 0);
            ThreadManager.AddThread("radiation", 1);
            ThreadManager.AddThread("ecg", 2);
            ThreadManager.AddThread("stamina", 0.142f);
            ThreadManager.AddThread("painspike", 0);
            ThreadManager.AddThread("earthquake", 0);
            ThreadManager.AddThread("electricity", 0);
            ThreadManager.AddThread("falling", 0.577f);
            ThreadManager.AddThread("explosion", 0.343f);
            ThreadManager.AddThread("soundcannon", 0);
            ThreadManager.AddThread("soundcannon_blocked", 0.928f);

        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            CUButtSettingsWindow.Cleanup();
        }

        private void OnGUI()
        {
            CUButtSettingsWindow.Draw();
        }
    }
}
