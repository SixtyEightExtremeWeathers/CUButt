using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CUButt
{
    public class VibrationThread
    {
        public string Name;
        public float Priority;
        public List<float> Queue;
        public float Multiplier => GetMultiplier();

        public VibrationThread(string name, float priority)
        {
            Name = name;
            Priority = priority;
            Queue = Enumerable.Repeat(-1f, 6000).ToList();
        }

        public float GetMultiplier()
        {
            return Plugin.Multipliers[Name].Value;
        }
    }



    static public class ThreadManager
    {
        static public Dictionary<string, VibrationThread> Threads = new Dictionary<string, VibrationThread>();
        static public List<VibrationThread> ThreadsList => Threads.Values.ToList();


        static public void AddThread(string name, float priority = 0f)
        {
            if (Threads.ContainsKey(name))return;
            Threads.Add(name, new VibrationThread(name, priority));
        }

        static public Dictionary<string, float> GetThreadVibrationSnapshot()
        {
            var snapshot = new Dictionary<string, float>();

            foreach (var thread in ThreadsList)
            {
                float value = thread.Queue[0];
                if (value >= 0f)
                {
                    snapshot[thread.Name] = Mathf.Clamp01(value);
                }
            }

            return snapshot;
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