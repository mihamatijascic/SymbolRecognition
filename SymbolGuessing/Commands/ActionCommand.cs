using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SymbolGuessing.Commands
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;

        public ActionCommand(Action<object> action) : this(action, null)
        {

        }

        public ActionCommand(Action<object> action, Predicate<object> canExecute)
        {
            _action = action;
           _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _action(parameter);

        private EventHandler _eventHandler;

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                _eventHandler += value;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                _eventHandler -= value;
                CommandManager.RequerySuggested -= value;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            _eventHandler?.Invoke(this, new EventArgs());
        }
    }
}
