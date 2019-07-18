using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ink;

namespace RecognizingUMLSketchDiagrams
{
    public class Relation
    {
        // Tipo de la relación:
        // 0 => --------
        // 1 => ------->
        // 2 => ------|>
        // 3 => ------<>
        int type;

        // Clase donde se inicia la relación
        Class start_class;

        // Clase donde termina la relación
        Class end_class;

        // Stroke "largo" de la relación
        Stroke long_stroke;

        public Relation(int t, Class c1, Class c2, Stroke s)
        {
            this.type = t;
            this.start_class = c1;
            this.end_class = c2;
            this.long_stroke = s;
        }

        public int GetType()
        {
            return type;
        }

        public Class GetStartClass()
        {
            return start_class;
        }

        public Class GetEndClass()
        {
            return end_class;
        }

        public Stroke GetLongStroke()
        {
            return long_stroke;
        }
    }
}