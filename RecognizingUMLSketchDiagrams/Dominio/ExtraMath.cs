using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;


namespace RecognizingUMLSketchDiagrams
{
    /// <summary>
    /// Miscellaneous mathematical functions.
    /// </summary>
    public class ExtraMath
    {
        public ExtraMath()
        {

        }

        // Calcula la distancia Euclidea de la cuerda entre los puntos p1 y p2
        public static double Distance(Point p1, Point p2)
        {
            Double asqr = Math.Pow((p1.X - p2.X), 2.0);
            Double bsqr = Math.Pow((p1.Y - p2.Y), 2.0);

            return (double)Math.Sqrt(asqr + bsqr);
        }

        public static bool IsRightTurn(Point p1, Point p2, Point p3)
        {
            int a = p1.X - p3.X;
            int b = p1.Y - p3.Y;
            int c = p2.X - p3.X;
            int d = p2.Y - p3.Y;

            return ((a * d) < (b * c));
        }

        public static double CalculateTriangularArea(Point p1, Point p2, Point p3)
        {
            return Math.Abs(CalculateSignedTriangularArea(p1, p2, p3));
        }

        public static double CalculateSignedTriangularArea(Point p1, Point p2, Point p3)
        {
            int a = p1.X - p3.X;
            int b = p1.Y - p3.Y;
            int c = p2.X - p3.X;
            int d = p2.Y - p3.Y;

            return (0.5 * (double)((a * d) - (b * c)));
        }

        // Devuelve la pendiente de un stroke. Si option vale 0 devuelve el valor absoluto de
        // la pendiente. Si vale 1 sin valor absoluto
        public static double Slope(Stroke s, int option)
        {
            int x1, x2, y1, y2;
            double slope;


            // Cada stroke es una línea. De esas líneas se calcula su pendiente para
            // clasificarlas como rectas horizontales, verticales u otras.
            // La pendiente se calcula como (y2-y1)/(x2-x1) (en valor absoluto), 
            // siendo (x1,y1) el primer punto del stroke, y (x2,y2) el último.
            x1 = s.GetPoint(0).X;
            x2 = s.GetPoint(s.GetPoints().Length - 1).X;

            y1 = s.GetPoint(0).Y;
            y2 = s.GetPoint(s.GetPoints().Length - 1).Y;

            // Se clasifica en primer lugar las lineas totalmente verticales para 
            // evitar el error (1/0)
            if ((x2 - x1) == 0)
            {
                slope = Facade.INFINITE;
            }
            else
            {
                if(option == 0)
                    slope = Math.Abs((y2 - y1) / (x2 - x1));
                else
                    slope = (100*(y2 - y1) / (x2 - x1));
            }

            return slope;
        }

        // Devuelve true si los strokes introducidos son paralelos, false en caso contrario.
        // Dos rectas son paralelas si su pendiente es la misma. Se introduce un porcentaje
        // de error porque puede ocurrir que no sean exactamente las mismas pendientes
        public static bool AreParallels(Stroke s1, Stroke s2)
        {
            double slope1 = Slope(s1, 1);
            double slope2 = Slope(s2, 1);

            double slope_max;
            double slope_min;

            if (slope1 > slope2)
            {
                slope_max = slope1;
                slope_min = slope2;
            }
            else
            {
                slope_max = slope2;
                slope_min = slope1;
            }

            if ((slope_min + Facade.ERROR) > (slope_max - Facade.ERROR))
                return true;
            else
                return false;
        }
    }
}