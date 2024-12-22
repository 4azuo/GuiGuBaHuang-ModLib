using System.IO;
using UnityEngine;

public static class SpriteHelper
{
    private const string IMG_FOLDER = "ModImg";

    public static string GetImgFilePath(string modId, string fileName)
    {
        return Path.Combine(GetImgFolderPath(modId), fileName);
    }

    public static string GetImgFolderPath(string modId)
    {
        return Path.Combine(AssemblyHelper.GetModPathRoot(modId), IMG_FOLDER);
    }

    public static byte[] ReadImgData(string modId, string fileName)
    {
        return File.ReadAllBytes(GetImgFilePath(modId, fileName));
    }

    public static void CopyImgs(string modId)
    {
        var orgFolder = $"{AssemblyHelper.GetModPathSource(modId)}\\{IMG_FOLDER}\\";
        if (Directory.Exists(orgFolder))
        {
            Directory.CreateDirectory(ConfHelper.GetConfFolderPath(modId));
            foreach (var orgFile in Directory.GetFiles(orgFolder))
            {
                File.Copy(orgFile, ConfHelper.GetConfFilePath(modId, Path.GetFileName(orgFile)), true);
            }
        }
    }

    public static Sprite GetImage(string modId, string resourceName)
    {
        if (!File.Exists(GetImgFilePath(modId, resourceName)))
            return null;

        Texture2D texture = new Texture2D(2, 2);
        if (ImageConversion.LoadImage(texture, ReadImgData(modId, resourceName))) {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }
        return null;
    }
}