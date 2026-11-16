using Newtonsoft.Json;

namespace ModLib.Object
{
    //Dont change "object" to "dynamic"
    /// <summary>
    /// Serializable tuple of object values (used for data tables in configs/enums).
    /// </summary>
    public class MultiValue
    {
        public object[] Values { get; set; }
        [JsonIgnore]
        public object Value0 => Values?.Length > 0 ? Values[0] : null;
        [JsonIgnore]
        public object Value1 => Values?.Length > 1 ? Values[1] : null;
        [JsonIgnore]
        public object Value2 => Values?.Length > 2 ? Values[2] : null;

        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public MultiValue()
        {
        }

        /// <summary>
        /// Creates a MultiValue from values.
        /// </summary>
        public static MultiValue Create(params object[] values)
        {
            return new MultiValue()
            {
                Values = values
            };
        }

        /// <summary>
        /// Value at index, or null.
        /// </summary>
        public object Get(int index)
        {
            return Values?.Length > index ? Values[index] : null;
        }
    }
}
