using UnityEngine;
using System.Reflection;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace CUButt.Triggers
{
    internal static class Electricity
    {
        private static float _electricalUntil;
        private static float _electricalStrength;

        internal static void TriggerElectricalShock()
        {
            VibrationManager.AddSpeedSequence(new List<float> { 0.3f, 0.9f, 0.4f, 0.5f, 0.95f, 0.1f, 0.8f });
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
                Electricity.TriggerElectricalShock();
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
                Electricity.TriggerElectricalShock();
            }
        }
    }
}