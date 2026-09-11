using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CUButt
{
    public static class CUButtSettingsWindow
    {
        private const int ChartWidth = 360;
        private const int ChartHeight = 80;

        private static bool _visible;
        private static Vector2 _scroll;
        private static Rect _windowRect = new Rect(50, 50, 450, 580);
        private static Texture2D _vibrationTexture;
        private static Texture2D _threadTexture;

        private static readonly GUIStyle PanelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(0, 0, 0, 0)
        };

        private static readonly GUIStyle HeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = Color.white }
        };

        private static readonly GUIStyle StatusConnectedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = Color.green }
        };

        private static readonly GUIStyle StatusDisconnectedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = Color.red }
        };

        public static void Open()
        {
            _visible = true;
        }

        public static void Close()
        {
            _visible = false;
        }

        public static void Cleanup()
        {
            if (_vibrationTexture != null)
            {
                UnityEngine.Object.Destroy(_vibrationTexture);
                _vibrationTexture = null;
            }

            if (_threadTexture != null)
            {
                UnityEngine.Object.Destroy(_threadTexture);
                _threadTexture = null;
            }
        }

        public static void Draw()
        {
            if (!_visible)
                return;

            _windowRect = GUI.Window(31245, _windowRect, DrawSettingsWindow, "CUButt Settings");
        }

        private static void EnsureTexture(ref Texture2D texture, int width, int height)
        {
            if (texture != null && texture.width == width && texture.height == height)
                return;

            if (texture != null)
                UnityEngine.Object.Destroy(texture);

            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        private static void DrawSettingsWindow(int id)
        {
            GUILayout.BeginArea(new Rect(0, 0, _windowRect.width, _windowRect.height));
            GUILayout.BeginVertical(PanelStyle);

            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Intiface: " + (VibrationManager.Initialized ? "Connected" : "Disconnected"),
                VibrationManager.Initialized ? StatusConnectedStyle : StatusDisconnectedStyle,
                GUILayout.Width(260));
            if (GUILayout.Button("Reconnect", GUILayout.Height(26), GUILayout.Width(100)))
            {
                _ = VibrationManager.Initialize();
            }
            if (GUILayout.Button("Close", GUILayout.Height(26), GUILayout.Width(70)))
            {
                Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.BeginVertical(PanelStyle);
            GUILayout.Label("Vibration graph", HeaderStyle);
            DrawVibrationChart();
            GUILayout.EndVertical();

            GUILayout.Space(8);
            GUILayout.BeginVertical(PanelStyle);
            GUILayout.Label("Thread vibration graph", HeaderStyle);
            DrawThreadVibrationChart();
            GUILayout.EndVertical();

            GUILayout.Space(8);
            GUILayout.BeginVertical(PanelStyle);
            GUILayout.Label("Trigger multipliers", HeaderStyle);
            DrawMultiplierSliders();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 24));
        }

        private static void DrawVibrationChart()
        {
            var samples = VibrationManager.GetVibrationHistory().ToArray();

            EnsureTexture(ref _vibrationTexture, ChartWidth, ChartHeight);
            var pixels = new Color[ChartWidth * ChartHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.08f, 0.08f, 0.08f, 1f);
            }

            if (samples.Length > 1)
            {
                int sampleCount = samples.Length;

                for (int i = 1; i < sampleCount; i++)
                {
                    int x0 = (int)((float)(i - 1) / Math.Max(1, sampleCount - 1) * (ChartWidth - 1));
                    int x1 = (int)((float)i / Math.Max(1, sampleCount - 1) * (ChartWidth - 1));

                    int y0 = (int)(Mathf.Clamp01(samples[i - 1]) * (ChartHeight - 1));
                    int y1 = (int)(Mathf.Clamp01(samples[i]) * (ChartHeight - 1));

                    int steps = Math.Max(1, Math.Abs(x1 - x0));
                    for (int s = 0; s < steps; s++)
                    {
                        int x = x0 + s;
                        int y = y0 + (y1 - y0) * s / steps;
                        if (x >= 0 && x < ChartWidth && y >= 0 && y < ChartHeight)
                        {
                            pixels[y * ChartWidth + x] = new Color(0.2f, 0.9f, 0.5f, 1f);
                        }
                    }
                }
            }

            _vibrationTexture.SetPixels(pixels);
            _vibrationTexture.Apply();

            GUILayout.Box(_vibrationTexture, GUILayout.Width(ChartWidth), GUILayout.Height(ChartHeight));
        }

        private static void DrawThreadVibrationChart()
        {
            var samples = VibrationManager.GetThreadVibrationHistory().ToArray();

            EnsureTexture(ref _threadTexture, ChartWidth, ChartHeight);
            var pixels = new Color[ChartWidth * ChartHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.08f, 0.08f, 0.08f, 1f);
            }

            if (samples.Length > 0)
            {
                var threadNames = ThreadManager.Threads.Keys.OrderBy(k => k).ToArray();

                foreach (var threadName in threadNames)
                {
                    Color color = GetThreadColor(threadName);
                    int? lastX = null;
                    int? lastY = null;

                    for (int i = 0; i < samples.Length; i++)
                    {
                        var sample = samples[i];
                        if (!sample.TryGetValue(threadName, out float value))
                        {
                            lastX = null;
                            lastY = null;
                            continue;
                        }

                        value = Mathf.Clamp01(value);
                        int x = (int)((float)i / Math.Max(1, samples.Length - 1) * (ChartWidth - 1));
                        int y = (int)(value * (ChartHeight - 1));

                        if (lastX.HasValue && lastY.HasValue)
                        {
                            int x0 = lastX.Value;
                            int y0 = lastY.Value;
                            int steps = Math.Max(1, Math.Abs(x - x0));
                            for (int s = 0; s < steps; s++)
                            {
                                int px = x0 + s;
                                int py = y0 + (y - y0) * s / steps;
                                if (px >= 0 && px < ChartWidth && py >= 0 && py < ChartHeight)
                                {
                                    pixels[py * ChartWidth + px] = color;
                                }
                            }
                        }

                        lastX = x;
                        lastY = y;
                    }
                }
            }

            _threadTexture.SetPixels(pixels);
            _threadTexture.Apply();

            GUILayout.Box(_threadTexture, GUILayout.Width(ChartWidth), GUILayout.Height(ChartHeight));
        }

        private static Color GetThreadColor(string threadName)
        {
            uint hash = 2166136261u;
            foreach (char c in threadName)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            byte r = (byte)(hash & 0xFF);
            byte g = (byte)((hash >> 8) & 0xFF);
            byte b = (byte)((hash >> 16) & 0xFF);

            return new Color(r / 255f, g / 255f, b / 255f, 1f);
        }

        private static void DrawMultiplierSliders()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(210));
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(190));

            foreach (var threadName in ThreadManager.Threads.Keys.OrderBy(k => k))
            {
                var thread = ThreadManager.Threads[threadName];
                var config = Plugin.Multipliers[threadName];
                var value = config.Value;
                var color = GetThreadColor(threadName);

                GUILayout.BeginHorizontal();
                GUILayout.Label("■", new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    normal = { textColor = color }
                }, GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Label($"{thread.Priority:0.###}", GUILayout.Width(42));
                GUILayout.Label(threadName, GUILayout.Width(110));
                float newValue = GUILayout.HorizontalSlider(value, 0f, 2f, GUILayout.Width(180));
                config.Value = newValue;
                GUILayout.Label(newValue.ToString("0.00"), GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
