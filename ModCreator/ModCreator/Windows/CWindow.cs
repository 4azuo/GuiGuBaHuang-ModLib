using ModCreator.Attributes;
using ModCreator.WindowData;
using System.ComponentModel;
using System.Reflection;
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
        [IgnoredProperty]
        public T WindowData { get; private set; }

        /// <summary>
        /// Override only when you need custom initialization logic.
        /// Base implementation creates new T() and calls WindowData.New() automatically.
        /// </summary>
        public virtual T InitData(CancelEventArgs e) { return new T(); }

        /// <summary>
        /// Handles the event triggered when a window is closing.
        /// </summary>
        /// <remarks>Use the <see cref="CancelEventArgs.Cancel"/> property to prevent the window from
        /// closing if necessary.</remarks>
        /// <param name="s">The source of the event, typically the window being closed.</param>
        /// <param name="e">An instance of <see cref="CancelEventArgs"/> that allows the operation to be canceled.</param>
        public virtual void ClosingWindow(object s, CancelEventArgs e) { }

        public void ClearData()
        {
            WindowData = new T();
            WindowData.InitWindow(this, new CancelEventArgs(false));
            WindowData.NotifyAll();
        }

        public CWindow()
        {
            this.GetType().GetProperty("LastInstance", BindingFlags.Public | BindingFlags.Static)?.SetValue(this, this);
            GetType().GetMethod(COMP_INIT_METHOD).Invoke(this, null);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var initDataFlg = new CancelEventArgs(false);
            WindowData = InitData(initDataFlg);
            DataContext = WindowData;
            
            Loaded += (s, e) =>
            {
                WindowData.InitWindow(this, initDataFlg);
            };

            Closing += (s, e) =>
            {
                ClosingWindow(s, e);
            };
        }
    }
}