using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnPlayerOpenTreeVault(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerOpenTreeVault", e);
        }

        public virtual void _OnPlayerEquipCloth(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerEquipCloth", e);
        }

        public virtual void _OnPlayerInMonstArea(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerInMonstArea", e);
        }

        public virtual void _OnPlayerRoleEscapeInMap(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerRoleEscapeInMap", e);
        }

        public virtual void _OnPlayerRoleUpGradeBig(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerRoleUpGradeBig", e);
        }

        public virtual void _OnUpGradeAndCloseFateFeatureUI(ETypeData e)
        {
            CallEvents<ETypeData>("OnUpGradeAndCloseFateFeatureUI", e);
        }

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
        public virtual void OnPlayerOpenTreeVault(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerOpenTreeVault", e);
        }

        public virtual void OnPlayerEquipCloth(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerEquipCloth", e);
        }

        public virtual void OnPlayerInMonstArea(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerInMonstArea", e);
        }

        public virtual void OnPlayerRoleEscapeInMap(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerRoleEscapeInMap", e);
        }

        public virtual void OnPlayerRoleUpGradeBig(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnPlayerRoleUpGradeBig", e);
        }

        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUpGradeAndCloseFateFeatureUI", e);
        }

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