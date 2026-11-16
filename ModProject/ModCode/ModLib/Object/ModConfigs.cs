using ModLib.Enum;

namespace ModLib.Object
{
    //you have to set default value for new ones
    /// <summary>
    /// ModLib settings read from mod_configs.json (new properties need a default value).
    /// </summary>
    public class ModConfigs
    {
        public DebugModeEnum DebugMode { get; set; } = DebugModeEnum.Info;
    }
}
