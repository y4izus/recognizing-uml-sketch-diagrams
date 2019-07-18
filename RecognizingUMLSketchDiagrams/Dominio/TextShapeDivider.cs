using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Ink;

namespace RecognizingUMLSketchDiagrams
{
    public class TextShapeDivider
    {
        public static Guid the_time_guid = Guid.NewGuid();

        // Dependiendo de la posición del stroke se ejecuta un código u otro
        public static int Divide(Stroke s)
        {
            // Un único stroke
            if (OnlyOne(s))
                return DivideOnlyOne(s);
            else
            {
                // Primer stroke
                if (!ExistsPrev(s))
                    return DivideFirst(s);
                else
                {
                    // Último stroke
                    if (!ExistsNext(s))
                        return DivideLast(s);
                    else
                        // Strokes intermedios
                        return DivideMedium(s);
                }
            }
        }

        // Divider cuando se tiene un único stroke
        public static int DivideOnlyOne(Stroke s)
        {
            if (GetWidth(s) >= Facade.B_BOX_WIDTH)
                if (GetTotalAngle(s) < Facade.TOTAL_ANGLE)
                    return Facade.SHAPE;
                else
                    return Facade.TEXT;
            
            else
                if (GetInkInside(s) >= Facade.AMT_INK)
                    return Facade.SHAPE;
                else
                    if (GetPerimeterToArea(s) < Facade.PERIMETER)
                        return Facade.SHAPE;
                    else
                        return Facade.TEXT;
        }

        // Divider cuando se tiene el primer stroke
        public static int DivideFirst(Stroke s)
        {
            if (GetWidth(s) >= Facade.B_BOX_WIDTH)
                if (GetTotalAngle(s) < Facade.TOTAL_ANGLE)
                    return Facade.SHAPE;
                else
                    return Facade.TEXT;
            else 
            {
                double timeToNext = GetTimeToNext(s, GetNextStroke(s));
                double distanceToNext = GetDistanceBetweenStrokes(GetNextStroke(s), s);
                double speedToNext = distanceToNext / timeToNext;

                if (timeToNext > Facade.TIME_NEXT)
                    return Facade.SHAPE;

                if (distanceToNext >= Facade.DIST_NEXT)
                    if (speedToNext < Facade.SPEED_NEXT)
                        if (GetInkInside(s) >= Facade.AMT_INK)
                            return Facade.SHAPE;
                        else
                            if (GetPerimeterToArea(s) < Facade.PERIMETER)
                                return Facade.SHAPE;
                            else
                                return Facade.TEXT;
                    else
                        return Facade.TEXT;
                else
                    return Facade.TEXT;
            }
        }

        // Divider cuando se tiene un último stroke
        public static int DivideLast(Stroke s)
        {
            if (GetWidth(s) >= Facade.B_BOX_WIDTH)
                if (GetTotalAngle(s) < Facade.TOTAL_ANGLE)
                    return Facade.SHAPE;
                else
                    return Facade.TEXT;
            else
                
                if (GetDistanceBetweenStrokes(s, GetPrevStroke(s)) >= Facade.DIST_LAST) 
                    return Facade.SHAPE;
                else
                    //return TEXT;
                    if (GetInkInside(s) >= Facade.AMT_INK)
                        return Facade.SHAPE;
                    else
                        if (GetPerimeterToArea(s) < Facade.PERIMETER)
                            return Facade.SHAPE;
                        else
                            return Facade.TEXT;
        }

        // Divider cuando se tienen strokes intermedios
        public static int DivideMedium(Stroke s)
        {
            if (GetWidth(s) >= Facade.B_BOX_WIDTH)
                if (GetTotalAngle(s) < Facade.TOTAL_ANGLE)
                    return Facade.SHAPE;
                else
                    return Facade.TEXT;
            else
                
                if (GetDistanceBetweenStrokes(s, GetPrevStroke(s)) >= Facade.DIST_LAST)
                    if (GetTimeToNext(s, GetNextStroke(s)) >= Facade.TIME_NEXT)
                        return Facade.SHAPE;
                    else
                        return Facade.TEXT;
                else
                {
                    double distanceToNext = GetDistanceBetweenStrokes(GetNextStroke(s), s);
                    double timeToNext = GetTimeToNext(s, GetNextStroke(s));
                    double speedToNext = distanceToNext / timeToNext;

                    if (distanceToNext >= Facade.DIST_NEXT)
                        if (speedToNext < Facade.SPEED_NEXT)
                            if (GetInkInside(s) >= Facade.AMT_INK)
                                return Facade.SHAPE;
                            else
                                if (GetPerimeterToArea(s) < Facade.PERIMETER)
                                    return Facade.SHAPE;
                                else
                                    return Facade.TEXT;
                        else
                            return Facade.TEXT;
                    else
                        return Facade.TEXT;
                }
        }

        // Comprueba si el stroke "s" es el único de la lista
        public static bool OnlyOne(Stroke s)
        {
            if (!ExistsNext(s) && !ExistsPrev(s))
                return true;
            else
                return false;
        }

        // Comprueba si existe stroke anterior al actual
        public static bool ExistsPrev(Stroke s)
        {
            int index = FormManager.ink_overlay.Ink.Strokes.IndexOf(s);

            if (index == 0)
                return false;
            else
                return true;


        }

        // Comprueba si existe stroke posterior al actual
        public static bool ExistsNext(Stroke s)
        {
            int num_strokes = FormManager.ink_overlay.Ink.Strokes.Count;
            int index = FormManager.ink_overlay.Ink.Strokes.IndexOf(s);

            if ((index + 1) == num_strokes)
                return false;
            else
                return true;
        }

