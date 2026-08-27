using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CUButt.Triggers
{

    [HarmonyPatch(typeof(Sound), "Play", new Type[] { typeof(string), typeof(Vector2), typeof(bool), typeof(bool), typeof(Transform), typeof(float), typeof(float), typeof(bool), typeof(bool)})]
    public class SoundCannonPatch
    {

        static void Prefix(string clip)
        {
            if (clip == "tinnitus")
            {
                List<float> tinnitusPattern = PatternGenerator.CreateImpulse(new List<VibrationCheckPoint>
                {
                    new VibrationCheckPoint(0.0f, 0.3f, InterpolationType.Smooth),
                    new VibrationCheckPoint(3f, 0.2f, InterpolationType.Smooth),
                    new VibrationCheckPoint(7f, 0.0f, InterpolationType.Linear)
                });
                VibrationManager.AddSpeedSequence(tinnitusPattern, 1);
            }
            if (clip == "sonarouchblocked")
            {
                List<float> tinnitusPattern = PatternGenerator.CreateImpulse(new List<VibrationCheckPoint>
                {
                    new VibrationCheckPoint(0.0f, 0.0f, InterpolationType.Smooth),
                    new VibrationCheckPoint(0.5f, 0.35f, InterpolationType.Smooth),
                    new VibrationCheckPoint(2f, 0f, InterpolationType.Linear)
                });
                VibrationManager.AddSpeedSequence(tinnitusPattern);
            }
            if (clip == "sonarouch")
            {
                List<float> tinnitusPattern = PatternGenerator.CreateImpulse(new List<VibrationCheckPoint>
                {
                    new VibrationCheckPoint(0.0f, 0.0f, InterpolationType.Smooth),
                    new VibrationCheckPoint(0.2f, 0.8f, InterpolationType.Smooth),
                    new VibrationCheckPoint(15f, 0f, InterpolationType.Linear)
                });
                VibrationManager.AddSpeedSequence(tinnitusPattern);
            }
            if (clip == "sonarmegaouchfixed")
            {
                List<float> tinnitusPattern = PatternGenerator.CreateImpulse(new List<VibrationCheckPoint>
                {
                    new VibrationCheckPoint(0.0f, 0.5f, InterpolationType.Smooth),
                    new VibrationCheckPoint(0.2f, 1f, InterpolationType.Smooth),
                    new VibrationCheckPoint(20f, 0f, InterpolationType.Linear)
                });
                VibrationManager.AddSpeedSequence(tinnitusPattern);
            }
        }
    }
}