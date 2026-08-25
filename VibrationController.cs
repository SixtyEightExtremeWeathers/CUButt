using System;
using System.Linq;
using System.Threading.Tasks;
using ButtplugManaged;

namespace CUButt
{
    public static class VibrationController
    {
        private static ButtplugClient _client;
        public static bool Initialized = false;

        public static async Task Initialize()
        {
            if (Initialized)
                return;

            _client = new ButtplugClient("CUButt");

            try
            {
                var connector = new ButtplugWebsocketConnectorOptions(
                    new Uri("ws://127.0.0.1:12345")
                );

                await _client.ConnectAsync(connector);

                await _client.StartScanningAsync();

                Plugin.Log.LogInfo($"Devices found: {_client.Devices.Length}");

                int vibrators = _client.Devices
                    .Count(device =>
                        device.AllowedMessages.ContainsKey(DeviceMessages.VibrateCmd));

                Plugin.Log.LogInfo($"Available vibrators: {vibrators}");

                Initialized = true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    $"Failed to connect to Intiface. Error: {ex.Message}"
                );
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