using Newtonsoft.Json;

namespace ModLib.Object
{
    public class MultiValue
    {
        public dynamic[] Values { get; set; }
        [JsonIgnore]
        public dynamic Value0 => Values?.Length > 0 ? Values[0] : null;
        [JsonIgnore]
        public dynamic Value1 => Values?.Length > 1 ? Values[1] : null;
        [JsonIgnore]
        public dynamic Value2 => Values?.Length > 2 ? Values[2] : null;

        public MultiValue()
        {
        }

        public static MultiValue Create(params dynamic[] values)
        {
            return new MultiValue()
            {
                Values = values
            };
        }

        public dynamic Get(int index)
        {
            return Values?.Length > index ? Values[index] : null;
        }
    }
}
