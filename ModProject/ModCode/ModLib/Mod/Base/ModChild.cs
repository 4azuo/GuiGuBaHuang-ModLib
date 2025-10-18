using ModLib.Object;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    public abstract class ModChild : ModEvent
    {
        [JsonIgnore]
        public ParameterStore ParameterStore { get; private set; }

        public void SetParameterStore(ParameterStore parameterStore)
        {
            ParameterStore = parameterStore;
        }
    }
}
