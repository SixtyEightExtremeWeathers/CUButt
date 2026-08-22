using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using Buttplug;
using UnityEngine;
using HarmonyLib;

namespace CUButt.Triggers
{
    [HarmonyPatch(typeof(global::Body), "HandleBody")]
    internal static class Body
    {
        private static float PainSpikeThreshold => Plugin.PainSpikeThreashold.Value;
        private static float PainSpikeDuration => Plugin.PainSpikeDuration.Value;
        private static float PainMultiplier => 0.60f * Plugin.PainMultiplier.Value;


        private static bool _wasAlive;
        private static bool _wasConscious;
        private static float _lastPain;
        private static float _painWindowBaseline;
        private static float _painWindowStartedAt;

        private static float _painLevel;
        private static float _bleedingLevel;
        private static float _radiationLevel;
        private static bool _cardiacProblem;
        private static bool _criticalState;

        public static float _happiness;
        public static float _opiate;

        private static float _painSpikeUntil;
        private static float _painSpikeStrength;
        private static float _unconsciousStartedAt = float.NegativeInfinity;
        private static float _deathStartedAt = float.NegativeInfinity;
        private static bool _deathLatched;

        private static void Postfix(global::Body __instance)
        {
            bool alive = __instance.alive;
            bool conscious = __instance.conscious;

            _happiness = __instance.happiness;
            _opiate = __instance.opiateHappiness;

            float pain = Mathf.Clamp(__instance.averagePain, 0.0f, 100.0f);
            if (Timer.Time - _painWindowStartedAt > 0.35f || pain < _painWindowBaseline)
            {
                _painWindowBaseline = pain;
                _painWindowStartedAt = Timer.Time;
            }

            float painDelta = pain - _painWindowBaseline;
            if (alive && painDelta >= PainSpikeThreshold)
            {
                float severity = Mathf.InverseLerp(PainSpikeThreshold, 60.0f, painDelta);
                _painSpikeStrength = Mathf.Max(_painSpikeStrength, Mathf.Lerp(0.45f, 0.85f, severity));
                _painSpikeUntil = Mathf.Max(_painSpikeUntil, Timer.Time + PainSpikeDuration);
                _painWindowBaseline = pain;
                _painWindowStartedAt = Timer.Time;
            }

            if (_wasConscious && !conscious && alive)
            {
                _unconsciousStartedAt = Timer.Time;
            }

            if (_wasAlive && !alive)
            {
                _deathStartedAt = Timer.Time;
                _deathLatched = true;
            }
            else if (alive && !_wasAlive)
            {
                _deathLatched = false;
                _deathStartedAt = float.NegativeInfinity;
            }

            _painLevel = pain / 100.0f;
            _bleedingLevel = CalculateBleedingLevel(__instance);
            _radiationLevel = CalculateRadiationLevel(__instance);
            _cardiacProblem = __instance.fibrillationProgress > 1.0f || __instance.fibrillationForced || __instance.inCardiacArrest;
            _criticalState = __instance.isCriticallyDying;

            _lastPain = pain;
            _wasAlive = alive;
            _wasConscious = conscious;

            Add();
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

        public static bool IsDead()
        {
            return _deathLatched;
        }

        public static void Add()
        {
            float unconsciousElapsed = Timer.Time - _unconsciousStartedAt;
            float unconsciousImpulse = 0.0f;
            if (unconsciousElapsed >= 0.0f && unconsciousElapsed < 1.0f)
            {
                float envelope = 1.0f - unconsciousElapsed;
                unconsciousImpulse = envelope * envelope;
            }

            if (_criticalState)
            {
                VibrationManager.SetSpeed(VibrationManager.SharpPulse(Timer.Time, 3.4f));
                VibrationManager.SetSpeed(unconsciousImpulse);
            }

            VibrationManager.SetSpeed(Mathf.Clamp01(_painLevel) * PainMultiplier);

            if (_bleedingLevel > 0.0f)
            {
                float bleedPulse = VibrationManager.SmoothPulse(Timer.Time, 1f);
                float bleedAmplitude = Mathf.Lerp(0.08f, 0.40f, _bleedingLevel);
                VibrationManager.SetSpeed(bleedPulse * bleedAmplitude);
            }

            if (_cardiacProblem)
            {
                VibrationManager.SetSpeed(VibrationManager.SharpPulse(Timer.Time, 3.4f) * 0.72f);
            }

            if (_radiationLevel > 0.001f)
            {
                float radiation = Mathf.Lerp(0.58f, 1.0f, Timer.Value);
                radiation *= Mathf.Lerp(0.70f, 1.0f, _radiationLevel);
                VibrationManager.SetSpeed(radiation);
            }


            if (Timer.Time < _painSpikeUntil)
            {
                float remaining = _painSpikeUntil - Timer.Time;
                float fade = remaining < 0.15f ? remaining / 0.15f : 1.0f;
                VibrationManager.SetSpeed(_painSpikeStrength * Mathf.Clamp01(fade));
            }
            else
            {
                _painSpikeStrength = 0.0f;
            }

        }
    }
}
