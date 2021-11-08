using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SymbolGuessing.Annotations;
using SymbolGuessing.Interfaces;
using SymbolGuessing.Services;

namespace SymbolGuessing.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        public IGestureRepository GestureRepository { get; set; }
        public NeuralNetwork NeuralNet { get; set; }
        
        public MainWindowViewModel(IGestureRepository repository, NeuralNetwork neuralNetwork)
        {
            GestureRepository = repository;
            NeuralNet = neuralNetwork;
            GestureRepository.RepositoryChangeEvent += ConfigureNetworkVisibilityChanged;
            NeuralNet.NeuralNetworkConfigured += PredictGestureVisibilityChanged;
            _configureNetworkVisibility = GestureRepository.AllPatternCount() > 0;
        }

        private bool _configureNetworkVisibility;

        public bool ConfigureNetworkVisibility
        {
            get => _configureNetworkVisibility;
            set
            {
                _configureNetworkVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool _predictGestureVisibility;

        public bool PredictGestureVisibility
        {
            get => _predictGestureVisibility;
            set
            {
                _predictGestureVisibility = value;
                OnPropertyChanged();
            }
        }

        public void ConfigureNetworkVisibilityChanged()
        {
            ConfigureNetworkVisibility = GestureRepository.AllPatternCount() > 0;
        }

        public void PredictGestureVisibilityChanged()
        {
            PredictGestureVisibility = NeuralNet.IsNeuralNetworkConfigured;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
