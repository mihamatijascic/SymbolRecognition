using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolGuessing.Services
{
    public class NeuralNetwork
    {
        public static Random Rand = new Random();
        public static double RandomLowerbound = -0.4;
        public static double RandomUpperbound = 0.4;
        public static string DefaultHiddenLayerConfig = "4x3";
        public static int DefaultIterations = 10000;
        public static int DefaultMiniBatchSize = 15;
        public static double DefaultPrecision = 0.0001;
        public static double DefaultLearningRate = 0.2;

        public List<string> GestureIndexes { get; set; }

        public string HiddenLayerConfig { get; set; } = NeuralNetwork.DefaultHiddenLayerConfig;
        public int Iterations { get; set; } = NeuralNetwork.DefaultIterations;
        public int MiniBatchSize { get; set; } = NeuralNetwork.DefaultMiniBatchSize;
        public double Precision { get; set; } = NeuralNetwork.DefaultPrecision;
        public double LearningRate { get; set; } = NeuralNetwork.DefaultLearningRate;
        public bool IsNeuralNetworkConfigured { get; private set; } = false;

        public delegate void NeuralNetworkChanged();

        public event NeuralNetworkChanged NeuralNetworkConfigured;

        private void OnNeuralNetworkConfigured()
        {
            NeuralNetworkConfigured?.Invoke();
        }

        public NeuralNetwork()
        {
            
        }

        private List<int> _neuronsPerLayer;
        private List<double[]> _biases;
        private List<double[]> _currentDeltaNeuronOut;
        private List<double[]> _totalDeltaBiases;
        private List<double[,]> _weights;
        private List<double[,]> _totalDeltaWeights;
        private List<double[]> _outputs;

        public NeuralNetwork ConfigureNeuralNetwork(int iterations = 1000, int miniBatchSize = 1, double precision = 1e6,
            double learningRate = 0.01, string hiddenLayerConfig = "3x2")
        {
            this.Iterations = iterations;
            this.MiniBatchSize = miniBatchSize;
            this.Precision = precision;
            this.LearningRate = learningRate;
            this.HiddenLayerConfig = hiddenLayerConfig;
            return this;
        }

        public void StartLearning(List<double[]> examples, List<double[]> labels, out double totalError, out int FinalIteration)
        {
            IsNeuralNetworkConfigured = false;
            InputDataValidation(examples, labels);
            SetNeuronsPerLayer(HiddenLayerConfig, examples[0].Length, labels[0].Length);
            InitializeData();

            Learning(examples, labels, out totalError, out FinalIteration);
            IsNeuralNetworkConfigured = true;
            OnNeuralNetworkConfigured();
        }

        private void Learning(List<double[]> examples, List<double[]> labels, out double totalError, out int finalIteration)
        {
            int iteration;
            totalError = 0;
            for (iteration = 0; iteration < Iterations; iteration++)
            {
                totalError = 0;
                Shuffle(examples, labels);
                bool areParametersUpdated = false;

                for (int index = 0; index < examples.Count; index++)
                {
                    areParametersUpdated = false;
                    double[] prediction = Predict(examples[index]);
                    totalError += CalculatePredictionError(prediction, labels[index]);
                    UpdateDeltaParameters(labels[index]);
                    
                    if (index + 1 % MiniBatchSize == 0)
                    {
                        UpdateWeightsAndBiases();
                        ResetDeltaParameters();
                        areParametersUpdated = true;
                    }
                }

                if (!areParametersUpdated)
                {
                    UpdateWeightsAndBiases();
                    ResetDeltaParameters();
                }

                totalError /= (2 * (double) examples.Count);
                if(Math.Abs(totalError) < Precision) break;
            }

            finalIteration = iteration;
        }

        private double CalculatePredictionError(double[] prediction, double[] label)
        {
            double error = 0;
            for (int index = 0; index < prediction.Length; index++)
            {
                error += (label[index] - prediction[index]) * (label[index] - prediction[index]);
            }

            return error;
        }

        private void UpdateWeightsAndBiases()
        {
            for (int layer = 1; layer < _neuronsPerLayer.Count; layer++)
            {
                UpdateBiases(layer);
                UpdateWeights(layer - 1);
            }
        }

        private void UpdateWeights(int index)
        {
            int backLayer = index;
            int frontLayer = index + 1;
            double[,] weights = _weights[index];
            double[,] deltaWeights = _totalDeltaWeights[index];

            for (int backNeuron = 0; backNeuron < _neuronsPerLayer[backLayer]; backNeuron++)
            {
                for (int frontNeuron = 0; frontNeuron < _neuronsPerLayer[frontLayer]; frontNeuron++)
                {
                    weights[backNeuron, frontNeuron] +=
                        LearningRate * deltaWeights[backNeuron, frontNeuron] / MiniBatchSize;
                }
            }
        }

        private void UpdateBiases(int layer)
        {
            for (int neuron = 0; neuron < _neuronsPerLayer[layer]; neuron++)
            {
                _biases[layer][neuron] += LearningRate * _totalDeltaBiases[layer][neuron] / MiniBatchSize;
            }
        }

        private void UpdateDeltaParameters(double[] label)
        {
            UpdateOutputLayerParameters(label);
            UpdateHiddenLayerParameters();
        }

        private void UpdateHiddenLayerParameters()
        {
            int layer = _neuronsPerLayer.Count - 2;

            for (; layer > 0; layer--)
            {
                UpdateDeltaWeights(layer);
            }
        }

        private void UpdateDeltaWeights(int layer)
        {
            for (int frontNeuron = 0; frontNeuron < _neuronsPerLayer[layer]; frontNeuron++)
            {
                double deltaOutput = CalculateHiddenNeuronDeltaOutput(layer, frontNeuron);
                _currentDeltaNeuronOut[layer][frontNeuron] = deltaOutput;
                _totalDeltaBiases[layer][frontNeuron] += deltaOutput;
                for (int backNeuron = 0; backNeuron < _neuronsPerLayer[layer - 1]; backNeuron++)
                {
                    _totalDeltaWeights[layer - 1][backNeuron, frontNeuron] +=
                        _currentDeltaNeuronOut[layer][frontNeuron] * _outputs[layer - 1][backNeuron];
                }
            }
        }

        private double CalculateHiddenNeuronDeltaOutput(int layer, int neuron)
        {
            double[,] frontWeights = _weights[layer];
            int frontFrontNeuronCount = _neuronsPerLayer[layer + 1];

            double deltaNeuronOutput = 0;
            for (int frontFrontNeuron = 0; frontFrontNeuron < frontFrontNeuronCount; frontFrontNeuron++)
            {
                deltaNeuronOutput += _currentDeltaNeuronOut[layer + 1][frontFrontNeuron] *
                                     _weights[layer][neuron, frontFrontNeuron];
            }
            deltaNeuronOutput *= (1 - _outputs[layer][neuron]) * _outputs[layer][neuron];

            return deltaNeuronOutput;
        }

        private void UpdateOutputLayerParameters(double[] label)
        {
            int outputLayer = _neuronsPerLayer.Count - 1;
            int layerBefore = outputLayer - 1;

            for (int frontNeuron = 0; frontNeuron < _neuronsPerLayer[outputLayer]; frontNeuron++)
            {
                double deltaOut = (label[frontNeuron] - _outputs[outputLayer][frontNeuron]) * 
                                  (1 - _outputs[outputLayer][frontNeuron]) * _outputs[outputLayer][frontNeuron];
                _currentDeltaNeuronOut[outputLayer][frontNeuron] = deltaOut;
                _totalDeltaBiases[outputLayer][frontNeuron] += deltaOut;

                for (int backNeuron = 0; backNeuron < _neuronsPerLayer[layerBefore]; backNeuron++)
                {
                    _totalDeltaWeights[layerBefore][backNeuron, frontNeuron] +=
                        _currentDeltaNeuronOut[outputLayer][frontNeuron] * _outputs[layerBefore][backNeuron];
                }
            }
        }

        private void Shuffle(List<double[]> examples, List<double[]> labels)
        {
            int n = examples.Count;
            while (n > 1)
            {
                n--;
                int k = NeuralNetwork.Rand.Next(n + 1);
                var exampleValue = examples[k];
                var labelValue = labels[k];

                examples[k] = examples[n];
                labels[k] = labels[n];

                examples[n] = exampleValue;
                labels[n] = labelValue;
            }
        }

        public double[] Predict(double[] example)
        {
            _outputs[0] = example;
            for (int layer = 1; layer < _neuronsPerLayer.Count; layer++)
            {
                for (int neuron = 0; neuron < _neuronsPerLayer[layer]; neuron++)
                {
                    _outputs[layer][neuron] = CalculateNeuronOutput(layer, neuron);
                }
            }

            //last element of array
            return _outputs[^1];
        }

        private double CalculateNeuronOutput(int layer, int currentNeuron)
        {
            double net = 0;
            net += _biases[layer][currentNeuron];
            for (int previousNeuron = 0; previousNeuron < _outputs[layer - 1].Length; previousNeuron++)
            {
                net += _outputs[layer - 1][previousNeuron] * _weights[layer - 1][previousNeuron,currentNeuron];
            }
            return Sigmoid(net);
        }

        private double Sigmoid(double net)
        {
            return 1 / (1 + Math.Pow(Math.E, -net));
        }

        private void SetNeuronsPerLayer(string hiddenLayerConfig, int inputLayerNeurons, int outputLayerNeurons)
        {
            string[] _neuronsPerLayerString = hiddenLayerConfig.Split("x");
            _neuronsPerLayer = new List<int>();
            _neuronsPerLayer.Add(inputLayerNeurons);
            foreach (var numOfNeurons in _neuronsPerLayerString)
            {
                this._neuronsPerLayer.Add(int.Parse(numOfNeurons));
            }
            _neuronsPerLayer.Add(outputLayerNeurons);
        }

        private void InitializeData()
        {
            _biases = new List<double[]>(_neuronsPerLayer.Count);
            _weights = new List<double[,]>(_neuronsPerLayer.Count-1);
            _totalDeltaBiases = new List<double[]>(_neuronsPerLayer.Count);
            _totalDeltaWeights = new List<double[,]>(_neuronsPerLayer.Count-1);
            _currentDeltaNeuronOut = new List<double[]>(_neuronsPerLayer.Count);
            InitializeBiases();
            InitializeWeights();
            ResetOutputs();
        }

        private void ResetDeltaParameters()
        {
            ResetDeltaBiases();
            ResetDeltaWeights();
        }

        private void ResetDeltaWeights()
        {
            for (int index = 0; index < _neuronsPerLayer.Count - 1; index++)
            {
                int rows = _neuronsPerLayer[index];
                int columns = _neuronsPerLayer[index + 1];
                _totalDeltaWeights[index] = new double[rows, columns];
            }
        }

        private void ResetDeltaBiases()
        {
            for (int index = 0; index < _neuronsPerLayer.Count; index++)
            {
                _totalDeltaBiases[index] = new double[_neuronsPerLayer[index]];
            }
        }

        private void ResetOutputs()
        {
            _outputs = new List<double[]>();
            for (int index = 0; index < _neuronsPerLayer.Count; index++)
            {
                _outputs.Add(new double[_neuronsPerLayer[index]]);
                _currentDeltaNeuronOut.Add(new double[_neuronsPerLayer[index]]);
            }
        }

        private void InitializeWeights()
        {
            for (int index = 0; index < _neuronsPerLayer.Count - 1; index++)
            {
                int rows = _neuronsPerLayer[index];
                int columns = _neuronsPerLayer[index + 1];
                _weights.Add(CreateRandom2DArray(rows, columns));
                _totalDeltaWeights.Add(new double[rows, columns]);
            }
        }

        private void InitializeBiases()
        {
            for (int index = 0; index < _neuronsPerLayer.Count; index++)
            {
                _biases.Add(CreateRandomArray(_neuronsPerLayer[index]));
                _totalDeltaBiases.Add(new double[_neuronsPerLayer[index]]);
            }
        }

        private double[] CreateRandomArray(int arrayLength)
        {
            double[] newArray = new double[arrayLength];
            for (int index = 0; index < arrayLength; index++)
            {
                newArray[index] = RandomDouble(NeuralNetwork.RandomLowerbound, NeuralNetwork.RandomUpperbound);
            }

            return newArray;
        }

        private double[,] CreateRandom2DArray(int rows, int columns)
        {
            double[,] new2DArray = new double[rows, columns];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    new2DArray[row, column] =
                        RandomDouble(NeuralNetwork.RandomLowerbound, NeuralNetwork.RandomUpperbound);
                }
            }

            return new2DArray;
        }

        private double RandomDouble(double lowerBound, double upperBound)
        {
            return lowerBound + NeuralNetwork.Rand.NextDouble() * (upperBound - lowerBound);
        }

        private void InputDataValidation(List<double[]> examples, List<double[]> labels)
        {
            if (examples == null || labels == null || examples.Count == 0 || labels.Count == 0)
            {
                throw new ArgumentException("Arguments must not be empty or null!");
            }

            if (examples.Count != labels.Count)
            {
                throw new ArgumentException("Each example must have its own label!");
            }
        }
    }
}
