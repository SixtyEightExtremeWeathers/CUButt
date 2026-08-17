using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Buttplug.Client;
using Buttplug.Core.Messages;

namespace CUButt
{
    internal static class VibrationManager
    {
        

        private static float _tempSpeed;

        static private ButtplugClient _client;
        static public bool _initialized;
        static private float _lastSpeed;


        internal static void Tick()
        {
            if (!_initialized)return;

            float speed;
            if (!Plugin.WSMode.Value) 
            {
                speed = CalculateOutput();
            }
            else
            {
                speed = WholesomeOutput();
            }

            SendSpeed(speed);
        }

        public static void Add(float speed)
        {
            _tempSpeed = Mathf.Max(speed, _tempSpeed);
        }

        internal static float CalculateOutput()
        {
            _tempSpeed = 0f;
            if (Triggers.Body.IsDead()){return _tempSpeed;}


            Triggers.Earthquake.Add();
            Triggers.Landing.Add();
            Triggers.Body.Add();
            Triggers.Electricity.Add();

            return _tempSpeed;
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

        internal static bool IsPlayerBody(Body body)
        {
            return body != null && PlayerCamera.main != null && PlayerCamera.main.body == body;
        }


        static public async Task Initialize()
        {
            if (_initialized)
                return;

            _client = new ButtplugClient("CUButt");

            await _client.ConnectAsync("ws://127.0.0.1:12345");

            _initialized = true;
        }

        public static async Task SendSpeed(float speed)
        {
            if (!_initialized || _client == null || !_client.Connected){return;}

            speed = Math.Clamp(speed * Plugin.GlobalMultiplier.Value, 0f, 1f);

            if (Math.Abs(_lastSpeed - speed) < Plugin.UpdateRate.Value){return;}
            _lastSpeed = speed;

            var devices = _client.Devices
                .Where(device =>
                    device.HasOutput(OutputType.Vibrate))
                .ToArray();

            var tasks = devices.Select(device =>
                device.RunOutputAsync(
                    DeviceOutput.Vibrate.Percent(speed)));

            await Task.WhenAll(tasks);
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
