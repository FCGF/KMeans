using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KMeans {
    public class Program {

        private static double DifferenceThreshold = 0.001;
        private static string TestbasePath = @"D:\Uni\5a fase\Inteligencia Artificial\BaseRGB";
        private static string PointsToClassifyPath = @"D:\Uni\5a fase\Inteligencia Artificial\BaseRGBPointsToClassify";

        public static void Main(string[] args) {
            //KMeans();
            KNN();
        }

        private static void KMeans() {
            IList<Point> points = MakePoints(TestbasePath, true);

            int kAmount = ValidateAndParseK(ReadK());

            string rawValue = ReadPointCoordinates();
            IList<Point> pointsToClassify;

            do {
                try {
                    double[] values = rawValue.Split(',').Select(v => double.Parse(v)).ToArray();
                    ValidateCoordinateLenght(points[0], values);
                    pointsToClassify = new List<Point>() { MakeUncategorizedPoint(values) };
                } catch {
                    pointsToClassify = MakePoints(rawValue.Trim());
                }

                IList<Point> ks = CreateInitialKs(kAmount, pointsToClassify);

                ks = RepositionKs(points, ks);

                CalculateDistanceToKs(pointsToClassify, ks);

                Console.ReadLine();
            } while (true);

        }

        private static void CalculateDistanceToKs(IList<Point> pointsToClassify, IList<Point> ks) {
            foreach (Point pointToClassify in pointsToClassify) {
                foreach (Point k in ks) {
                    k.CalculateEuclidianDistanceToPoint(pointToClassify);
                    pointToClassify.NearestPoints.Add(k);
                }
                Point chosenK = pointToClassify.NearestPoints.OrderBy(p => p.Distance).FirstOrDefault();
                Console.WriteLine("The point with coordinates [" + string.Join(", ", pointToClassify.Coordinates.Select(c => string.Format("{0:000.000}", c))) + "] is of Class " + chosenK.Class
                                + ", belonging to k at position  \t[" + string.Join(", ", chosenK.Coordinates.Select(c => string.Format("{0:000.000}", c))) + "] "
                                + " with distance " + chosenK.Distance);
            }
        }

        private static IList<Point> RepositionKs(IList<Point> points, IList<Point> ks) {
            IList<Point> previousPositions;

            do {
                previousPositions = new List<Point>(ks);

                foreach (Point p in points) {
                    Point nearestK = null;
                    foreach (Point k in ks) {
                        k.CalculateEuclidianDistanceToPoint(p);
                        if (nearestK == null || k.Distance < nearestK.Distance) {
                            nearestK = k;
                        }
                    }
                    nearestK.NearestPoints.Add(p);
                }

                IList<Point> newKs = new List<Point>();
                foreach (Point k in ks) {
                    IList<double> coordinates = new List<double>();

                    for (int i = 0; i < k.Coordinates.Count; i++) {
                        double average = 0;
                        if (k.NearestPoints != null && k.NearestPoints.Count > 0) {
                            foreach (Point point in k.NearestPoints) {
                                average += point.Coordinates[i];
                            }
                            average /= k.NearestPoints.Count;
                        } else {
                            average = k.Coordinates[i];
                        }
                        coordinates.Add(average);

                    }
                    Point newK = MakeUncategorizedPoint(coordinates.ToArray());
                    newK.Class = k.Class;
                    newKs.Add(newK);
                }

                ks = newKs;
            } while (AverageDifferences(ks, previousPositions) > DifferenceThreshold);
            return ks;
        }

        private static double AverageDifferences(IList<Point> currentPositions, IList<Point> previousPositions) {
            double Sum = 0;

            Point origin = MakeUncategorizedPoint(new double[previousPositions.Count]);

            for (int i = 0; i < currentPositions.Count; i++) {
                currentPositions[i].CalculateEuclidianDistanceToPoint(previousPositions[i]);
                Sum += (currentPositions[i].Distance);
            }

            return Sum / currentPositions.Count;
        }

        private static IList<Point> CreateInitialKs(int k, IList<Point> pointsToClassify) {
            //Criar Ks
            int size = pointsToClassify.First().Coordinates.Count;
            IList<Point> ks = new List<Point>();
            Random random = new Random();
            for (int i = 1; i <= k; i++) {
                double[] coords = new double[size];
                for (int j = 0; j < size; j++) {
                    coords[j] = (pointsToClassify.Max(p => p.Coordinates[j]) - pointsToClassify.Min(p => p.Coordinates[j])) * random.NextDouble();
                }
                AddPoint(ks, (PointClass)Enum.Parse(typeof(PointClass), i.ToString(), true), coords);
            }

            return ks;
        }

        private static void KNN() {
            IList<Point> points = MakePoints(TestbasePath, true);

            string rawValue = ReadPointCoordinates();
            IList<Point> pointsToClassify;

            try {
                double[] values = rawValue.Split(',').Select(v => double.Parse(v)).ToArray();
                ValidateCoordinateLenght(points[0], values);
                pointsToClassify = new List<Point>() { MakeUncategorizedPoint(values) };
            } catch {
                pointsToClassify = MakePoints(rawValue.Trim());
            }

            string readK = ReadK();
            int k = ValidateAndParseK(readK);

            foreach (Point pointToClassify in pointsToClassify) {
                foreach (Point point in points) {
                    point.CalculateEuclidianDistanceToPoint(pointToClassify);
                }
                Point[] nearestPoints = FindNearestPoints(points, k);
                IDictionary<PointClass, int> count = CountClasses(nearestPoints);

                var highest = count.Aggregate((l, r) => l.Value > r.Value ? l : r);

                Console.WriteLine("The point with coordinates [" + string.Join(", ", pointToClassify.Coordinates) + "] has a " + ((double)highest.Value) / k * 100 + "% probability to be of Class " + highest.Key);
            }

            Console.ReadLine();
        }

        private static IDictionary<PointClass, int> CountClasses(Point[] nearestPoints) {
            IDictionary<PointClass, int> count = new Dictionary<PointClass, int>();
            foreach (Point point in nearestPoints) {
                if (count.Keys.Contains(point.Class)) {
                    count[point.Class]++;
                } else {
                    count.Add(point.Class, 1);
                }
            }
            return count;
        }

        private static Point[] FindNearestPoints(IList<Point> points, int k) {
            Point[] nearestPoints = new Point[k];

            foreach (Point point in points) {
                if (HasEmpty(nearestPoints)) {
                    InsertOrReplacePoint(nearestPoints, point, null);
                } else {
                    Point farthest = nearestPoints.Aggregate((i1, i2) => i1.Distance > i2.Distance ? i1 : i2);
                    if (point.Distance < farthest.Distance) {
                        InsertOrReplacePoint(nearestPoints, point, farthest);
                    }
                }
            }

            return nearestPoints;
        }

        private static void InsertOrReplacePoint(Point[] nearestPoints, Point point, Point farthest) {
            for (int i = 0; i < nearestPoints.Length; i++) {
                if (nearestPoints[i] == farthest) {
                    nearestPoints[i] = point;
                    break;
                }
            }
        }

        private static bool HasEmpty(object[] objects) {
            return objects.Any(o => o == null);
        }

        private static void ValidateCoordinateLenght(Point point, double[] coordinates) {
            if (coordinates.Length != point.Coordinates.Count) {
                throw new ArgumentException("Incorrect parameter quantity!");
            }
        }

        private static int ValidateAndParseK(string readK) {
            int k;
            if (!int.TryParse(readK, out k)) {
                throw new ArgumentException("K was not a number!");
            }
            return k;
        }

        private static string ReadPointCoordinates() {
            Console.WriteLine("Insert the comma separated RGB value or file path:");
            return Console.ReadLine().Trim();
        }

        private static string ReadK() {
            Console.WriteLine("Insert value for K:");
            return Console.ReadLine().Trim();
        }

        private static IList<Point> MakePoints(string path = null, bool hasClassifier = false) {
            IList<Point> points = new List<Point>();

            if (string.IsNullOrWhiteSpace(path)) {
                //AddPoints(points);
                points = ReadFile(PointsToClassifyPath, hasClassifier);
            } else {
                points = ReadFile(path, hasClassifier);
            }

            return points;
        }

        private static IList<Point> ReadFile(string path, bool hasClassifier = false) {
            IList<Point> points = new List<Point>();

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines) {
                IList<double> values = line.Split(',').Select(v => double.Parse(v.Trim())).ToList();
                PointClass pClass = default(PointClass);
                if (hasClassifier) {
                    pClass = (PointClass)Enum.Parse(typeof(PointClass), values.Last().ToString());
                    values.RemoveAt(values.Count - 1);
                }
                points.Add(new Point() { Class = pClass, Coordinates = values });
            }

            return points;
        }

        private static void AddPoints(IList<Point> points) {
            AddPoint(points, PointClass.One, 1, 10, 200);
            AddPoint(points, PointClass.One, 2, 20, 230);
            AddPoint(points, PointClass.One, 6, 25, 150);
            AddPoint(points, PointClass.One, 7, 45, 100);
            AddPoint(points, PointClass.One, 10, 50, 125);
            AddPoint(points, PointClass.One, 3, 24, 111);
            AddPoint(points, PointClass.Two, 100, 4, 10);
            AddPoint(points, PointClass.Two, 250, 7, 50);
            AddPoint(points, PointClass.Two, 243, 5, 68);
            AddPoint(points, PointClass.Two, 210, 2, 90);
            AddPoint(points, PointClass.Two, 200, 1, 95);
            AddPoint(points, PointClass.Two, 215, 0, 68);
            AddPoint(points, PointClass.Three, 56, 200, 1);
            AddPoint(points, PointClass.Three, 79, 234, 3);
            AddPoint(points, PointClass.Three, 80, 210, 8);
            AddPoint(points, PointClass.Three, 95, 200, 10);
            AddPoint(points, PointClass.Three, 80, 210, 4);
            AddPoint(points, PointClass.Three, 49, 207, 1);
        }

        private static void AddPoint(IList<Point> points, PointClass pointClass, params double[] coordinates) {
            Point point = MakeUncategorizedPoint(coordinates);
            point.Class = pointClass;
            points.Add(point);
        }

        private static Point MakeUncategorizedPoint(params double[] coordinates) {
            Point point = new Point();
            foreach (double coordinate in coordinates) {
                point.Coordinates.Add(coordinate);
            }

            return point;
        }
    }

}
