using ModCreator.Attributes;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace ModCreator.Commons
{
    [NotifyMethod("WriteHistory")]
    public abstract class HistorableObject : AutoNotifiableObject
    {
        public const int MAX_HIST_TIMES = 5;

        [JsonIgnore, IgnoredProperty]
        public List<string> Histories { get; } = new List<string>();
        public void WriteHistory(object obj, PropertyInfo prop, object oldValue, object newValue) {
            if (IsUpdated())
            {
                Histories.Add(JsonConvert.SerializeObject(this));
                if (Histories.Count > MAX_HIST_TIMES)
                {
                    Histories.RemoveAt(0);
                }
            }
        }

        public bool IsUpdated()
        {
            return Histories.Count == 0 || JsonConvert.SerializeObject(this) != Histories.Last();
        }
    }
}