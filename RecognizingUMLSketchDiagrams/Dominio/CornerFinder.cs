using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;


namespace RecognizingUMLSketchDiagrams
{
    public class CornerFinder
    {
        public CornerFinder()
        {
        }

        // Determina la distancia que debe existir entre los puntos muestreados
        public static double DeterminingResampleSpacing(Stroke s)
        {
            double dist_between_points;
            Point top_left = new Point();
            Point bottom_right = new Point(); ;

            // Se obtiene el BoundingBox del Stroke
            Rectangle r = s.GetBoundingBox();

            // Se asignan a top_left la esquina superior izquierda del boundingbox,
            // y a bottom_right la esquina inferior derecha.
            top_left.X = r.Left;
            top_left.Y = r.Top;

            bottom_right.X = r.Right;
            bottom_right.Y = r.Bottom;

            // Se calcula la longitud de la diagonal formada por las dos esquinas 
            // calculadas
            double diagonal = ExtraMath.Distance(bottom_right, top_left);

            // Se calcula la distancia entre los puntos muestreados
            dist_between_points = diagonal / 40.0;

            return dist_between_points;
        }

        // Deja los puntos del stroke que esten espaciados en dist_between_points
        public static List<Point> ResamplePoints(Stroke s, double dist_between_points)
        {
            // Se crea una lista con los puntos del stroke
            List<Point> points = new List<Point>(s.GetPoints());

            // Se añade el primer punto del stroke a la coleccion de resampled
            List<Point> resampled = new List<Point>();
            resampled.Add(points[0]);

            // Se crea un nuevo punto que se utilizara mas adelante
            Point pt = new Point();

            // Se inicializa el marcador de distancia a 0
            double distance_holder = 0;

            // Se inicializa la distancia entre dos puntos
            double distance_points = 0;

            // Se recorren todos los puntos del stroke haciendo lo siguiente
            for (int i = 1; i < points.Count; i++)
            {
                // Se calcula la distancia Euclidea entre el punto actual y el anterior
                distance_points = ExtraMath.Distance(points[i - 1], points[i]);

                if ((distance_holder + distance_points) >= dist_between_points)
                {

                    // Se crea un punto nuevo (pt) localizado aproximadamente a una distancia
                    // dist_between_points del último punto muestreado
                    pt.X = (int)(points[i - 1].X +
                        (((dist_between_points - distance_holder) / distance_points) *
                        (points[i].X - points[i - 1].X)));
                    pt.Y = (int)(points[i - 1].Y +
                        (((dist_between_points - distance_holder) / distance_points) *
                        (points[i].Y - points[i - 1].Y)));

                    // Se añade el punto pt a la lista de "resampled"
                    resampled.Add(pt);

                    // Se añade el punto pt al stroke, en la posicion anterior al punto en el 
                    // que estamos
                    points.Insert(i, pt);

                    distance_holder = 0;
                }
                else
                    distance_holder += distance_points;
            }

            return resampled;
        }

        // Busca los puntos de resampled que se corresponden con esquinas
        public static List<int> GetCorners(List<Point> resampled)
        {
            // Se crea la lista donde iran las esquinas. Almacenara un conjunto de indices
            // que referencian puntos. Por ejemplo, corner(i)=j indica que el punto(j) es 
            // la i-esima esquina encontrada
            List<int> corners = new List<int>();

            corners.Add(0);

            // Se crea una lista con las distancias entre dos puntos separados W puntos del punto
            // actual
            List<double> straws = new List<double>();

            for (int i = 0; i < Facade.W; i++)
            {
                straws.Add(ExtraMath.Distance(resampled[i + Facade.W], resampled[i]));
            }

            for (int i = Facade.W; i < (resampled.Count - Facade.W); i++)
            {
                straws.Add(ExtraMath.Distance(resampled[i - Facade.W], resampled[i + Facade.W]));
            }

            for (int i = (resampled.Count - Facade.W); i < resampled.Count; i++)
            {
                straws.Add(ExtraMath.Distance(resampled[i - Facade.W], resampled[i]));
            }

            // Se calcula un umbral, threshold. Para ello, ordenamos la lista de straws y calculamos
            // su mediana. Para calcular la mediana se necesita ordenar la lista. Se trabajara con una
            // copia de la lista para no modificar la original
            List<double> copy_straws = new List<double>(straws);
            copy_straws.Sort();
            int middle = copy_straws.Count / 2;
            double median = (copy_straws.Count % 2 != 0) ?
                (double)copy_straws[middle] :
                ((double)copy_straws[middle] + (double)copy_straws[middle - 1]) / 2;
            double threshold = median * 0.95;

            // Ahora se recorre la lista de straws. Si la distancia es menor que el umbral, se considera
            // esquina
            double local_min;
            int local_min_index;

            for (int i = Facade.W; i < (resampled.Count - Facade.W); i++)
            {
                if (straws[i] < threshold)
                {
                    local_min = Facade.INFINITE;
                    local_min_index = i;

                    while (i < straws.Count && straws[i] < threshold)
                    {
                        if (straws[i] < local_min)
                        {
                            local_min = straws[i];
                            local_min_index = i;
                        }
                        i++;
                    }
                    corners.Add(local_min_index);
                }
            }
            // Se añade el ultimo indice a corners
            corners.Add(resampled.Count - 1);
 
            corners = PostProcessCorners(resampled, corners, straws);

            return corners;

        }

        // Se vuelven a procesar las esquinas encontradas para eliminar falsos positivos
        public static List<int> PostProcessCorners(List<Point> resampled, List<int> corners, List<double> straws)
        {
            int c1, c2 = 0;
            
            for (int i = 1; i < (corners.Count - 1); i++)
            {
                c1 = corners[i - 1];
                c2 = corners[i + 1];

                if (IsLine(resampled, c1, c2))
                {
                    corners.RemoveAt(i);
                    i -= 1;
                }
            }

            return corners;
        }

        // Determina si la parte del stroke indicada es una línea o no
        public static bool IsLine(List<Point> resampled, int a, int b)
        {
            double distance = (double)ExtraMath.Distance(resampled[a], resampled[b]);
            double path_distance = (double)PathDistance(resampled, a, b);

            if ((distance / path_distance) > Facade.THRESHOLD)
                return true;
            else
                return false;
        }

        // Determina la distancia del tramo (no la distancia más corta)
        public static double PathDistance(List<Point> resampled, int a, int b)
        {
            double distance = 0;

            for (int i = a; i < b; i++)
            {
                distance += (double)ExtraMath.Distance(resampled[i], resampled[i + 1]);
            }
            return distance;
        }

    }
}
