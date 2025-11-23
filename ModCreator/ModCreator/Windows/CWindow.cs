using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    /// <summary>
    /// Base window class with separated data layer
    /// 
    /// USAGE RULES:
    /// 1. DO NOT define constructors in derived classes
    ///    - CWindow automatically calls InitializeComponent() via reflection
    /// 
    /// 2. DO NOT override InitData() unless you need custom initialization logic
    ///    - Base implementation already creates new T() and calls WindowData.New()
    ///    - Only override when you need special setup (e.g., load settings, pass parameters)
    /// 
    /// 3. Only define event handlers for actions (button clicks, etc.)
    ///    - UI state changes should be handled through data binding
    /// </summary>
    public abstract class CWindow<T> : Window where T : CWindowData, new()
    {
        private const string COMP_INIT_METHOD = "InitializeComponent";

        /// <summary>
        /// Window data (business logic)
        /// </summary>
        public T WindowData { get; private set; }

        /// <summary>
        /// Override only when you need custom initialization logic.
        /// Base implementation creates new T() and calls WindowData.New() automatically.
        /// </summary>
        public virtual T InitData(CancelEventArgs e)
        {
            var data = new T();
            data.New();
            return data;
        }

        /// <summary>
        /// Override to handle window closing
        /// </summary>
        public virtual void ClosingWindow(object s, CancelEventArgs e) { }

        public CWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CWindow] Initializing {GetType().Name}");
                
                // Call InitializeComponent
                var initMethod = GetType().GetMethod(COMP_INIT_METHOD);
                if (initMethod == null)
                {
                    throw new InvalidOperationException(
                        $"InitializeComponent method not found on type {GetType().FullName}. " +
                        "Make sure the XAML file is properly configured.");
                }
                
                initMethod.Invoke(this, null);
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                System.Diagnostics.Debug.WriteLine($"[CWindow] InitializeComponent completed for {GetType().Name}");

                // Initialize data
                var initDataFlg = new CancelEventArgs(false);
                WindowData = InitData(initDataFlg);
                DataContext = WindowData;

                System.Diagnostics.Debug.WriteLine($"[CWindow] WindowData initialized for {GetType().Name}");

                // Wire up events
                Loaded += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[CWindow] Window Loaded event for {GetType().Name}");
                    WindowData.InitWindow(this, initDataFlg);
                };

                Closing += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[CWindow] Window Closing event for {GetType().Name}");
                    ClosingWindow(s, e);
                };
                
                System.Diagnostics.Debug.WriteLine($"[CWindow] Constructor completed successfully for {GetType().Name}");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error initializing window {GetType().Name}:\n\n{ex.Message}\n\n{ex.StackTrace}";
                System.Diagnostics.Debug.WriteLine($"[CWindow] ERROR: {errorMsg}");
                MessageBox.Show(
                    errorMsg,
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }
    }
}
