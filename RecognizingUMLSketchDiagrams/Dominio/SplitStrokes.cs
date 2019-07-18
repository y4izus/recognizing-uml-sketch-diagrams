using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;

namespace RecognizingUMLSketchDiagrams
{
    public class SplitStrokes
    {
        // Para option = 0 => se está trabajando con los strokes_shape
        // Para option = 1 => con strokes_without_recognize
        public static void SplitStroke(Stroke s, List<Point> resampled, List<int> corners, int option)
        {
            // Se crea el findex que se va a emplear en el split
            float findex = 0;

            // Se comprueba la posicion en que el punto del stroke coincide con la segunda
            // esquina (sólo hace falta mirar la segunda esquina porque la primera es el 
            // primer punto del stroke, y a partir de la tercera pertenecerían al stroke
            // nuevo, el obtenido al romper el trazo). Si la segunda esquina es el último 
            // punto, ya tenemos la línea, pero si no lo es, tendremos que ver que 
            // posición ocupa en el stroke para poder romperlo por ahí. 

            // Punto de la esquina (en resampled)
            Point pt_corner = resampled[corners[1]];

            // Se comprueba que no sea el último punto
            if (!pt_corner.Equals(resampled[resampled.Count - 1]))
            {

                findex = SplitStrokes.GetFIndex(pt_corner, s);

                // El stroke se rompe por ese punto
                Stroke new_stroke = s.Split(findex);

                // El nuevo stroke se añade a la lista de strokes
                FormManager.ink_overlay.Ink.Strokes.Add(new_stroke);

                if (option == 0)
                    FormManager.strokes_shape.Add(new_stroke);
                else
                    Facade.strokes_without_recognize.Add(new_stroke);
            }
        }

        public static float GetFIndex(Point pt_corner, Stroke s)
        {
            double best_distance = Facade.INFINITE;
            double distance = Facade.INFINITE;
            float findex = 0;

            for (int i = 0; i < s.GetPoints().Length; i++)
            {
                distance = ExtraMath.Distance(pt_corner, s.GetPoint(i));

                if (distance < best_distance)
                {
                    best_distance = distance;
                    findex = (float)i;
                }
            }

            return findex;
        } 
    }
}