        // Obtiene el stroke siguiente al actual
        public static Stroke GetNextStroke(Stroke s)
        {
            int index_next = FormManager.ink_overlay.Ink.Strokes.IndexOf(s) + 1;

            Stroke next_stroke = FormManager.ink_overlay.Ink.Strokes[index_next];

            if (!next_stroke.Deleted)
                return next_stroke;
            else
                return GetNextStroke(next_stroke);
        }

        // Obtiene el stroke anterior al actual
        public static Stroke GetPrevStroke(Stroke s)
        {
            int index_prev = FormManager.ink_overlay.Ink.Strokes.IndexOf(s) - 1;

            Stroke prev_stroke = FormManager.ink_overlay.Ink.Strokes[index_prev];

            if (!prev_stroke.Deleted)
                return prev_stroke;
            else
                return GetPrevStroke(prev_stroke);
        }

        // Se mira el alto y ancho del stroke y se devuelve el mayor
        public static int GetWidth(Stroke s)
        {
            int width;
            int height;

            width = s.GetBoundingBox().Width;
            height = s.GetBoundingBox().Height;

            if (width > height)
                return width;
            else
                return height;
        }

        public static double GetTotalAngle(Stroke s)
        {
            Point p1, p2, p3;
            p1 = p2 = p3 = new Point(0, 0);
            double temp;

            double angle_sum = 0;
            p1 = s.GetPoint(0);
            if (s.GetPoints().Length > 1)
                p2 = s.GetPoint(1);

            for (int i = 1; i < s.GetPoints().Length - 1; i++)
            {
                p3 = s.GetPoint(i + 1);

                temp = (
                    (
                        (
                        (p3.X - p2.X) * (p2.Y - p1.Y)
                        )
                        - (
                        (p2.X - p1.X) * (p3.Y - p2.Y)
                        ))
                        /
                    (double)(
                    (
                    (p3.X - p2.X) * (p2.X - p1.X)
                    )
                        + (
                        (p3.Y - p2.Y) * (p2.Y - p1.Y)
                        ))
                    );
                temp = Math.Atan(temp);
                if (Double.IsNaN(temp))
                {
                    temp = 0;
                }

                angle_sum += temp;
                p1 = p2;
                p2 = p3;
            }

            return angle_sum;
        }

        public static double GetInkInside(Stroke s)
        {
            double numIntersections = 0;
            int numPoints = s.GetPoints().Length;
            System.Drawing.Rectangle rect1 = s.GetBoundingBox();
            rect1.Inflate(rect1.Width / 5, rect1.Height / 5);

            StrokeIntersection[] intersections1 = s.GetRectangleIntersections(rect1);
            foreach (StrokeIntersection intersection in intersections1)
            {
                int beginIndex = (intersection.BeginIndex != -1 ? (int)intersection.BeginIndex : 0);
                int endIndex = (intersection.EndIndex != -1 ? (int)intersection.EndIndex : numPoints - 1);

                numIntersections += (endIndex - beginIndex + 1);
            }
            return numIntersections;
        }

        public static double GetDistanceBetweenStrokes(Stroke currentS, Stroke prevS)
        {
            Point p1 = currentS.GetPoint(0);
            Point p2 = prevS.GetPoint(prevS.GetPoints().Length - 1);
            return (Distance(p1, p2));
        }

        public static double Distance(Point p1, Point p2)
        {
            Double asqr = Math.Pow((p1.X - p2.X), 2.0);
            Double bsqr = Math.Pow((p1.Y - p2.Y), 2.0);

            return Math.Sqrt(asqr + bsqr);
        }

        public static double GetTimeToNext(Stroke curr, Stroke next)
        {
            double startTime = 0;
            double endTime = 0;

            #region first get the start time of the next stroke
            if (next.ExtendedProperties.DoesPropertyExist(the_time_guid))
            {
                // Get the raw data out of this stroke's extended
                // properties list, using the previously defined
                // Guid as a key to the required extended property.
                long theLong = (long)next.ExtendedProperties[the_time_guid].Data;
                // Then turn it (as a FileTime) into a time string.
                startTime = DateTime.FromFileTime(theLong).TimeOfDay.TotalMilliseconds;
            }
            #endregion

            #region now get the end time of this stroke
            if (curr.ExtendedProperties.DoesPropertyExist(the_time_guid))
            {
                long theLong = (long)curr.ExtendedProperties[the_time_guid].Data;
                endTime = DateTime.FromFileTime(theLong).TimeOfDay.TotalMilliseconds;
            }

            bool fFound = false;
            //from Tablet PC Platform SDK: Ink Data Management, Part II ch 6 code
            for (int j = 0; j < curr.PacketDescription.Length; j++)
            {
                if (curr.PacketDescription[j] == PacketProperty.TimerTick)
                {
                    fFound = true;
                    break;
                }
            }
            List<int> tempTicks = new List<int>();
            if (fFound)
                tempTicks.AddRange(curr.GetPacketValuesByProperty(PacketProperty.TimerTick));

            if (tempTicks.Count > 0)
            {
                endTime = endTime + tempTicks[tempTicks.Count - 1] * 0.0001;
            }
            #endregion

            return (startTime - endTime);
        }

        public static double GetPerimeterToArea(Stroke s)
        {
            ConvexHulls convexHull = new ConvexHulls(s.GetPoints());
            double v = convexHull.GetConvexArea() / convexHull.GetConvexPerimeter();
            return v;
        }

    }
}
