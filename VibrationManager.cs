using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ButtplugManaged;

namespace CUButt
{
    public static class VibrationManager
    {
        private static ButtplugClient _client;
        public static bool Initialized => _client != null && _client.Connected;
        private static float UpdateTimer = 0f;
        private static float AlertTimer = 0f;

        private const int HistoryWindowSeconds = 5;
        private const int HistorySamplesPerSecond = 10;
        private static readonly Queue<float> VibrationHistory = new Queue<float>();
        private static readonly Queue<Dictionary<string, float>> ThreadVibrationHistory = new Queue<Dictionary<string, float>>();

        public static IReadOnlyList<float> GetVibrationHistory()
        {
            return VibrationHistory.ToArray();
        }

        public static IReadOnlyList<Dictionary<string, float>> GetThreadVibrationHistory()
        {
            return ThreadVibrationHistory.ToArray();
        }

        internal static void RecordVibrationSample(float speed)
        {
            speed = Mathf.Clamp01(speed);
            VibrationHistory.Enqueue(speed);

            int maxSamples = HistoryWindowSeconds * HistorySamplesPerSecond;
            while (VibrationHistory.Count > maxSamples)
            {
                VibrationHistory.Dequeue();
            }
        }

        internal static void RecordThreadVibrationSample()
        {
            var snapshot = ThreadManager.GetThreadVibrationSnapshot();
            ThreadVibrationHistory.Enqueue(snapshot);

            int maxSamples = HistoryWindowSeconds * HistorySamplesPerSecond;
            while (ThreadVibrationHistory.Count > maxSamples)
            {
                ThreadVibrationHistory.Dequeue();
            }
        }

        internal async static Task Tick()
        {
            UpdateTimer += Time.unscaledDeltaTime;
            AlertTimer += Time.unscaledDeltaTime;
            if (!Initialized && AlertTimer > 7f)
            {
                if (PlayerCamera.main != null)
                {
                    PlayerCamera.main.DoAlert("Intiface isn't connected. Start it and write \"intiface\" to console");
                }
                AlertTimer = 0f;
                return;
            }
            else if (UpdateTimer < 0.1f || !Initialized)return;

            float speed = ThreadManager.GetSpeed();
            RecordVibrationSample(speed);
            RecordThreadVibrationSample();
            await SendSpeed(speed);
            ThreadManager.QueueUpdate();
            UpdateTimer = 0f;
        }

        public static void SetSpeed(float speed, string thread)
        {
            ThreadManager.SetSpeed(speed, thread);
        }

        public static void AddSpeedSequence(List<float> sequence, string thread)
        {   
            int index = 0;
            foreach (float speed in sequence)
            {
                ThreadManager.SetSpeed(speed, thread, index);
                index++;
            }
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
