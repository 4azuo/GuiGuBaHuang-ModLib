using System;
using UnityEngine;

namespace ModLib.Object
{
    /// <summary>
    /// Il2Cpp MonoBehaviour attached to g.root; calls UpdateFunc every frame (drives OnMonoUpdate) and LateUpdates
    /// after every Update of the frame, before it is drawn (e.g. to hold the camera over the tweens of the game).
    /// </summary>
    public class MonoUpdater : MonoBehaviour
    {
        /// <summary>
        /// Il2Cpp constructor.
        /// </summary>
        public MonoUpdater(IntPtr ptr) : base(ptr) { }

        public Action UpdateFunc { get; set; }
        /// <summary>Called every frame from LateUpdate (after the Updates and tweens of the frame, before it is drawn).</summary>
        public static event Action LateUpdates;

        //Dont delete
        void Update()
        {
            UpdateFunc?.Invoke();
        }

        //Dont delete
        void LateUpdate()
        {
            try { LateUpdates?.Invoke(); } catch (Exception ex) { ModLib.Helper.DebugHelper.WriteLine(ex); }
        }
    }
}
