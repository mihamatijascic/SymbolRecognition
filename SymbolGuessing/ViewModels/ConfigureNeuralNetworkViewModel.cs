using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using SymbolGuessing.Annotations;
using SymbolGuessing.Interfaces;
using SymbolGuessing.Services;

namespace SymbolGuessing.ViewModels
{
    public class ConfigureNeuralNetworkViewModel : INotifyPropertyChanged
    {
        public IGestureRepository GestureRepository { get; set; }

        public NeuralNetwork NeuralNet { get; set; }

        public ConfigureNeuralNetworkViewModel(IGestureRepository gestureRepository, NeuralNetwork network)
        {
            GestureRepository = gestureRepository;
            NeuralNet = network;
            _numRepresentativePoints = ProcessPattern.RepresentativePoints;
            _iterations = NeuralNetwork.DefaultIterations;
            _miniBatchSize = NeuralNetwork.DefaultMiniBatchSize;
            _hiddenLayerConfiguration = NeuralNetwork.DefaultHiddenLayerConfig;
            _precision = NeuralNetwork.DefaultPrecision;
            _learningRate = NeuralNetwork.DefaultLearningRate;
        }

        private int _numRepresentativePoints;

        public int NumOfRepresentativePoints
        {
            get => _numRepresentativePoints;
            set
            {
                _numRepresentativePoints = value;
                _numRepresentativePoints = _numRepresentativePoints == 0 ? ProcessPattern.RepresentativePoints : _numRepresentativePoints;
                OnPropertyChanged();
            }
        }

        private string _hiddenLayerConfiguration;

        public string HiddenLayerConfiguration
        {
            get => _hiddenLayerConfiguration;
            set
            {
                _hiddenLayerConfiguration = value;
                _hiddenLayerConfiguration =
                    string.IsNullOrEmpty(_hiddenLayerConfiguration) ? NeuralNetwork.DefaultHiddenLayerConfig : _hiddenLayerConfiguration;
                OnPropertyChanged();
            }
        }

        private int _iterations;

        public int Iterations
        {
            get => _iterations;
            set
            {
                _iterations = value;
                _iterations = _iterations == 0 ? NeuralNetwork.DefaultIterations : _iterations;
                OnPropertyChanged();
            }
        }

        private int _miniBatchSize;

        public int MiniBatchSize
        {
            get => _miniBatchSize;
            set
            {
                _miniBatchSize = value;
                _miniBatchSize = _miniBatchSize == 0 ? NeuralNetwork.DefaultMiniBatchSize : _miniBatchSize;
                OnPropertyChanged();
            }
        }

        private double _precision;

        public double Precision
        {
            get => _precision;
            set
            {
                _precision = Math.Abs(value) <double.Epsilon ? NeuralNetwork.DefaultPrecision : value;
                OnPropertyChanged();
            }
        }

        private double _learningRate;

        public double LearningRate
        {
            get => _learningRate;
            set
            {
                _learningRate = Math.Abs(value) < double.Epsilon ? NeuralNetwork.DefaultLearningRate : value;
                OnPropertyChanged();
            }
        }

        public List<string> Algorithms { get; set; } = new List<string>()
        {
            "Batch", "MiniBatch", "Stochastic"
        };

        private string _selectedAlgorithm = "Stochastic";

        public string SelectedAlgorithm
        {
            get => _selectedAlgorithm;
            set
            {
                _selectedAlgorithm = value;
                if (_selectedAlgorithm == "MiniBatch")
                    MiniBatchSizeVisibility = true;
                else
                {
                    MiniBatchSizeVisibility = false;
                }
                OnPropertyChanged();
            }
        }

        private double _totalError = 0;

        public double TotalError
        {
            get => _totalError;
            set
            {
                _totalError = value;
                OnPropertyChanged();
            }
        }

        private int _finalIteration;

        public int FinalIteration
        {
            get => _finalIteration;
            set
            {
                _finalIteration = value;
                OnPropertyChanged();
            }
        }

        private bool _isConfigured;

        public bool IsConfigured
        {
            get => _isConfigured;
            set
            {
                _isConfigured = value;
                OnPropertyChanged();
            }
        }

        private bool _miniBatchSizeVis;

        public bool MiniBatchSizeVisibility
        {
            get => _miniBatchSizeVis;
            set
            {
                _miniBatchSizeVis = value;
                OnPropertyChanged();
            }
        }

        public delegate bool TryParse<T>(string parameter, out T result);

        public T ConvertString<T>(string parameter, TryParse<T> parseFunc)
        {
            if (string.IsNullOrEmpty(parameter) || !parseFunc(parameter, out T result))
            {
                return default;
            }

            return result;
        }

        private ICommand _configureNeuralNetwork;

        public ICommand ConfigureNeuralNetwork
        {
            get
            {
                return _configureNeuralNetwork ??= new ActionCommand(param =>
                {
                    ProcessPattern.RepresentativePoints = _numRepresentativePoints == 0 ? 30 : _numRepresentativePoints;
                    ProcessPattern.ProcessPatterns(GestureRepository.GetAllGesturesPatterns(), 
                        out List<double[]> examples,
                        out List<double[]> labels,
                        out List<string> indexOfGesture);
                    if (_selectedAlgorithm == "Batch") _miniBatchSize = examples.Count;
                    else if (_selectedAlgorithm == "Stochastic") _miniBatchSize = 1;
                    NeuralNet.GestureIndexes = indexOfGesture;
                    NeuralNet.ConfigureNeuralNetwork(_iterations, _miniBatchSize, _precision, _learningRate, _hiddenLayerConfiguration)
                        .StartLearning(examples, labels, out _totalError, out _finalIteration);
                    OnPropertyChanged(nameof(TotalError));
                    OnPropertyChanged(nameof(FinalIteration));
                    IsConfigured = true;
                });
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
