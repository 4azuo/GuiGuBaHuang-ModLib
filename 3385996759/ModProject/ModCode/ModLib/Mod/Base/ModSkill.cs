using EBattleTypeData;
using EGameTypeData;
using ModLib.Object;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    [TraceIgnore]
    public abstract class ModSkill
    {
        public virtual void OnLoadClass() { }
        public virtual void OnLoad() { }
        public virtual void OnUnload() { }
    }
}
