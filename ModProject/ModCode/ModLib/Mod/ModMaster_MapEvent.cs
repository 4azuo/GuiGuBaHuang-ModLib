using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnPlayerEquipCloth(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerEquipCloth", e, true, false);
        }

        public virtual void _OnPlayerInMonstArea(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerInMonstArea", e, true, false);
        }

        public virtual void _OnPlayerRoleEscapeInMap(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerRoleEscapeInMap", e, true, false);
        }

        public virtual void _OnPlayerRoleUpGradeBig(ETypeData e)
        {
            CallEvents<ETypeData>("OnPlayerRoleUpGradeBig", e, true, false);
        }

        public virtual void _OnUpGradeAndCloseFateFeatureUI(ETypeData e)
        {
            CallEvents<ETypeData>("OnUpGradeAndCloseFateFeatureUI", e, true, false);
        }

        public virtual void _OnUseHobbyProps(ETypeData e)
        {
            CallEvents<ETypeData>("OnUseHobbyProps", e, true, false);
        }
        #endregion

        #region ModLib - Events
        public virtual void OnPlayerEquipCloth(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnPlayerInMonstArea(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnPlayerRoleEscapeInMap(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnPlayerRoleUpGradeBig(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnUseHobbyProps(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }
        #endregion
    }
}