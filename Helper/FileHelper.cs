using ModLib.Object;
using Newtonsoft.Json;
using System.IO;

public static class FileHelper
{
    public static bool IsReadable<T>(string filePath) where T : CachableObject
    {
        try
        {
            JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
