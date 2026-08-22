using System;
using System.Collections.Generic;


namespace CUButt
{
    public enum InterpolationType
    {
        Linear,
        EaseIn,
        EaseOut,
        Smooth
    }

    public struct VibrationCheckPoint
    {
        public float time;
        public float strength;
        public InterpolationType interpolation;

        public VibrationCheckPoint(float time, float strength, InterpolationType interpolation)
        {
            this.time = time;
            this.strength = strength;
            this.interpolation = interpolation;
        }
    }


    public class VibrationGenerator
    {
        private const float Step = 0.1f;


        public List<float> CreateImpulse(List<VibrationCheckPoint> points)
        {
            List<float> sequence = new List<float>();

            if (points.Count < 2)
                return sequence;


            float duration = points[^1].time;

            int count = (int)MathF.Ceiling(duration / Step);


            for (int i = 0; i <= count; i++)
            {
                float currentTime = i * Step;

                float value = Evaluate(points, currentTime);

                sequence.Add(value);
            }


            return sequence;
        }


        private float Evaluate(List<VibrationCheckPoint> points, float time)
        {
            VibrationCheckPoint a = points[0];
            VibrationCheckPoint b = points[^1];


            for (int i = 0; i < points.Count - 1; i++)
            {
                if (time >= points[i].time && time <= points[i + 1].time)
                {
                    a = points[i];
                    b = points[i + 1];
                    break;
                }
            }


            float t = (time - a.time) / (b.time - a.time);

            t = ApplyInterpolation(t, a.interpolation);


            return Lerp(a.strength, b.strength, t);
        }


        private float ApplyInterpolation(float t, InterpolationType type)
        {
            switch (type)
            {
                case InterpolationType.Linear:
                    return t;


                case InterpolationType.EaseIn:
                    return t * t;


                case InterpolationType.EaseOut:
                    return 1f - (1f - t) * (1f - t);


                case InterpolationType.Smooth:
                    return t * t * (3f - 2f * t);


                default:
                    return t;
            }
        }


        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}