using UnityEngine;
using System.Reflection;
using HarmonyLib;
using System;

namespace CUButt.Triggers
{
    internal static class Electricity
    {
        private static float _electricalUntil;
        private static float _electricalStrength;

        internal static void TriggerElectricalShock(float strength, float duration = 1.25f)
        {
            _electricalStrength = Mathf.Max(_electricalStrength, Mathf.Clamp01(strength));
            _electricalUntil = Mathf.Max(_electricalUntil, Timer.Time + Mathf.Max(0.1f, duration));
        }

        public static void Add()
        {
            if (Timer.Time < _electricalUntil)
            {
                float electrical = Mathf.Lerp(0.62f, 1.0f, Timer.Value) * _electricalStrength;
                VibrationManager.SetSpeed(electrical, 1);
            }
            else
            {
                _electricalStrength = 0.0f;
            }
        }
    }

    [HarmonyPatch(typeof(global::Item), "Defibrillate")]
    internal static class ItemDefibrillatePatch
    {
        private static readonly FieldInfo LimbField = AccessTools.Field(AccessTools.TypeByName("Item+DefibInfo"), "limb");

        private static void Prefix(object[] __args, ref bool __state)
        {
            object info = __args != null && __args.Length > 0 ? __args[0] : null;
            var limb = info == null ? null : LimbField?.GetValue(info) as global::Limb;
            __state = limb != null;
        }

        private static void Postfix(bool __state)
        {
            if (__state)
            {
                Electricity.TriggerElectricalShock(0.90f, 0.75f);
            }
        }
    }


    [HarmonyPatch(typeof(global::CoilScript), "Shock")]
    internal static class CoilScriptShockPatch
    {
        private static void Prefix(global::CoilScript __instance, global::Limb __0, ref bool __state)
        {
            __state = __instance.cooldown <= 0.0f && __0 != null;
        }

        private static void Postfix(bool __state)
        {
            if (__state)
            {
                Electricity.TriggerElectricalShock(1.0f, 1.5f);
            }
        }
    }
}