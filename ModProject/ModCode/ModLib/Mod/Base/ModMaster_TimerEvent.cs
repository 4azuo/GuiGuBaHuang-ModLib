using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnTimeUpdate10ms()
        {
            CallEvents("OnTimeUpdate10ms");
        }

        public virtual void _OnTimeUpdate100ms()
        {
            CallEvents("OnTimeUpdate100ms");
        }

        public virtual void _OnTimeUpdate200ms()
        {
            CallEvents("OnTimeUpdate200ms");
        }

        public virtual void _OnTimeUpdate500ms()
        {
            CallEvents("OnTimeUpdate500ms");
        }

        public virtual void _OnTimeUpdate1000ms()
        {
            CallEvents("OnTimeUpdate1000ms");
        }
        #endregion

        #region ModLib - Events
        public virtual void OnTimeUpdate10ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate10ms");
        }

        public virtual void OnTimeUpdate100ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate100ms");
        }

        public virtual void OnTimeUpdate200ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate200ms");
        }

        public virtual void OnTimeUpdate500ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate500ms");
        }

        public virtual void OnTimeUpdate1000ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate1000ms");
        }
        #endregion
    }
}