namespace CUButt
{
    public static class Timer
    {
        private static float _randomTimer;
        public static float Value;
        public static float Time;
        public static bool CanUpdate = false;

        private const float UpdateInterval = 0.1f;

        public static void Tick(float unscaledDeltaTime, float unscaledTime)
        {
            Time = unscaledTime;
            _randomTimer -= unscaledDeltaTime;

            if (_randomTimer <= 0.0f)
            {
                _randomTimer = UpdateInterval;
                CanUpdate = true;
                Value = UnityEngine.Random.Range(0.0f, 1.0f);
            }
        }
    }

    public struct VibrationPoint
    {
        public float speed;
        public int priority;
    };

}