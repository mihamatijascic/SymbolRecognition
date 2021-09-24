using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SymbolGuessing.Interfaces;

namespace SymbolGuessing.Model
{
    public class GestureFileRepository : IGestureRepository
    {
        public string CurrentGesturesDirectory;

        public GestureFileRepository()
        {
            CurrentGesturesDirectory = $@"{Directory.GetCurrentDirectory()}\Gestures";
            Directory.CreateDirectory(CurrentGesturesDirectory);
        }

        public void CreateGesture(string gestureName)
        {
            string gesturePath = CreateGesturePath(gestureName);
            if (File.Exists(gesturePath)) return;
            else File.Create(gesturePath).Close();
            OnRepositoryChanged();
        }

        public void RemoveGesture(string gestureName)
        {
            if(!GestureExists(gestureName)) return;
            string filePath = $@"{CurrentGesturesDirectory}\{gestureName}.txt";
            try
            {
                File.Delete(filePath);
                OnRepositoryChanged();
            }
            catch (IOException e)
            {

            }
        }

        public bool GestureExists(string gestureName)
        {
            string gesturePath = CreateGesturePath(gestureName);
            if (File.Exists(gesturePath)) return true;
            return false;
        }

        public int GesturePatternCount(string gestureName)
        {
            string path = CreateGesturePath(gestureName);
            return File.ReadAllLines(path).Length;
        }

        public int GestureCount()
        {
            return Directory.GetFiles(CurrentGesturesDirectory).Length;
        }

        public int AllPatternCount()
        {
            var files = Directory.GetFiles(CurrentGesturesDirectory);
            int count = 0;
            foreach (var file in files)
            {
                count += File.ReadAllLines(file).Length;
            }
            return count;
        }

        public void AddPattern(string gestureName, List<Point> patternPoints)
        {
            StringBuilder patternBuilder = new StringBuilder();
            foreach (var patternPoint in patternPoints)
            {
                patternBuilder.Append(patternPoint.ToString()).Append("|");
            }

            patternBuilder.Remove(patternBuilder.Length - 1, 1);
            patternBuilder.Append(Environment.NewLine);

            try
            {
                File.AppendAllText(CreateGesturePath(gestureName), patternBuilder.ToString());
                OnRepositoryChanged();
            }
            catch (IOException e)
            {

            }
        }

        public void DeleteLastPattern(string gestureName)
        {
            List<string> lines = File.ReadAllLines(CreateGesturePath(gestureName)).ToList();
            if(lines.Count == 0) return;
            File.WriteAllLines(CreateGesturePath(gestureName), lines.GetRange(0, lines.Count-1).ToArray());
        }

        public List<Gesture> GetGestures()
        {
            var files = Directory.GetFiles(CurrentGesturesDirectory);
            List<Gesture> gestures = new List<Gesture>();
            foreach (var file in files)
            {
                Gesture gesture = new Gesture();
                gesture.Name = Path.GetFileNameWithoutExtension(file);
                try
                {
                    gesture.NumberOfPatterns = File.ReadAllLines(file).Length;
                }
                catch (IOException e)
                {
                    gesture.NumberOfPatterns = 0;
                }
                gestures.Add(gesture);
            }

            return gestures;
        }

        public void RemoveAllGestures()
        {
            var files = Directory.GetFiles(CurrentGesturesDirectory);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    OnRepositoryChanged();
                }
                catch (IOException e)
                {
                    
                }
            }
        }

        public ObservableCollection<Gesture> GetObservableGestures()
        {
            ObservableCollection<Gesture> observable = new ObservableCollection<Gesture>();
            GetGestures().ForEach(observable.Add);
            return observable;
        }

        public Dictionary<string, List<List<Point>>> GetAllGesturesPatterns()
        {
            Dictionary<string, List<List<Point>>> gesturesPatterns = new Dictionary<string, List<List<Point>>>();
            var files = Directory.GetFiles(CurrentGesturesDirectory);
            foreach (var file in files)
            {
                List<List<Point>> patterns = GetPatternsFromFile(file);
                gesturesPatterns.Add(Path.GetFileNameWithoutExtension(file), patterns);
            }
            return gesturesPatterns;
        }

        private List<List<Point>> GetPatternsFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            List<List<Point>> patterns = new List<List<Point>>();
            foreach (var line in lines)
            {
                List<Point> pattern = line.Split("|").ToList().Select(pointStr =>
                {
                    var pointParts = pointStr.Split(";");
                    return new Point(Double.Parse(pointParts[0]), Double.Parse(pointParts[1]));
                }).ToList();
                patterns.Add(pattern);
            }
            return patterns;
        }

        private string CreateGesturePath(string gestureName)
        {
            return $@"{CurrentGesturesDirectory}\{gestureName}.txt";
        }

        public event IGestureRepository.RepositoryChanged RepositoryChangeEvent;

        private void OnRepositoryChanged()
        {
            RepositoryChangeEvent?.Invoke();
        }

    }
}
