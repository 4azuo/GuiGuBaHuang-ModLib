using ModCreator.Attributes;
using ModCreator.Commons;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;
using System.Reflection;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Base class for window data (business logic layer).
    /// 
    /// IMPORTANT NOTES:
    /// - Inherits from AutoNotifiableObject, so property changes are automatically tracked
    /// - DO NOT manually call OnPropertyChanged() in derived classes - it's handled automatically
    /// - Use [NotifyMethod] attribute to trigger methods when properties change
    /// - Properties will automatically notify UI bindings without explicit calls
    /// - DO NOT use private methods in derived classes - use public methods instead for [NotifyMethod] attribute to work
    /// 
    /// Lifecycle:
    /// 1. New() - Called when window data is created (optional override)
    /// 2. OnLoad() - Called when window is loaded (optional override)
    /// 3. OnDestroy() - Called when window is closing (optional override)
    /// </summary>
    public abstract class CWindowData : AutoNotifiableObject
    {
        /// <summary>
        /// Parent window reference
        /// </summary>
        [JsonIgnore, IgnoredProperty]
        public Window Window { get; private set; }

        /// <summary>
        /// Event: Called when window data is created
        /// </summary>
        public virtual void New() { }

        /// <summary>
        /// Event: Called when window is loaded
        /// </summary>
        public virtual void OnLoad() { }

        /// <summary>
        /// Event: Called when window is closing
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// Initialize window reference
        /// </summary>
        /// <param name="iWindow"></param>
        public virtual void InitWindow(Window iWindow, CancelEventArgs initDataFlg)
        {
            Window = iWindow;
            this.GetType().GetProperty("LastInstance", BindingFlags.Public | BindingFlags.Static)?.SetValue(this, this);

            //load
            if (!initDataFlg.Cancel)
            {
                Pause();
                OnLoad();
                Unpause();
            }
        }

        ~CWindowData()
        {
            Pause();
            OnDestroy();
        }
    }
}