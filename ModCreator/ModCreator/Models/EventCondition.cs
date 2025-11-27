using System.ComponentModel;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a condition in ModEvent
    /// </summary>
    public class EventCondition : INotifyPropertyChanged
    {
        private string _name;
        private string _code;
        private int _order;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Condition name/type
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
        /// Description of the condition
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// C# code for the condition
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
        /// Order/position in the condition list
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
