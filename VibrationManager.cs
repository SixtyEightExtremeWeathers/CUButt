using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Buttplug.Client;
using Buttplug.Core.Messages;
using System.Collections.Generic;
using System.Net.Sockets;

namespace CUButt
{
    public static class VibrationManager
    {
        internal static List<VibrationPoint> queue = Enumerable.Repeat(new VibrationPoint { speed = 0, priority = 0 }, 600).ToList();

        internal static void Tick()
        {
            if (!VibrationController.Initialized && Timer.InitAttemptTimer > 5f)
            {
                Timer.InitAttemptTimer = 0f;
                VibrationController.Initialize();
                return;
            }
            if (!VibrationController.Initialized || !Timer.CanUpdate || queue.Count == 0)return;
            if (Plugin.WSMode.Value)
            {
                SetSpeed(WholesomeOutput(), 0);
            }

            VibrationController.SendSpeed(queue[0].speed);
            queue.RemoveAt(0);
            queue.Add(new VibrationPoint { speed = 0, priority = 0 });
            Timer.CanUpdate = false;
        }

        private static void SetPoint(VibrationPoint point, int index = 0)
        {
            int priority = point.priority;
            float speed = Math.Clamp(point.speed, 0f, 1f);
            index = Math.Clamp(index, 0, Math.Max(0, queue.Count - 1));

            int queuePriority;
            float queueSpeed;
            if (queue.Count > index)
            {
                queuePriority = queue[index].priority;
                queueSpeed = queue[index].speed;
            }
            else
            {
                queuePriority = 0;
                queueSpeed = 0;
            }

            if (queuePriority < priority)
            {
                queue[index] = new VibrationPoint { speed = speed, priority = priority };
            }
            else if (queueSpeed < speed && queuePriority <= priority)
            {
                queue[index] = new VibrationPoint { speed = speed, priority = priority };
            }
        }

        public static void SetSpeed(float speed, int priority = 0)
        {
            SetPoint(new VibrationPoint { speed = speed, priority = priority });
        }

        public static void AddPointSequence(List<VibrationPoint> sequence)
        {
            int index = 0;
            foreach (var point in sequence)
            {
                SetPoint(point, index);
                index++;
            }
        }

        public static void AddSpeedSequence(List<float> sequence, int priority = 0)
        {
            List<VibrationPoint> points = sequence.Select(v => new VibrationPoint{ speed = v, priority = priority}).ToList();
            AddPointSequence(points);
        }

        private static float WholesomeOutput()
        {
            float opiate = Triggers.Body._opiate;
            float happiness = Triggers.Body._happiness;
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
                return midpoint + amplitude * Mathf.Sin(Timer.Time * Mathf.PI * 2.0f / OpiateCycleSeconds);
            }

            if (happiness < HappinessMin) {return 0;}
            else if (happiness > HappinessMax) {return HappinessMaxStrength;}
            else
            {
                return (happiness - HappinessMin)/(HappinessMax-HappinessMin)*HappinessMaxStrength;
            }
        
        }


        public static float SmoothPulse(float time, float frequency)
        {
            return 0.5f + 0.5f * Mathf.Sin(time * frequency * Mathf.PI * 2.0f);
        }

        public static float SharpPulse(float time, float frequency)
        {
            float phase = Mathf.Repeat(time * frequency, 1.0f);
            if (phase >= 0.32f)
            {
                return 0.0f;
            }

            float normalized = 1.0f - phase / 0.32f;
            return normalized * normalized;
        }

    }
}
