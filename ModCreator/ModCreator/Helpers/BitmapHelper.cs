using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for BitmapImage operations
    /// </summary>
    public static class BitmapHelper
    {
        /// <summary>
        /// Load BitmapImage from file path without locking the file.
        /// Returns null if file doesn't exist or cannot be loaded.
        /// </summary>
        /// <param name="filePath">Absolute path to the image file</param>
        /// <returns>BitmapImage or null</returns>
        public static BitmapImage LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load into memory, release file handle
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze(); // Make it cross-thread accessible and ensure file is released
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Load BitmapImage from file path with custom decode pixel width for thumbnails.
        /// This is useful for displaying large images in smaller controls to save memory.
        /// Returns null if file doesn't exist or cannot be loaded.
        /// </summary>
        /// <param name="filePath">Absolute path to the image file</param>
        /// <param name="decodePixelWidth">Width in pixels to decode (height will be calculated proportionally)</param>
        /// <returns>BitmapImage or null</returns>
        public static BitmapImage LoadFromFileWithSize(string filePath, int decodePixelWidth)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = decodePixelWidth; // Decode to smaller size
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if a file is a valid image file by attempting to load it.
        /// </summary>
        /// <param name="filePath">Absolute path to the file</param>
        /// <returns>True if file is a valid image, false otherwise</returns>
        public static bool IsValidImageFile(string filePath)
        {
            return LoadFromFile(filePath) != null;
        }
    }
}
