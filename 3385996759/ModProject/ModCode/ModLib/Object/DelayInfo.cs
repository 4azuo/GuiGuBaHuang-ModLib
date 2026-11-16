using ModLib.Mod;
using System;
using System.Reflection;

namespace ModLib.Object
{
    /// <summary>
    /// A queued delayed call of ModDelayEvent (event, method or custom action, parameter, delay, start time).
    /// </summary>
    public class DelayInfo
    {
        public ModEvent Event { get; set; }
        public MethodInfo Method { get; set; }
        public Action CustomMethod { get; set; }
        public object Parameter {  get; set; }
        public int Delay { get; set; }
        public DateTime Start { get; } = DateTime.Now;
    }
}
