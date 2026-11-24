using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// ModMaster partial: map (EMapType) events.
    /// </summary>
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnPlayerOpenTreeVault through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnPlayerOpenTreeVault(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerOpenTreeVault", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnPlayerEquipCloth through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnPlayerEquipCloth(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerEquipCloth", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnPlayerInMonstArea through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnPlayerInMonstArea(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerInMonstArea", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnPlayerRoleEscapeInMap through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnPlayerRoleEscapeInMap(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerRoleEscapeInMap", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnPlayerRoleUpGradeBig through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnPlayerRoleUpGradeBig(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerRoleUpGradeBig", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnUpGradeAndCloseFateFeatureUI through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnUpGradeAndCloseFateFeatureUI(ETypeData e)
        {
            CallEvents<ETypeData>("OnUpGradeAndCloseFateFeatureUI", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnUseHobbyProps through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnUseHobbyProps(ETypeData e)
        {
            CallEvents<ETypeData>("OnUseHobbyProps", e);
        }

        //public virtual void _OnFortuitousTrigger(ETypeData e)
        //{
        //    CallEvents<ETypeData>("OnFortuitousTrigger", e);
        //}
        #endregion

        #region ModLib - Events
        /// <summary>
        /// Dispatches OnPlayerOpenTreeVault to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnPlayerOpenTreeVault(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerOpenTreeVault", e);
        }

        /// <summary>
        /// Dispatches OnPlayerEquipCloth to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnPlayerEquipCloth(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerEquipCloth", e);
        }

        /// <summary>
        /// Dispatches OnPlayerInMonstArea to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnPlayerInMonstArea(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerInMonstArea", e);
        }

        /// <summary>
        /// Dispatches OnPlayerRoleEscapeInMap to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnPlayerRoleEscapeInMap(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerRoleEscapeInMap", e);
        }

        /// <summary>
        /// Dispatches OnPlayerRoleUpGradeBig to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnPlayerRoleUpGradeBig(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerRoleUpGradeBig", e);
        }

        /// <summary>
        /// Dispatches OnUpGradeAndCloseFateFeatureUI to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUpGradeAndCloseFateFeatureUI", e);
        }

        /// <summary>
        /// Dispatches OnUseHobbyProps to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnUseHobbyProps(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUseHobbyProps", e);
        }

        //public virtual void OnFortuitousTrigger(ETypeData e)
        //{
        //    EventHelper.RunMinorEvents("OnFortuitousTrigger", e);
        //}
        #endregion
    }
}