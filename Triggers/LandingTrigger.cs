using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CUButt.Triggers
{
    [HarmonyPatch(typeof(global::Body), "OnCollisionEnter2D")]
    internal static class Landing
    {
        private static void Prefix(global::Body __instance, Collision2D __0)
        {
            if (__0 == null)return;

            float downwardSpeed = -__instance.lastTimeStepVelocity.y;
            if (downwardSpeed < 0.75f || !HasHorizontalContact(__0))return;


            float heavyLandingSpeed = Mathf.Max(8.0f, __instance.jumpSpeed * 2.0f);
            float severity = Mathf.InverseLerp(0.75f, heavyLandingSpeed, downwardSpeed);

            float strength = Mathf.Clamp(Mathf.Lerp(0.10f, 0.25f, severity), 0.04f, 0.30f);
            List<float> landingPattern = PatternGenerator.CreateImpulse(new List<VibrationCheckPoint>
            {
                new VibrationCheckPoint(0.0f, 0.0f, InterpolationType.Smooth),
                new VibrationCheckPoint(0.1f, strength*0.35f, InterpolationType.Linear),
            });
            VibrationManager.AddSpeedSequence(landingPattern);
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

    }
}
