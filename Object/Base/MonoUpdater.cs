using System;
using UnityEngine;

namespace ModLib.Object
{
    public class MonoUpdater : MonoBehaviour
    {
        public MonoUpdater(IntPtr ptr) : base(ptr) { }

        public Action UpdateFunc { get; set; }

        //Dont delete
        void Update()
        {
            UpdateFunc?.Invoke();
        }
    }
}
