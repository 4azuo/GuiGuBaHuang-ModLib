using ModLib.Object;
using System;
using System.Reflection;

//Effect3017: Shield
public static class EffectHelper
{
    public static void ShowStaticMethods()
    {
        var ass = Assembly.LoadFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\鬼谷八荒\\MelonLoader\\Managed\\Assembly-CSharp.dll");
        for (int i = 0; i < 10000; i++)
        {
            var t = ass.GetType($"Effect{i:0000}");
            if ( t != null)
            {
                DebugHelper.WriteLine($"{t.Name}");
                foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    DebugHelper.WriteLine($"　{m.Name}");
                }
            }
        }
    }
}