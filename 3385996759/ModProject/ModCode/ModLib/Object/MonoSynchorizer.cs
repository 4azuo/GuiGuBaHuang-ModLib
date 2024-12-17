using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace ModLib.Object
{
    public class MonoSynchorizer : MonoBehaviour
    {
        public static ConcurrentQueue<Action> RunOnMainThread { get; } = new ConcurrentQueue<Action>();

        void Update()
        {
            if (!RunOnMainThread.IsEmpty)
            {
                while (RunOnMainThread.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }
        }
    }
}
