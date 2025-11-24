using ModCreator.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ModCreator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load settings
            SettingHelper.EnsureLoaded();

            // Cleanup old logs
            DebugHelper.CleanupOldLogs(keepDays: 7);

            // Setup global exception handlers
            SetupExceptionHandling();

            DebugHelper.Info("Application started");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DebugHelper.Info($"Application exiting with code: {e.ApplicationExitCode}");
            base.OnExit(e);
        }

        private void SetupExceptionHandling()
        {
            // Handle UI thread exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Handle non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Handle Task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DebugHelper.Error(e.Exception, "Unhandled UI thread exception");

            // // Prevent default unhandled exception processing
            // e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            
            if (e.IsTerminating)
            {
                DebugHelper.Fatal(exception, "Fatal unhandled exception - application will terminate");
            }
            else
            {
                DebugHelper.Error(exception, "Unhandled AppDomain exception");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DebugHelper.Error(e.Exception, "Unobserved task exception");
            
            // // Prevent the exception from crashing the application
            // e.SetObserved();
        }
    }
}