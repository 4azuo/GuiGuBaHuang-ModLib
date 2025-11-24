namespace ModCreator.Models
{
    /// <summary>
    /// Variable type model
    /// </summary>
    public class VarType
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Desc { get; set; }

        public override string ToString() => Type;
    }
}
