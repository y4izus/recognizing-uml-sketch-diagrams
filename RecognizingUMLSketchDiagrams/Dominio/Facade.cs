using System;
using System.Collections.Generic;
using Microsoft.Ink;

namespace RecognizingUMLSketchDiagrams
{
    public class Facade
    {
        //Para buscar las esquinas
        public const int W = 3;
        public const int INFINITE = 99999;
        //public const double THRESHOLD = 0.95; // Valor de shortstraw
        public const double THRESHOLD = 0.85;

        //Para el text-shape divider
        public const int SHAPE = 0;
        public const int TEXT = 1;

        public const double B_BOX_WIDTH = 1847.5;
        public const double TOTAL_ANGLE = 10.09914;
        public const double DIST_LAST = 2553.684;
        public const double DIST_NEXT = 1646.576;
        public const double TIME_NEXT = 660.8878;
        public const double SPEED_NEXT = 3.18416;
        public const double AMT_INK = 51.5;
        public const double PERIMETER = 25.31871;
     
        // Para separar lineas en horizontales, verticales y otras
        public const double SLOPE_MAX = 0.3;

        // El error para detectar rectas paralelas
        public const double ERROR = 20;

        //
        public const double LENGTH_PERCENTAGE = 0.5;


        public static List<Stroke> vertical_strokes = new List<Stroke>();
        public static List<Stroke> horizontal_strokes = new List<Stroke>();
        public static List<Stroke> other_strokes = new List<Stroke>();

        public static List<Stroke> line_strokes = new List<Stroke>();
        public static List<Stroke> triangle_strokes = new List<Stroke>();
        public static List<Stroke> diamond_strokes = new List<Stroke>();
        public static List<Stroke> arrow_strokes = new List<Stroke>();

        public static List<Stroke> strokes_recognized = new List<Stroke>();
        public static List<Stroke> strokes_without_recognize = new List<Stroke>();

        public static List<Class> classes = new List<Class>();
        public static List<Relation> relations = new List<Relation>();

        public static String path;

    }


}

