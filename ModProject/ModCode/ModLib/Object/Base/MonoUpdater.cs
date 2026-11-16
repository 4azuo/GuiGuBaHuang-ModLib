using System;
using UnityEngine;

namespace ModLib.Object
{
    /// <summary>
    /// Il2Cpp MonoBehaviour attached to g.root; calls UpdateFunc every frame (drives OnMonoUpdate).
    /// </summary>
    public class MonoUpdater : MonoBehaviour
    {
        /// <summary>
        /// Il2Cpp constructor.
        /// </summary>
        public MonoUpdater(IntPtr ptr) : base(ptr) { }

        public Action UpdateFunc { get; set; }

        //Dont delete
        void Update()
        {
            UpdateFunc?.Invoke();
        }
    }
}
