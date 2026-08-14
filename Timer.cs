namespace CUButt
{
    public static class Timer
    {
        private static float _randomTimer;
        public static float Value;
        public static float Time;

        private const float RandomUpdateInterval = 0.05f;

        public static void Tick(float unscaledDeltaTime, float unscaledTime)
        {
            Time = unscaledTime;

            _randomTimer -= unscaledDeltaTime;
            if (_randomTimer <= 0.0f)
            {
                _randomTimer = RandomUpdateInterval;
                Value = UnityEngine.Random.Range(0.0f, 1.0f);
            }
        }
    }
}