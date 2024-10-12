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
        public virtual void _OnTimeUpdate1s()
        {
            CallEvents("OnTimeUpdate1s");
        }

        public virtual void _OnFrameUpdate()
        {
            CallEvents("OnFrameUpdate");
        }
        #endregion

        #region ModLib - Events
        public virtual void OnTimeUpdate()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnTimeUpdate1s()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnFrameUpdate()
        {
            EventHelper.RunMinorEvents();
        }
        #endregion
    }
}