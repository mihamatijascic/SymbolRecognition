using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SymbolGuessing.Annotations;

namespace SymbolGuessing.Model
{
    public class Gesture : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private int _numberOfPatterns = 0;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfPatterns
        {
            get => _numberOfPatterns;
            set
            {
                _numberOfPatterns = value;
                OnPropertyChanged();
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Gesture gesture)
            {
                if (this.Name.Equals(gesture.Name)) return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode() * 17 + _numberOfPatterns.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
