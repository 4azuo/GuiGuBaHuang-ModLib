namespace MOD_nE7UL2.Object
{
    public class CustomEffect
    {
        public string Id { get; set; }
        public string Efx { get; set; }
        public EfxType Type { get; set; }

        public enum EfxType
        {
            Direct,
            Instant
        }
    }
}
