namespace ModLib.Object
{
    public class MultiValue
    {
        public object[] Values { get; set; }

        public MultiValue()
        {
        }

        public static MultiValue Create(params object[] values)
        {
            return new MultiValue()
            {
                Values = values
            };
        }
    }
}
