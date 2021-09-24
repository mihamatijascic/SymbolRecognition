using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SymbolGuessing.Model;

namespace SymbolGuessing.Interfaces
{
    public interface IGestureRepository
    {
        public delegate void RepositoryChanged();
        public event RepositoryChanged RepositoryChangeEvent;

        public void CreateGesture(string gestureName);
        public void RemoveGesture(string gestureName);
        public bool GestureExists(string gestureName);
        public int GesturePatternCount(string gestureName);
        public int GestureCount();
        public int AllPatternCount();
        public void AddPattern(string gestureName, List<Point> patternPoints);
        public void DeleteLastPattern(string gestureName);
        public List<Gesture> GetGestures();
        public void RemoveAllGestures();
        public ObservableCollection<Gesture> GetObservableGestures();
        public Dictionary<string, List<List<Point>>> GetAllGesturesPatterns();
    }
}
