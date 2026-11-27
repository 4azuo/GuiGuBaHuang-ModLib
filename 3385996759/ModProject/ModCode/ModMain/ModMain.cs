using ModLib.Mod;

namespace MOD_flQrxq
{
    /// <summary>
    /// ModLib mod entry (MOD_flQrxq): the concrete ModMaster with mod name, id and version.
    /// </summary>
    public sealed class ModMain : ModMaster
    {
        public static new ModMain ModObj => ModMaster.ModObj as ModMain;
        public override string ModName => "MOD_flQrxq";
        public override string ModId => "flQrxq";
        public override string Version => "2.3.06";
    }
}
