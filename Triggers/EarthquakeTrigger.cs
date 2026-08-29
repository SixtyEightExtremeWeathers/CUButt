using HarmonyLib;
using UnityEngine;

namespace CUButt.Triggers
{
    [HarmonyPatch(typeof(global::WorldGeneration), "Update")]
    public static class Earthquake
    {
        private static float EarthQuakeMultiplier => 1;

        private static void Postfix(global::WorldGeneration __instance)
        {
            float EarthquakeIntensity = Mathf.Clamp01(__instance.earthquakeIntensity);
            if (EarthquakeIntensity > 0.01)
            {
                float quake = Mathf.Lerp(0.12f, 0.48f, Time.time) * EarthquakeIntensity * EarthQuakeMultiplier;
                VibrationManager.SetSpeed(quake, "earthquake");
            }
        }
    }
}
