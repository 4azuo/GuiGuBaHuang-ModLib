using ModCreator.Commons;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Input window data layer
    /// </summary>
    public class InputWindowData : CWindowData
    {
        /// <summary>
        /// Window title
        /// </summary>
        public string WindowTitle { get; set; } = "Input";

        /// <summary>
        /// Label for the input field
        /// </summary>
        public string Label { get; set; } = "Value:";

        /// <summary>
        /// Input value
        /// </summary>
        public string InputValue { get; set; } = string.Empty;
    }
}
