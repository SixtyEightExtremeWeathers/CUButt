using System;
using System.Threading.Tasks;
using System.Linq;
using Buttplug.Client;
using Buttplug.Core.Messages;
using System.Net.Sockets;

namespace CUButt
{
    class VibrationController
    {
        static private ButtplugClient _client;
        static public bool Initialized = false;

        static public async Task Initialize()
        {
            if (Initialized)
                return;

            _client = new ButtplugClient("CUButt");

            try
            {
            await _client.ConnectAsync("ws://127.0.0.1:12345");
            await _client.StartScanningAsync();
            Initialized = true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to connect to Intiface. Retrying in 5 seconds. Error: {ex.Message}");
            }

        }

        public static async Task SendSpeed(float speed)
        {
            if (!Initialized || _client == null || !_client.Connected){return;}

            speed = Math.Clamp(speed * Plugin.GlobalMultiplier.Value, 0f, 1f);

            var devices = _client.Devices
                .Where(device =>
                    device.HasOutput(OutputType.Vibrate))
                .ToArray();

            var tasks = devices.Select(device =>
                device.RunOutputAsync(
                    DeviceOutput.Vibrate.Percent(speed)));

            await Task.WhenAll(tasks);
        }
    }
}