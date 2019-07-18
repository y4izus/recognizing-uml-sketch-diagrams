using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Microsoft.Ink;

namespace RecognizingUMLSketchDiagrams
{
    /// <summary>
    /// Holds data about the convex hull of a set of points.
    /// </summary>
    public class ConvexHulls
    {
        /// <summary>
        /// The amount that defines a different angle when calculating the smallest
        /// enclosing boundingBox.
        /// </summary>
        private const double RECT_ROTATE_ANGLE = Math.PI / 8;

        private Point[] convexPoints;
        private double convexPerimeter = 0;
        private double convexArea = 0;

        // Creates a convex hull from the specified points.
        public ConvexHulls(Point[] points)
        {
            convexPerimeter = 0;
            convexArea = 0;
            convexPoints = CalculateConvexHull(points);

        }

        /// <summary>
        /// Uses the Andrew variant of the Graham formula for calculating
        /// convex hulls.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public Point[] CalculateConvexHull(Point[] points)
        {
            if (points.Length < 3)
            {
                return points;
            }

            // TODO: Implement a quicksort algorithm, and find cut off point for switching algorithms.
            points = BubblesortPoints(points);

            List<Point> upper = new List<Point>();

            upper.Add(points[0]);
            upper.Add(points[1]);

            for (int i = 2; i < points.Length; i++)
            {
                upper.Add(points[i]);
                while (upper.Count > 2 && !ExtraMath.IsRightTurn(upper[upper.Count - 3], upper[upper.Count - 2], upper[upper.Count - 1]))
                {
                    upper.RemoveAt(upper.Count - 2);
                }
            }

            List<Point> lower = new List<Point>();
            lower.Add(points[points.Length - 1]);
            lower.Add(points[points.Length - 2]);

            for (int i = points.Length - 3; i >= 0; i--)
            {
                lower.Add(points[i]);
                while (lower.Count > 2 && !ExtraMath.IsRightTurn(lower[lower.Count - 3], lower[lower.Count - 2], lower[lower.Count - 1]))
                {
                    lower.RemoveAt(lower.Count - 2);
                }
            }
            Point[] retPoints = new Point[upper.Count + lower.Count];
            for (int i = 0; i < upper.Count; i++)
            {
                retPoints[i] = upper[i];
            }
            for (int i = 0; i < lower.Count; i++)
            {
                retPoints[i + upper.Count] = lower[i];
            }
            return retPoints;
        }


        protected Point[] BubblesortPoints(Point[] points)
        {
            points = (Point[])points.Clone();
            for (int i = 0; i < points.Length; i++)
            {
                Point p = points[i];
                int pointIndex = i;
                for (int q = i + 1; q < points.Length; q++)
                {
                    if (points[q].X < p.X || (points[q].X == p.X && points[q].Y < p.Y))
                    {
                        p = points[q];
                        pointIndex = q;
                    }
                }

                points[pointIndex] = points[i];
                points[i] = p;

            }
            return points;
        }

        /// <summary>
        /// Calculated the first time it is asked for and cached after that.
        /// </summary>
        /// <returns></returns>
        public double GetConvexPerimeter()
        {
            if (this.convexPerimeter == 0)
            {
                double perim = 0;
                for (int i = 0; i < convexPoints.Length - 1; i++)
                {
                    perim += ExtraMath.Distance(convexPoints[i], convexPoints[i + 1]);
                }
                convexPerimeter = perim;
            }
            return convexPerimeter;
        }

        /// <summary>
        /// Returns the area of the convex hull, calculated as the sum of the triangles.
        /// </summary>
        /// <returns></returns>
        public double GetConvexArea()
        {
            if (this.convexArea == 0)
            {
                double area = 0;
                Point point1 = convexPoints[0];
                for (int i = 1; i < convexPoints.Length - 1; i++)
                {
                    area += ExtraMath.CalculateTriangularArea(point1, convexPoints[i], convexPoints[i + 1]);
                }
                convexArea = area;
            }
            return convexArea;
        }
    }
}
