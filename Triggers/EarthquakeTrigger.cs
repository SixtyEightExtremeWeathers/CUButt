using HarmonyLib;
using UnityEngine;

namespace CUButt.Triggers
{
    [HarmonyPatch(typeof(global::WorldGeneration), "Update")]
    public static class Earthquake
    {
        public static float _earthquakeIntensity;
        private static float EarthQuakerMultiplier => 1;

        private static void Postfix(global::WorldGeneration __instance)
        {
            _earthquakeIntensity = Mathf.Clamp01(__instance.earthquakeIntensity);
        }

        public static void Add()
        {
            if (_earthquakeIntensity > 0.01)
            {
                float quake = Mathf.Lerp(0.12f, 0.48f, Timer.Value) * _earthquakeIntensity * EarthQuakerMultiplier;
                VibrationManager.Add(quake);
            }
        }
    }
}
