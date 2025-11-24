using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for debugging - outputs to Console, Debug, Log file, and MessageBox
    /// </summary>
    public static class DebugHelper
    {
        private static readonly object _logLock = new object();
        private static string _logFilePath;

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }

        static DebugHelper()
        {
            InitializeLogFile();
        }

        private static void InitializeLogFile()
        {
            try
            {
                var logDir = Constants.LogsDir;
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                _logFilePath = Path.Combine(logDir, $"debug-{DateTime.Now:yyyy-MM-dd}.log");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }

        #region Public Methods

        /// <summary>
        /// Log debug message
        /// </summary>
        public static void Log(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogMessage(LogLevel.Debug, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public static void Info(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogMessage(LogLevel.Info, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void Warning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogMessage(LogLevel.Warning, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public static void Error(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogMessage(LogLevel.Error, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Log error with exception
        /// </summary>
        public static void Error(Exception exception, string additionalMessage = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var message = string.IsNullOrEmpty(additionalMessage)
                ? exception.ToString()
                : $"{additionalMessage}\n{exception}";
            
            LogMessage(LogLevel.Error, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Log fatal error message
        /// </summary>
        public static void Fatal(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogMessage(LogLevel.Fatal, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Log fatal error with exception
        /// </summary>
        public static void Fatal(Exception exception, string additionalMessage = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var message = string.IsNullOrEmpty(additionalMessage)
                ? exception.ToString()
                : $"{additionalMessage}\n{exception}";
            
            LogMessage(LogLevel.Fatal, message, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Show error in MessageBox and log it
        /// </summary>
        public static void ShowError(string message, string title = "Error")
        {
            Error(message);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        /// <summary>
        /// Show error with exception in MessageBox and log it
        /// </summary>
        public static void ShowError(Exception exception, string title = "Error", string additionalMessage = "")
        {
            Error(exception, additionalMessage);
            
            var displayMessage = string.IsNullOrEmpty(additionalMessage)
                ? $"An error occurred:\n\n{exception.Message}"
                : $"{additionalMessage}\n\n{exception.Message}";

            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(displayMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        /// <summary>
        /// Show warning in MessageBox and log it
        /// </summary>
        public static void ShowWarning(string message, string title = "Warning")
        {
            Warning(message);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        /// <summary>
        /// Show info in MessageBox and log it
        /// </summary>
        public static void ShowInfo(string message, string title = "Information")
        {
            Info(message);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        #endregion

        #region Private Methods

        private static void LogMessage(LogLevel level, string message, string memberName, string filePath, int lineNumber)
        {
            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpper().PadRight(7);
            var location = $"{fileName}::{memberName}:{lineNumber}";
            var formattedMessage = $"[{timestamp}] [{levelStr}] [{location}] {message}";

            // Output to Console
            Console.WriteLine(formattedMessage);

            // Output to Debug (Visual Studio Output window)
            Debug.WriteLine(formattedMessage);

            // Output to Log File
            WriteToLogFile(formattedMessage);
        }

        private static void WriteToLogFile(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_logLock)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear old log files (keep last N days)
        /// </summary>
        public static void CleanupOldLogs(int keepDays = 7)
        {
            try
            {
                var logDir = Path.Combine(Constants.RootDir, "ModCreator", "Logs");
                if (!Directory.Exists(logDir))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-keepDays);
                var logFiles = Directory.GetFiles(logDir, "*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(logFile);
                            Debug.WriteLine($"Deleted old log file: {logFile}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to delete log file {logFile}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to cleanup old logs: {ex.Message}");
            }
        }

        #endregion
    }
}