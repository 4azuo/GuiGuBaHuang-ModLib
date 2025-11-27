namespace ModCreator.Models
{
    /// <summary>
    /// Represents condition metadata from JSON
    /// </summary>
    public class ConditionInfo
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// Display text for UI: "Category - DisplayName"
        /// </summary>
        public string DisplayText => string.IsNullOrEmpty(Category) ? DisplayName : $"{Category} - {DisplayName}";
    }
}
