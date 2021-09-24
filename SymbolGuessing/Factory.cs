using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymbolGuessing.Interfaces;
using SymbolGuessing.Model;
using SymbolGuessing.Services;

namespace SymbolGuessing
{
    public static class Factory
    {
        private static IGestureRepository _gestureFileRepository;

        public static IGestureRepository SingletonGestureFileRepository
        {
            get
            {
                return _gestureFileRepository ??= new GestureFileRepository();
            }
        }

        private static NeuralNetwork _neuralNetwork;

        public static NeuralNetwork SingletonNeuralNetwork
        {
            get
            {
                return _neuralNetwork ??= new NeuralNetwork();
            }
        }
    }
}
