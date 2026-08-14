using HarmonyLib;
using UnityEngine;

namespace CUButt.Triggers
{
    [HarmonyPatch(typeof(global::Body), "OnCollisionEnter2D")]
    internal static class Landing
    {

        public static float _landingUntil;
        public static float _landingStrength;
        public const float LandingCooldown = 0.12f;
        private static void Prefix(global::Body __instance, Collision2D __0)
        {
            if (!VibrationManager.IsPlayerBody(__instance) || __0 == null)return;

            float downwardSpeed = -__instance.lastTimeStepVelocity.y;
            if (downwardSpeed < 0.75f || !HasHorizontalContact(__0))
            {
                return;
            }

            float heavyLandingSpeed = Mathf.Max(8.0f, __instance.jumpSpeed * 2.0f);
            float severity = Mathf.InverseLerp(0.75f, heavyLandingSpeed, downwardSpeed);

            float clamped = Mathf.Clamp(Mathf.Lerp(0.10f, 0.25f, severity), 0.04f, 0.30f);
            if (Timer.Time < _landingUntil && clamped <= _landingStrength)return;

            _landingStrength = clamped;
            _landingUntil = Timer.Time + LandingCooldown;
        }

        private static bool HasHorizontalContact(Collision2D collision)
        {
            int contactCount = collision.contactCount;
            for (int i = 0; i < contactCount; i++)
            {
                if (Mathf.Abs(collision.GetContact(i).normal.y) >= 0.35f)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Add()
        {

            if (Timer.Time < _landingUntil)
            {
                float fade = Mathf.InverseLerp(_landingUntil, _landingUntil - LandingCooldown, Timer.Time);
                VibrationManager.Add(_landingStrength * Mathf.Clamp01(fade));
            }
            else
            {
                _landingStrength = 0.0f;
            }
        }
    }
}
