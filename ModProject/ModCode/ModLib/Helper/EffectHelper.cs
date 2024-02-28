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

    public static int GetEfx3Value(this ConfRoleEffectItem roleEfx)
    {
        var info = roleEfx.value.Split('_');
        return info[2].Parse<int>();
    }

    public static void SetEfx3Value(this ConfRoleEffectItem roleEfx, int value)
    {
        var info = roleEfx.value.Split('_');
        info[2] = value.ToString();
        roleEfx.value = string.Join("_", info);
    }
}