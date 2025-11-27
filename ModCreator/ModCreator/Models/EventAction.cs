using System.ComponentModel;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents an action in ModEvent
    /// </summary>
    public class EventAction : INotifyPropertyChanged
    {
        private string _name;
        private string _code;
        private int _order;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Action name/type
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Display name for UI
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description of the action
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// C# code for the action
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged(nameof(Code));
                }
            }
        }

        /// <summary>
        /// Order/position in the action list
        /// </summary>
        public int Order
        {
            get => _order;
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged(nameof(Order));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
