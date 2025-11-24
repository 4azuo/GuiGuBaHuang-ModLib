namespace ModCreator.Models
{
    /// <summary>
    /// Image extension model
    /// </summary>
    public class ImageExtension
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Desc { get; set; }

        public override string ToString() => Extension;
    }
}
