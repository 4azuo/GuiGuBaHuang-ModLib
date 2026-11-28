using ModLib.Mod;

namespace MOD_flQrxq
{
    /// <summary>
    /// ModLib mod entry (MOD_flQrxq): the concrete ModMaster with mod name, id and version.
    /// </summary>
    public sealed class ModMain : ModMaster
    {
        /// <summary>The support page of ModLib: the "Support" window of the title screen (SupportHelper).</summary>
        public const string SUPPORT_URL = "https://ko-fi.com/fouru";
        public const string SUPPORT_QR = "fouru-kofi-qr.png";
        public const string SUPPORT_NAME = "ModLib";
        public const string SUPPORT_INFO = "libtxt999990067";

        public static new ModMain ModObj => ModMaster.ModObj as ModMain;
        public override string ModName => "MOD_flQrxq";
        public override string ModId => "flQrxq";
        public override string Version => "2.3.07";
    }
}
