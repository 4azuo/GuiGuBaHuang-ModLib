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

    public static T GetEfxValue<T>(this ConfRoleEffectItem roleEfx, int index)
    {
        var info = roleEfx.value.Split('_');
        return info[index].Parse<T>();
    }

    public static void SetEfxValue<T>(this ConfRoleEffectItem roleEfx, int index, T value)
    {
        var info = roleEfx.value.Split('_');
        info[index] = value.ToString();
        roleEfx.value = string.Join("_", info);
    }
}