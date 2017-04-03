using System;
using System.Collections.Generic;

namespace KMeans {
    public class Point {

        public Point() : base() {
            NearestPoints = new List<Point>();
        }

        private IList<int> coordinates;
        public IList<int> Coordinates {
            get {
                if (coordinates == null) {
                    coordinates = new List<int>();
                }
                return coordinates;
            }
            set {
                coordinates = value;
            }
        }

        public PointClass Class {
            get; set;
        }

        public double Distance {
            get; set;
        }

        public IList<Point> NearestPoints {
            get;
        }

        public void CalculateEuclidianDistanceToPoint(Point point) {
            if (point.Coordinates.Count != this.Coordinates.Count) {
                throw new ArgumentException("Points have different number of coordinates.");
            }

            double distance = 0;

            for (int i = 0; i < this.Coordinates.Count; i++) {
                distance += Math.Pow(this.Coordinates[i] - point.Coordinates[i], 2);
            }

            Distance = Math.Sqrt(distance);
        }

        public override bool Equals(object obj) {
            bool equals = false;
            if (obj != null && obj is Point) {
                Point point = obj as Point;
                equals = this.Coordinates == point.Coordinates
                    && this.Class == point.Class
                    && this.Distance == point.Distance;
            }
            return equals;
        }

    }
}
