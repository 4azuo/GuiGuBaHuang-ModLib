namespace ModLib.Object
{
    /// <summary>
    /// Items whose text is a format string filled by the UIItemWork formatter.
    /// </summary>
    public interface ITextFormat
    {
        string GetFormat();
        void SetFormat(string format);
    }
}