using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SymbolGuessing.Annotations;
using SymbolGuessing.Commands;
using SymbolGuessing.Services;
using SymbolGuessing.Views;

namespace SymbolGuessing.ViewModels
{
    public class PredictGestureViewModel : INotifyPropertyChanged
    {
        public NeuralNetwork NeuralNet { get; set; }

        public PredictGestureViewModel(NeuralNetwork neuralNetwork)
        {
            NeuralNet = neuralNetwork;
        }

        private string _prediction;

        public string Prediction
        {
            get => _prediction;
            set
            {
                _prediction = value;
                OnPropertyChanged();
            }
        }

        private List<KeyValuePair<string, double>> _gesturesPercentages = new List<KeyValuePair<string, double>>();

        public List<KeyValuePair<string, double>> GesturesPercentages
        {
            get => _gesturesPercentages;
            set
            {
                _gesturesPercentages = value;
                OnPropertyChanged();
            }
        }

        private ActionCommand _leftMouseDownCommand;
        private ActionCommand _mouseMoveCommand;
        private ActionCommand _leftMouseUpCommand;

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

            _state = GestureCanvasState.TRACKINGPOINTS;
            _gesturePoints = new List<Point>();
            _gesturePoints.Add(eventArgs.GetPosition((IInputElement)eventArgs.Source));
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
                            _gesturePoints.Add(eventArgs.GetPosition((IInputElement)eventArgs.Source));
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
                    if (param is MouseEventArgs eventArgs)
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

                double[] example = ProcessPattern.ProcessOnePattern(_gesturePoints);
                double[] prediction = NeuralNet.Predict(example);
                ChangeGesturesPercentages(prediction);

                Prediction = CreateStringPrediction(prediction);
            }
        }

        private void ChangeGesturesPercentages(double[] prediction)
        {
            _gesturesPercentages = new List<KeyValuePair<string, double>>();

            for (int index = 0; index < prediction.Length; index++)
            {
                KeyValuePair<string, double> item =
                    new KeyValuePair<string, double>(NeuralNet.GestureIndexes[index], prediction[index] * 100);
                _gesturesPercentages.Add(item);
            }
            OnPropertyChanged(nameof(GesturesPercentages));
        }

        private string CreateStringPrediction(double[] prediction)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < prediction.Length; index++)
            {
                //double percentage = prediction * 100;
                builder.Append(NeuralNet.GestureIndexes[index]).Append(": ");
                builder.Append((prediction[index] * 100).ToString()).Append("\n");
            }

            return builder.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
