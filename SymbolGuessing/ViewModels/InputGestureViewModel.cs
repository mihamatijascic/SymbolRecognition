using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SymbolGuessing.Annotations;
using SymbolGuessing.Commands;
using SymbolGuessing.Interfaces;
using SymbolGuessing.Model;
using SymbolGuessing.Views;
using Point = System.Windows.Point;

namespace SymbolGuessing.ViewModels
{
    public class InputGestureViewModel : INotifyPropertyChanged
    {
        private string _newGestureName = "nova gesta";
        public string NewGestureName
        {
            get => _newGestureName;
            set
            {
                _newGestureName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Gesture> _gestures;

        public ObservableCollection<Gesture> Gestures
        {
            get => _gestures ??= GestureRepository.GetObservableGestures();
            set
            {
                _gestures = value;
                OnPropertyChanged();
            }
        }

        private Gesture _selectedGesture;

        public Gesture SelectedGesture
        {
            get => _selectedGesture;
            set
            {
                if (value != null)
                {
                    _selectedGesture = value;
                    OnPropertyChanged();
                }
            }
        }

        public IGestureRepository GestureRepository { get; set; }

        public InputGestureViewModel(IGestureRepository gestureRepository)
        {
            GestureRepository = gestureRepository;
        }

        private ActionCommand _saveGestureCommand;
        private ActionCommand _deleteGestureCommand;
        private ActionCommand _refreshGesturesCommand;
        private ActionCommand _deleteLastPatternCommand;
        private ActionCommand _leftMouseDownCommand;
        private ActionCommand _mouseMoveCommand;
        private ActionCommand _leftMouseUpCommand;

        public ICommand SaveGestureCommand
        {
            get
            {
                return _saveGestureCommand ??= new ActionCommand(param =>
                {
                    if (!string.IsNullOrEmpty(_newGestureName))
                    {
                        GestureRepository.CreateGesture(_newGestureName);
                        Gestures = GestureRepository.GetObservableGestures();
                    }
                });
            }
        }

        public ICommand DeleteGestureCommand
        {
            get
            {
                return _deleteGestureCommand ??= new ActionCommand(param =>
                {
                    if (_selectedGesture != null)
                    {
                        GestureRepository.RemoveGesture(_selectedGesture.Name);
                        Gestures.Remove(_selectedGesture);
                    }
                });
            }
        }

        public ICommand RefreshGesturesCommand
        {
            get
            {
                return _refreshGesturesCommand ??= new ActionCommand(param =>
                {
                    Gestures = GestureRepository.GetObservableGestures();
                });
            }
        }

        public ICommand DeleteLastPatternCommand
        {
            get
            {
                return _deleteLastPatternCommand ??= new ActionCommand(param =>
                {
                    GestureRepository.DeleteLastPattern(SelectedGesture.Name);
                    Gestures = GestureRepository.GetObservableGestures();
                });
            }
        }

        private List<Point> _gesturePoints;
        private GestureCanvasState _state = GestureCanvasState.WAITING;

        public ICommand LeftMouseDownCommand
        {
            get
            {
                return _leftMouseDownCommand ??= new ActionCommand(param =>
                {
                    if (param is MouseEventArgs eventArgs)
                    {
                        StartTrackingPoints(eventArgs);
                    }
                });
            }
        }

        private void StartTrackingPoints(MouseEventArgs eventArgs)
        {
            InkCanvas canvas = (InkCanvas)eventArgs.Source;
            canvas.Children.Clear();
            canvas.Strokes.Clear();

            if (_selectedGesture != null)
            {
                _state = GestureCanvasState.TRACKINGPOINTS;
                _gesturePoints = new List<Point>();
                _gesturePoints.Add(eventArgs.GetPosition((IInputElement) eventArgs.Source));
            }
        }

        public ICommand MouseMoveCommand
        {
            get
            {
                return _mouseMoveCommand ??= new ActionCommand(param =>
                {
                    if (param is MouseEventArgs eventArgs)
                    {
                        if (_state == GestureCanvasState.TRACKINGPOINTS)
                        {
                            _gesturePoints.Add(eventArgs.GetPosition((IInputElement) eventArgs.Source));
                        }
                    }
                });
            }
        }

        public ICommand LeftMouseUpCommand
        {
            get
            {
                return _leftMouseUpCommand ??= new ActionCommand(param =>
                {
                    if(param is MouseEventArgs eventArgs)
                    {
                        EndTrackingPoints(eventArgs);
                    }
                });
            }
        }

        private void EndTrackingPoints(MouseEventArgs eventArgs)
        {
            if (_state == GestureCanvasState.TRACKINGPOINTS)
            {
                _state = GestureCanvasState.WAITING;
                _gesturePoints.Add(eventArgs.GetPosition((IInputElement)eventArgs.Source));
                GestureRepository.AddPattern(SelectedGesture.Name, _gesturePoints);
                RefreshGesturesCommand.Execute(null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
