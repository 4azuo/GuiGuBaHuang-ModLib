using ModLib.Object;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    /// <summary>
    /// Entry class of a mod using ModLib (exactly one per mod, CacheId = mod id).
    /// Loaded first (order -1, Global, WorkOn All) and owns an optional ParameterStore shared by its events.
    /// </summary>
    public abstract class ModChild : ModEvent
    {
        [JsonIgnore]
        public ParameterStore ParameterStore { get; private set; }

        /// <summary>
        /// Overrides the data set (units, buildings) used by this mod's events this month.
        /// </summary>
        public void SetParameterStore(ParameterStore parameterStore)
        {
            ParameterStore = parameterStore;
        }
    }
}
