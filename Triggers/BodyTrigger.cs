using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace CUButt.Triggers
{
    [HarmonyPatch(typeof(global::Body), "HandleBody")]
    internal static class Body
    {
        private static float PainMultiplier => 0.60f * Plugin.PainMultiplier.Value;


        private static float _lastPain;
        public static float _happiness;
        public static float _opiate;


        private static void Postfix(global::Body __instance)
        {
            bool Alive = __instance.alive;
            float Pain = Mathf.Clamp(__instance.averagePain, 0.0f, 100.0f);
            float BleedingLevel = CalculateBleedingLevel(__instance);
            float RadiationLevel = CalculateRadiationLevel(__instance);
            _happiness = __instance.happiness;
            _opiate = __instance.opiateHappiness;
            float PainDelta = Pain - _lastPain;
            float PainSpikeMultiplier = 1f;


            if (!Alive)
            {
                VibrationManager.SetSpeed(0.0f, 100);
            }

            if (__instance.isCriticallyDying)
            {
                VibrationManager.SetSpeed(VibrationManager.SharpPulse(Timer.Time, 3.4f), 1);
            }

            VibrationManager.SetSpeed(Mathf.Clamp01((Pain-0.5f*Timer.TimeSincePain)/100f) * PainMultiplier * PainSpikeMultiplier);

            if (BleedingLevel > 0.0f)
            {
                float bleedPulse = VibrationManager.SmoothPulse(Timer.Time, 1f);
                float bleedAmplitude = Mathf.Lerp(0.08f, 0.40f, BleedingLevel);
                VibrationManager.SetSpeed(bleedPulse * bleedAmplitude);
            }

            if (__instance.fibrillationProgress > 1.0f || __instance.fibrillationForced || __instance.inCardiacArrest)
            {
                VibrationManager.SetSpeed(VibrationManager.SharpPulse(Timer.Time, 3.4f) * 0.72f);
            }

            if (RadiationLevel > 0.1f)
            {
                float radiation = Mathf.Lerp(0.58f, 1.0f, Timer.Value);
                radiation *= Mathf.Lerp(0.70f, 1.0f, RadiationLevel);
                VibrationManager.SetSpeed(radiation);
            }


            if (PainDelta >= 5f)
            {
                Timer.ResetPainTimer();
                List<float> painSpikePattern = PatternGenerator.CreateImpulse(new List<VibrationCheckPoint>
                {
                    new VibrationCheckPoint(0.0f, 0.0f, InterpolationType.Linear),
                    new VibrationCheckPoint(0.2f, Pain*1.5f/100f, InterpolationType.Linear),
                    new VibrationCheckPoint(0.4f, Pain*1.5f/100f, InterpolationType.Smooth),
                    new VibrationCheckPoint(0.7f, Pain*0.8f/100f, InterpolationType.Linear)
                });
                VibrationManager.AddSpeedSequence(painSpikePattern);
            }

            _lastPain = Pain;
        }


        private static float CalculateBleedingLevel(global::Body body)
        {
            float severity = Mathf.Clamp01(Mathf.Max(body.totalBleedSpeed * 4.0f, body.internalBleeding / 100.0f));

            Limb[] limbs = body.limbs;
            if (limbs == null)
            {
                return severity;
            }

            for (int i = 0; i < limbs.Length; i++)
            {
                Limb limb = limbs[i];
                if (limb == null || limb.dismembered)
                {
                    continue;
                }

                if (limb.bleedAmount > 0.01f)
                {
                    severity = Mathf.Max(severity, Mathf.Clamp01(limb.bleedAmount / 30.0f));
                }
            }

            return severity;
        }

        private static float CalculateRadiationLevel(global::Body body)
        {
            float exposure = 0.0f;
            if (PlayerCamera.main != null)
            {
                exposure = Mathf.Clamp01(PlayerCamera.main.irradiateIntensity);
            }

            return Mathf.Max(exposure, Mathf.Clamp01(body.radiationSickness / 100.0f));
        }
    }
}
