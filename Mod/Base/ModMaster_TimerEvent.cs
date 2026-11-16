using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// ModMaster partial: timer events.
    /// </summary>
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTimeUpdate10ms through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTimeUpdate10ms()
        {
            CallEvents("OnTimeUpdate10ms");
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTimeUpdate100ms through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTimeUpdate100ms()
        {
            CallEvents("OnTimeUpdate100ms");
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTimeUpdate200ms through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTimeUpdate200ms()
        {
            CallEvents("OnTimeUpdate200ms");
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTimeUpdate500ms through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTimeUpdate500ms()
        {
            CallEvents("OnTimeUpdate500ms");
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTimeUpdate1000ms through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTimeUpdate1000ms()
        {
            CallEvents("OnTimeUpdate1000ms");
        }
        #endregion

        #region ModLib - Events
        /// <summary>
        /// Dispatches OnTimeUpdate10ms to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTimeUpdate10ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate10ms");
        }

        /// <summary>
        /// Dispatches OnTimeUpdate100ms to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTimeUpdate100ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate100ms");
        }

        /// <summary>
        /// Dispatches OnTimeUpdate200ms to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTimeUpdate200ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate200ms");
        }

        /// <summary>
        /// Dispatches OnTimeUpdate500ms to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTimeUpdate500ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate500ms");
        }

        /// <summary>
        /// Dispatches OnTimeUpdate1000ms to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTimeUpdate1000ms()
        {
            EventHelper.RunMinorEvents("OnTimeUpdate1000ms");
        }
        #endregion
    }
}