using System.ComponentModel;

namespace Camera.ViewModels
{
    /// <summary>
    /// View Model Base Implements INotifyPropertyChanged and holds all 
    /// logic that all VM's need.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Properties
        private int _fontSize = 36;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        #endregion
        #region Notify Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Tells the event, PropertyChanged, that a property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
