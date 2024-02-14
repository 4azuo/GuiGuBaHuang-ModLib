using EBattleTypeData;
using EGameTypeData;
using EMapTypeData;
using ModLib.Object;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    [Trace]
    public abstract class ModEvent : CachableObject
    {
        [JsonIgnore]
        public virtual int OrderIndex
        {
            [TraceIgnore]
            get;
        } = -1;

        #region Timer
        [TraceIgnore]
        public virtual void OnTimeUpdate()
        {
        }

        [TraceIgnore]
        public virtual void OnFrameUpdate()
        {
        }
        #endregion

        #region EMapType
        public virtual void OnPlayerEquipCloth(PlayerEquipCloth e)
        {
        }

        public virtual void OnPlayerInMonstArea(PlayerInMonstArea e)
        {
        }

        public virtual void OnPlayerRoleEscapeInMap(PlayerRoleEscapeInMap e)
        {
        }

        public virtual void OnPlayerRoleUpGradeBig(PlayerRoleUpGradeBig e)
        {
        }

        public virtual void OnUpGradeAndCloseFateFeatureUI(UpGradeAndCloseFateFeatureUI e)
        {
        }

        public virtual void OnUseHobbyProps(UseHobbyProps e)
        {
        }
        #endregion

        #region EGameType
        public virtual void OnOpenUIStart(OpenUIStart e)
        {
        }

        public virtual void OnOpenUIEnd(OpenUIEnd e)
        {
        }

        public virtual void OnCloseUIStart(CloseUIStart e)
        {
        }

        public virtual void OnCloseUIEnd(CloseUIEnd e)
        {
        }

        public virtual void OnLoadGame()
        {
        }

        public virtual void OnLoadGameFirst()
        {
        }

        public virtual void OnInitWorld(ETypeData e)
        {
        }

        public virtual void OnLoadScene(LoadScene e)
        {
        }

        public virtual void OnIntoWorld(ETypeData e)
        {
        }

        public virtual void OnFirstMonth()
        {
        }

        public virtual void OnMonthly()
        {
        }

        public virtual void OnSave(ETypeData e)
        {
        }
        #endregion

        #region EBattleType
        public virtual void OnBattleUnitInit(UnitInit e)
        {
        }

        public virtual void OnBattleUnitHit(UnitHit e)
        {
        }

        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
        }

        public virtual void OnBattleUnitUseProp(UnitUseProp e)
        {
        }

        public virtual void OnBattleUnitUseSkill(UnitUseSkill e)
        {
        }

        public virtual void OnBattleUnitUseStep(UnitUseStep e)
        {
        }

        public virtual void OnBattleUnitDie(UnitDie e)
        {
        }

        public virtual void OnBattleUnitDieEnd(ETypeData e)
        {
        }

        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e)
        {
        }

        public virtual void OnBattleUnitAddEffect(UnitAddEffect e)
        {
        }

        public virtual void OnBattleSetUnitType(SetUnitType e)
        {
        }

        public virtual void OnBattleStart(ETypeData e)
        {
        }

        public virtual void OnBattleEnd(BattleEnd e)
        {
        }

        public virtual void OnBattleExit(ETypeData e)
        {
        }
        #endregion
    }
}
