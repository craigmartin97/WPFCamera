using System;
using System.Windows.Input;

namespace Camera
{
    /// <summary>
    /// Generic Command that is reusable
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Method or function that is to be executed.
        /// </summary>
        private readonly Action<object> execute;

        /// <summary>
        /// Method or function to be executed and bool value indicating if it can be executed.
        /// </summary>
        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// CanExecuteChanged automatically picks up changes that
        /// could affect the outcome of our CanExecute Method
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Creates a new instance of the RelayCommand. 
        /// Sets the execute and canExecute.
        /// </summary>
        /// <param name="execute">Method or object to execute</param>
        /// <param name="canExecute">Can execute, defaults to true</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Determinds whether the command can be executed or not
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        /// <summary>
        /// Executes a command or function.
        /// </summary>
        /// <param name="parameter">Method or function to execute</param>
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
