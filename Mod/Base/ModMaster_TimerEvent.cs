using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnTimeUpdate()
        {
            CallEvents("OnTimeUpdate");
        }

        public virtual void _OnTimeUpdate200ms()
        {
            CallEvents("OnTimeUpdate200ms");
        }

        public virtual void _OnTimeUpdate500ms()
        {
            CallEvents("OnTimeUpdate500ms");
        }

        public virtual void _OnTimeUpdate1s()
        {
            CallEvents("OnTimeUpdate1s");
        }
        #endregion

        #region ModLib - Events
        public virtual void OnTimeUpdate()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate");
        }

        public virtual void OnTimeUpdate200ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate200ms");
        }

        public virtual void OnTimeUpdate500ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate500ms");
        }

        public virtual void OnTimeUpdate1s()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate1s");
        }
        #endregion
    }
}