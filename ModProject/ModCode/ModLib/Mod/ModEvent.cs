using EBattleTypeData;
using EGameTypeData;
using ModLib.Object;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Mod
{
    [Trace]
    public abstract class ModEvent : CachableObject
    {
        public virtual int OrderIndex
        {
            [TraceIgnore]
            get;
            [TraceIgnore]
            set;
        } = -1;

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

        public virtual void OnUnitInit(UnitInit e)
        {
        }

        public virtual void OnUnitHit(UnitHit e)
        {
        }

        public virtual void OnUnitUseProp(UnitUseProp e)
        {
        }

        public virtual void OnUnitUseSkill(UnitUseSkill e)
        {
        }

        public virtual void OnUnitUseStep(UnitUseStep e)
        {
        }

        public virtual void OnUnitDie(UnitDie e)
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

        [TraceIgnore]
        public virtual void OnTimeUpdate()
        {
        }

        [TraceIgnore]
        public virtual void OnFrameUpdate()
        {
        }
    }
}
