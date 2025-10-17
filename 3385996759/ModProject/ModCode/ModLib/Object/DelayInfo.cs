using ModLib.Mod;
using System;
using System.Reflection;

namespace ModLib.Object
{
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
