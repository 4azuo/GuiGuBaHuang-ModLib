using ModLib.Attributes;
using System.IO;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for loading and managing sprites and images.
    /// Provides utilities for loading images from ModImg folder into Unity sprites.
    /// </summary>
    [ActionCat("Sprite")]
    public static class SpriteHelper
    {
        private const string IMG_FOLDER = "ModImg";

        /// <summary>
        /// Gets the full path to an image file in ModImg folder.
        /// </summary>
        /// <param name="modId">Mod ID</param>
        /// <param name="fileName">Image filename</param>
        /// <returns>Full file path</returns>
        public static string GetImgFilePath(string modId, string fileName)
        {
            return Path.Combine(GetImgFolderPath(modId), fileName);
        }

        /// <summary>
        /// Gets the ModImg folder path for a mod.
        /// </summary>
        /// <param name="modId">Mod ID</param>
        /// <returns>Folder path</returns>
        public static string GetImgFolderPath(string modId)
        {
            return Path.Combine(AssemblyHelper.GetModPathRoot(modId), IMG_FOLDER);
        }

        /// <summary>
        /// Reads image file as byte array.
        /// </summary>
        /// <param name="modId">Mod ID</param>
        /// <param name="fileName">Image filename</param>
        /// <returns>Image bytes</returns>
        public static byte[] ReadImgData(string modId, string fileName)
        {
            return File.ReadAllBytes(GetImgFilePath(modId, fileName));
        }

        /// <summary>
        /// Copies all images from source ModImg to config folder.
        /// </summary>
        /// <param name="modId">Mod ID</param>
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

        /// <summary>
        /// Loads an image file as Unity Sprite.
        /// </summary>
        /// <param name="modId">Mod ID</param>
        /// <param name="resourceName">Image filename</param>
        /// <returns>Sprite or null if load fails</returns>
        public static Sprite GetImage(string modId, string resourceName)
        {
            if (!File.Exists(GetImgFilePath(modId, resourceName)))
                return null;

            Texture2D texture = new Texture2D(2, 2);
            if (ImageConversion.LoadImage(texture, ReadImgData(modId, resourceName)))
            {
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
            return null;
        }
    }
}