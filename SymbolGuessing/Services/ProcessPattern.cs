using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymbolGuessing.Services
{
    public static class ProcessPattern
    {
        /// <summary>
        /// Number of representative points that will represent single pattern as input in neural network
        /// </summary>
        public static int RepresentativePoints { get; set; } = 30;

        public static void ProcessPatterns(Dictionary<string, List<List<Point>>> gesturesPatterns,
            out List<double[]> examples, out List<double[]> labels, out List<string> gestureIndex)
        {
            gestureIndex = new List<string>();
            examples = new List<double[]>();
            labels = new List<double[]>();
            int index = 0;
            int labelLength = gesturesPatterns.Keys.Count;
            
            foreach (var gesture in gesturesPatterns.Keys)
            {
                gestureIndex.Add(gesture);
                double[] label = new double[labelLength];
                label[index] = 1;

                foreach (var pattern in gesturesPatterns[gesture])
                {
                    double[] example = ProcessOnePattern(pattern);
                    examples.Add(example);
                    labels.Add(label);
                }

                index++;
            }
        }

        public static double[] ProcessOnePattern(List<Point> pattern)
        {
            Point centroid = CalculateCentroid(pattern);
            List<Point> centralizedPoints = CentralizePoints(pattern, centroid);
            List<Point> scaledPoints = ScalePoints(centralizedPoints);
            List<Point> representativePoints = CreateRepresentativePoints(scaledPoints);
            double[] example = CreateExampleFromPoints(representativePoints);
            return example;
        }

        private static double[] CreateExampleFromPoints(List<Point> representativePoints)
        {
            double[] example = new double[representativePoints.Count * 2];
            for (int index = 0; index < representativePoints.Count; index++)
            {
                example[index * 2] = representativePoints[index].X;
                example[index * 2 + 1] = representativePoints[index].Y;
            }

            return example;
        }

        private static List<Point> CreateRepresentativePoints(List<Point> scaledPoints)
        {
            List<double> currentPointDistance;
            double distance = CalculateLengthOfPattern(scaledPoints, out currentPointDistance);
            double step = distance / ProcessPattern.RepresentativePoints;
            
            int pointIndex = 0;
            List<Point> representativePoints= new List<Point>();
            representativePoints.Add(scaledPoints[0]);

            for (int stepIndex = 1; stepIndex < RepresentativePoints; stepIndex++)
            {
                while (step * stepIndex > currentPointDistance[pointIndex])
                {
                    pointIndex++;
                }

                double percentage = (currentPointDistance[pointIndex] - step * stepIndex) / step;
                representativePoints.Add(InterpolatePoint(scaledPoints[pointIndex-1], scaledPoints[pointIndex], percentage));
            }

            return representativePoints;
        }

        private static Point InterpolatePoint(Point first, Point second, double percentage)
        {
            double x = first.X * percentage + second.X * (1- percentage);
            double y = first.Y * percentage + second.Y * (1 - percentage);
            return new Point(x, y);
        }

        private static double CalculateLengthOfPattern(List<Point> scaledPoints, out List<double> currentPointDistance)
        {
            double distance = 0;
            currentPointDistance = new List<double>();
            currentPointDistance.Add(0);
            for (int index = 0; index < scaledPoints.Count - 1; index++)
            {
                distance += CalculateDistance(scaledPoints[index], scaledPoints[index + 1]);
                currentPointDistance.Add(distance);
            }

            return distance;
        }

        private static double CalculateDistance(Point first, Point second)
        {
            double diffX = first.X - second.X;
            double diffY = first.Y - second.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        private static List<Point> ScalePoints(List<Point> centralizedPoints)
        {
            double max = double.MinValue;
            foreach (var point in centralizedPoints)
            {
                if (point.X > max) max = point.X;
                if (point.Y > max) max = point.Y;
            }

            return centralizedPoints.Select(s => new Point(s.X / max, s.Y / max)).ToList();
        }

        private static List<Point> CentralizePoints(List<Point> pattern, Point centroid)
        {
            List<Point> centralizedPoints = new List<Point>();

            foreach (var point in pattern)
            {
                Point newPoint = new Point();
                newPoint.X = point.X - centroid.X;
                newPoint.Y = point.Y - centroid.Y;
                centralizedPoints.Add(newPoint);
            }
            //return pattern.Select(p => new Point(p.X - centroid.X, p.Y - centroid.Y)).ToList();
            return centralizedPoints;
        }

        private static Point CalculateCentroid(List<Point> pattern)
        {
            Point centroid = new Point();
            foreach (var point in pattern)
            {
                centroid.X += point.X;
                centroid.Y += point.Y;
            }

            centroid.X /= pattern.Count;
            centroid.Y /= pattern.Count;
            return centroid;
        }
    }
}
