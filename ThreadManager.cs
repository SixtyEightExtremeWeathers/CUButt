using System;
using System.Collections.Generic;
using System.Linq;

namespace CUButt
{
    public class VibrationThread
    {
        public string Name;
        public float Priority;
        public List<float> Queue;
        public float Multiplier => GetMultiplier();
        private float _multiplier;

        public VibrationThread(string name, float priority, float multiplier)
        {
            Name = name;
            Priority = priority;
            Queue = Enumerable.Repeat(-1f, 6000).ToList();
            _multiplier = multiplier;
        }

        public float GetMultiplier()
        {
            if (_multiplier == -1)
            {
                return Plugin.Multipliers[Name].Value;
            }
            else{return _multiplier;}
        }
    }



    static public class ThreadManager
    {
        static public Dictionary<string, VibrationThread> Threads = new Dictionary<string, VibrationThread>();
        static public List<VibrationThread> ThreadsList => Threads.Values.ToList();


        static public void AddThread(string name, float priority = 0, float multiplier = -1)
        {
            if (Threads.ContainsKey(name))return;
            Threads.Add(name, new VibrationThread(name, priority, multiplier));
        }

        static public float GetSpeed()
        {
            var candidates = ThreadsList.Where(thread => thread.Queue[0] != -1f).ToList();

            if (candidates.Count == 0)
                return 0f;

            float highestBasePriority = candidates.Max(thread => (float)Math.Truncate(thread.Priority));
            var byBasePriority = candidates.Where(thread => (float)Math.Truncate(thread.Priority) == highestBasePriority).ToList();

            var groupedByPriority = byBasePriority
                .GroupBy(thread => thread.Priority)
                .Select(group => new
                {
                    Priority = group.Key,
                    MaxQueueValue = group.Max(thread => thread.Queue[0])
                })
                .ToList();

            return groupedByPriority.Sum(group => group.MaxQueueValue);
        }

        static public void SetSpeed(float speed, string threadName, int index = 0)
        {
            if (!Threads.ContainsKey(threadName))return;
            VibrationThread Thread = Threads[threadName];
            Thread.Queue[index] = speed * Thread.Multiplier;
        }

        static public void QueueUpdate()
        {
            foreach (var thread in ThreadsList)
            {
                thread.Queue.Add(-1f);
                thread.Queue.RemoveAt(0);
            }
        }

    }
}