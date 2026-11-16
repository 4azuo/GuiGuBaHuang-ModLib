namespace MOD_nE7UL2.Object
{
    /// <summary>
    /// Visual effect definition read from _Cus_Res_Efx_Battle_Skill.json.
    /// </summary>
    public class CustomEffect
    {
        public string Id { get; set; }
        public string Efx { get; set; }
        public EfxType Type { get; set; }

        /// <summary>
        /// Effect kind.
        /// </summary>
        public enum EfxType
        {
            Direct,
            Instant
        }
    }
}
