using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class DebugHelper
{
    public static readonly StringBuilder CurLog = new StringBuilder();
    public static readonly IList<Exception> Exceptions = new List<Exception>();

    public static string GetDebugFileName()
    {
        if (GameHelper.IsInGame())
        {
            return $"{g.world.playerUnit.GetUnitId()}_debug-{DateTime.Now:yyyyMMdd}.log";
        }
        else
        {
            return $"debug-{DateTime.Now:yyyyMMdd}.log"; //temp
        }
    }

    public static string GetDebugFolderName()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\debug\\";
    }

    public static string GetDebugFilePath()
    {
        return Path.Combine(GetDebugFolderName(), GetDebugFileName());
    }

    public static void CreateDebugFolder()
    {
        if (!Directory.Exists(GetDebugFolderName()))
            Directory.CreateDirectory(GetDebugFolderName());
    }

    public static void Save()
    {
        if (CurLog.Length > 0)
        {
            WriteLine();
            WriteLine();
            WriteLine();
            CreateDebugFolder();
            File.AppendAllText(GetDebugFilePath(), CurLog.ToString());
            CurLog.Clear();
        }
    }

    public static int WriteAt(int pos, string msg = "")
    {
        CurLog.Insert(pos, msg);
        return CurLog.Length;
    }

    public static int WriteLine(string msg = "", bool addTime = true)
    {
        if (addTime && !string.IsNullOrEmpty(msg))
            msg = $"{Now()} {msg}";
        CurLog.Append(msg);
        var lastPos = CurLog.Length;
        CurLog.AppendLine();
        return lastPos;
    }

    public static int Write(string msg = "", bool addTime = true)
    {
        if (addTime && !string.IsNullOrEmpty(msg))
            msg = $"{Now()} {msg}";
        CurLog.Append(msg);
        return CurLog.Length;
    }

    public static void WriteLine(Exception e)
    {
        if (Exceptions.Contains(e))
            return;
        Exceptions.Add(e);
        WriteLine($"{e.GetAllInnnerExceptionStr()}");
        Save();
    }

    private static string Now()
    {
        if (GameHelper.IsInGame())
            return $"[{DateTime.Now:HH:mm:ss.fff} ({(g.world.run.roundMonth / 12) + 1:0000}/{(g.world.run.roundMonth % 12) + 1:00}/{g.world.run.roundDay + 1:00})]";
        return $"[{DateTime.Now:HH:mm:ss.fff}]";
    }
}