using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ink;

namespace RecognizingUMLSketchDiagrams
{
    public class Class
    {
        Stroke side1;
        Stroke side2;
        Stroke side3;
        Stroke side4;
        Stroke side5;
        Stroke side6;

        // Una clase queda definida por el siguiente orden de lados
        // 
        //     ________3_______
        //    |                |
        //    |________4_______|
        //  1 |                | 2
        //    |________5_______|
        //    |                |
        //    |________6_______|
        //
        //

        // Nombre de la clase
        String name;

        // Atributos de la clase
        String[] attributes;

        // Métodos de la clase
        String[] methods;


        public Class(Stroke s1, Stroke s2, Stroke s3, Stroke s4, Stroke s5, Stroke s6)
        {
            this.side1 = s1;
            this.side2 = s2;
            this.side3 = s3;
            this.side4 = s4;
            this.side5 = s5;
            this.side6 = s6;
        }


        public Stroke GetS1()
        {
            return side1;
        }

        public Stroke GetS2()
        {
            return side2;
        }

        public Stroke GetS3()
        {
            return side3;
        }

        public Stroke GetS4()
        {
            return side4;
        }

        public Stroke GetS5()
        {
            return side5;
        }

        public Stroke GetS6()
        {
            return side6;
        }
        public List<Stroke> GetAllSides()
        {
            List<Stroke> sides = new List<Stroke>();

            sides.Add(side1);
            sides.Add(side2);
            sides.Add(side3);
            sides.Add(side4);
            sides.Add(side5);
            sides.Add(side6);

            return sides;
        }
        public String GetName()
        {
            return name;
        }

        public void SetName(String n)
        {
            this.name = n;
        }

        public String[] GetAttributes()
        {
            return attributes;
        }

        public void SetAttributes(String[] a)
        {
            attributes = a;
        }

        public String[] GetMethods()
        {
            return methods;
        }

        public void SetMethods(String[] m)
        {
            methods = m;
        }

        
    }
}