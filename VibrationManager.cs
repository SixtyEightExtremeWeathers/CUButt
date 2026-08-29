using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ButtplugManaged;

namespace CUButt
{
    public class VibrationPoint
    {
        public float speed;
        public int priority;
    }


    public static class VibrationManager
    {
        internal static List<VibrationPoint> queue = Enumerable.Repeat(new VibrationPoint { speed = 0, priority = 0 }, 6000).ToList();
        private static ButtplugClient _client;
        public static bool Initialized => _client != null && _client.Connected;
        private static float UpdateTimer = 0f;
        private static float AlertTimer = 0f;

        internal async static Task Tick()
        {
            UpdateTimer += Time.unscaledDeltaTime;
            AlertTimer += Time.unscaledDeltaTime;
            if (!Initialized && AlertTimer > 7f)
            {
                PlayerCamera.main.DoAlert("Intiface isn't connected. Start it and write \"intiface\" to console");
                AlertTimer = 0f;
                return;
            }
            else if (UpdateTimer < 0.1f || !Initialized)return;

            await SendSpeed(queue[0].speed);
            queue.RemoveAt(0);
            queue.Add(new VibrationPoint { speed = 0, priority = 0 });
            UpdateTimer = 0f;
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

        public static async Task<string> Initialize()
        {
            if (Initialized)
                return "Intiface already connected.";

            _client = new ButtplugClient("CUButt");

            try
            {
                var connector = new ButtplugWebsocketConnectorOptions(
                    new Uri("ws://127.0.0.1:12345")
                );

                await _client.ConnectAsync(connector);

                await _client.StartScanningAsync();


                int vibrators = _client.Devices
                    .Count(device =>
                        device.AllowedMessages.ContainsKey(DeviceMessages.VibrateCmd));

                return "CUButt successfully connected to Intiface.";
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    $"Failed to connect to Intiface. Error: {ex.Message}"
                );
                return "Failed to connect to Intiface.";
            }
        }


        public static async Task SendSpeed(float speed)
        {
            if (!Initialized || _client == null || !_client.Connected)
                return;

            speed = Math.Clamp(
                speed * Plugin.GlobalMultiplier.Value,
                0f,
                1f
            );


            var devices = _client.Devices
                .Where(device =>
                    device.AllowedMessages.ContainsKey(DeviceMessages.VibrateCmd))
                .ToArray();


            var tasks = devices.Select(device =>
                device.SendVibrateCmd(speed)
            );


            await Task.WhenAll(tasks);
        }
    }
}
