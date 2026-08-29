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
        private static float _lastPainTimer;
        private static float _lastRadiation;


        private static void Postfix(global::Body __instance)
        {
            if (Plugin.WSMode.Value)
            {
                WholesomeTrigger(__instance);
            }
            else
            {
                PainTrigger(__instance);
                DeathTrigger(__instance);
                BleedingTrigger(__instance);
                RadiationTrigger(__instance);
                ECGTrigger(__instance);
                StaminaTrigger(__instance);
            }
        }

        private static void WholesomeTrigger(global::Body body)
        {
            float opiate = body.opiateHappiness;
            float happiness = body.happiness;
            float HappinessMin = 5.0f;
            float HappinessMax = 70.0f;
            float HappinessMaxStrength = 0.75f;
            float OpiateMin = 5.0f;
            float OpiateMax = 45.0f;
            float OpiateMinStrength = 0.20f;
            float OpiateMaxStrength = 1.00f;
            float OpiateCycleSeconds = 2.0f;

            if (opiate > OpiateMin)
            {
                float trueOpiateMaxStrength;
                if (opiate > OpiateMax) {trueOpiateMaxStrength = OpiateMaxStrength;}
                else
                {
                    trueOpiateMaxStrength = (opiate - OpiateMin)/(OpiateMax - OpiateMin)*OpiateMaxStrength;
                }

                float midpoint = (OpiateMinStrength + trueOpiateMaxStrength) * 0.5f;
                float amplitude = (trueOpiateMaxStrength - OpiateMinStrength) * 0.5f;
                VibrationManager.SetSpeed(midpoint + amplitude * Mathf.Sin(Time.time * Mathf.PI * 2.0f / OpiateCycleSeconds), 99);
            }

            if (happiness < HappinessMin) {VibrationManager.SetSpeed(0f, 99);}
            else if (happiness > HappinessMax) {VibrationManager.SetSpeed(HappinessMaxStrength, 99);}
            else
            {
                VibrationManager.SetSpeed((happiness - HappinessMin)/(HappinessMax-HappinessMin)*HappinessMaxStrength, 99);
            }
        }

        private static void PainTrigger(global::Body body)
        {

            _lastPainTimer += Time.deltaTime;
            float Pain = body.averagePain;
            float PainDelta = Pain - _lastPain;
            VibrationManager.SetSpeed(Mathf.Clamp01((Pain-0.3f*_lastPainTimer)/100f * PainMultiplier));


            if (PainDelta >= 5f)
            {
                _lastPainTimer = 0f;
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

        private static void DeathTrigger(global::Body body)
        {
            if (body.alive)
            {
                return;
            }

            VibrationManager.SetSpeed(0.0f, 100);
        }

        private static void BleedingTrigger(global::Body body)
        {
            float severity = Mathf.Clamp01(Mathf.Max(body.totalBleedSpeed * 4.0f, body.internalBleeding / 100.0f));
            float bleedAmplitude;
            float bleedPulse;

            Limb[] limbs = body.limbs;
            if (limbs != null)
            {
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
            }

            if (severity <= 0.0f)
            {
                return;
            }

            bleedPulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 1f * Mathf.PI * 2.0f);
            bleedAmplitude = Mathf.Lerp(0.08f, 0.40f, severity);
            VibrationManager.SetSpeed(bleedPulse * bleedAmplitude);
        }

        private static void RadiationTrigger(global::Body body)
        {
            float RadiationLevel = body.radiationSickness*0.3f;
            float RadiationSpeed = (RadiationLevel - _lastRadiation)/Time.deltaTime;
            _lastRadiation = RadiationLevel;

            if (RadiationSpeed > 0.01f)
            {
                float radiation = Random.Range(0f, 1f) * RadiationSpeed;
                VibrationManager.SetSpeed(radiation, 1);
            }
        }
    
        static public void ECGTrigger(global::Body body)
        {
            float ModeMultiplier;
            if (body.fibrillationProgress > 1.0f || body.fibrillationForced || body.inCardiacArrest)
            {
                ModeMultiplier = 0.7f;
            }
            else if (body.isCriticallyDying)
            {
                ModeMultiplier = 1f;
            }
            else
            {
                return;
            }
            
            
            
            float num = Mathf.Lerp(body.heartCurveNormal.Evaluate(body.heartProg), body.heartCurveArrythmia.Evaluate(body.heartProg), body.fibrillationProgress / 90f) * body.randomFibrillationVariation;
            if (body.fibrillationProgress > 75f)
            {
                num *= 1f - (body.fibrillationProgress - 75f) / 25f;
            }
            if (body.heartRate <= 0f)
            {
                num = 0f;
            }
            if (body.defibShockedFrames > 0)
            {
                num = (Random.value > 0.5f) ? 1f : (-1f);
            }
            VibrationManager.SetSpeed(num * ModeMultiplier, 2);
        }
    
        private static void StaminaTrigger(global::Body body)
        {
            float stamina = body.stamina;
            if (stamina < 50f)
            {
                VibrationManager.SetSpeed((50f - stamina) / 100f * 0.25f);
            }
        }
    }
}
