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
    public class RecognizeShapes
    {
        // Clasifica las líneas en horizontales, verticales y otras
        public static void Clasifier(Stroke s)
        {
            double slope = ExtraMath.Slope(s, 0);


            // Si pendiente = INFINITE => vertical_strokes
            // Si pendiente<SLOPE_MAX => horizontal_strokes
            // Si pendiente>1/SLOPE_MAX=> vertical_strokes
            // En otro caso => other_strokes

            if(slope == Facade.INFINITE)
            {
                Facade.vertical_strokes.Add(s);
            }
            else
            {
                if (slope < Facade.SLOPE_MAX)
                {
                    Facade.horizontal_strokes.Add(s);
                }
                else
                {
                    if (slope > (1 / Facade.SLOPE_MAX))
                    {
                        Facade.vertical_strokes.Add(s);
                    }
                    else
                    {
                        Facade.other_strokes.Add(s);
                    }
                }
            }
        }

        // Busca las clases que hay
        public static void SearchClasses()
        {
            // s1 es el stroke que esta más cerca el primer punto del stroke horizontal.
            // s2 es el stroke más cercano al último punto del stroke horizontal
            Stroke s1 = null;
            Stroke s2 = null;

            // Se crea una matriz donde se almacena en cada posicion, el stroke horizontal
            // con los dos strokes verticales más cercanos a él ( 3 strokes en total, el
            // horizontal y los dos verticales más cercanos).
            Stroke[,] nearby = new Stroke[3, Facade.horizontal_strokes.Count];

            for (int i = 0; i < Facade.horizontal_strokes.Count; i++)
            {
                Stroke h_s = Facade.horizontal_strokes[i];

                NearbyStrokes(h_s, ref s1, ref s2, Facade.vertical_strokes, 0);
                
                // Se almacena en el array de vecinos el stroke horizontal con los dos verticales
                // más cercanos
                nearby[0, i] = h_s;
                nearby[1, i] = s1;
                nearby[2, i] = s2;

            }

            // Una vez que se tienen los strokes horizontales con los verticales más cercanos
            // se ve cuales de los horizontales tienen los mismos verticales cercanos. Cuando
            // haya 4 horizontales con los mismos 2 verticales, se forma una clase con los 6.

            // Se crea una lista con los strokes horizontales con mismas verticales más cerca
            List<Stroke> same_vertical = new List<Stroke>();

            int index = 0;

            for (int i = 0; i < Facade.horizontal_strokes.Count - 1; i++)
            {

                // Se inicializan los strokes que van a crear la clase
                Stroke side1 = null;
                Stroke side2 = null;
                Stroke side3 = null;
                Stroke side4 = null;
                Stroke side5 = null;
                Stroke side6 = null;

                same_vertical.Add(nearby[0, i]);

                // El for no llega al último elemento, porque si ya no está en la lista, es
                // que no coincide con otro stroke, es decir, no pertenece a la clase.

                for (int j = i + 1; j < Facade.horizontal_strokes.Count; j++)
                {
                    // MIRAR COMO PARO CUANDO SAME_VERTICAL TENGA 4 STROKES. No creo que sea 
                    // necesario porque no va a haber más de cuatro líneas horizontales con
                    // las mismas verticales más cercanas.

                    if (!same_vertical.Contains(nearby[0, j]))
                    {
                        if ((nearby[1, i].Equals(nearby[1, j]) || nearby[1, i].Equals(nearby[2, j]))
                            && (nearby[2, i].Equals(nearby[1, j]) || nearby[2, i].Equals(nearby[2, j])))
                        {
                            same_vertical.Add(nearby[0, j]);
                        }
                    }
                }

                // Cuando se tengan cuatro horizontales en same_vertical, tenemos una clase. Para
                // saber qué stroke está en qué posición de la clase hacemos lo siguiente:
                // - si s1.X < s2.X => s1=side1 y s2=side2
                //   else => s1=side2 y s2= side1
                // - en same_vertical, se mira el que tenga mínima Y => side3, el de máxima => side6.
                //   De los dos restantes se mira el de mayor Y => side5, y el restante => side 4.

                if (same_vertical.Count == 4)
                {
                    if (nearby[1, i].GetPoint(0).X < nearby[2, i].GetPoint(0).X)
                    {
                        side1 = nearby[1, i];
                        side2 = nearby[2, i];
                    }
                    else
                    {
                        side1 = nearby[2, i];
                        side2 = nearby[1, i];
                    }

                    Stroke stroke_y_max = null;
                    Stroke stroke_y_min = null;

                    for (int j = 0; j < same_vertical.Count; j++)
                    {
                        for (int k = j + 1; k < same_vertical.Count-1; k++)
                        {
                            if (same_vertical[j].GetPoint(0).Y > same_vertical[k].GetPoint(0).Y)
                            {
                                stroke_y_max = same_vertical[j];
                            }
                        }
                    }

                    side6 = stroke_y_max;
                    same_vertical.Remove(stroke_y_max);

                    for (int j = 0; j < same_vertical.Count; j++)
                    {
                        for (int k = j + 1; k < same_vertical.Count-1; k++)
                        {
                            if (same_vertical[j].GetPoint(0).Y < same_vertical[k].GetPoint(0).Y)
                            {
                                stroke_y_min = same_vertical[j];
                            }
                        }
                    }

                    side3 = stroke_y_min;
                    same_vertical.Remove(stroke_y_min);

                    if (same_vertical[0].GetPoint(0).Y < same_vertical[1].GetPoint(0).Y)
                    {
                        side4 = same_vertical[0];
                        side5 = same_vertical[1];
                    }
                    else
                    {
                        side4 = same_vertical[1];
                        side5 = same_vertical[0];
                    }

                    if (side1 != null && side2 != null && side3 != null && side4 != null && side5 != null && side6 != null)
                    {
                        // Se añade la nueva clase con los strokes indicados a la lista de clases
                        //Class a = new Class(side1, side2, side3, side4, side5, side6);
                        Facade.classes.Insert(index, new Class(side1, side2, side3, side4, side5, side6));
                        index++;
                        Facade.strokes_recognized.Add(side1);
                        Facade.strokes_recognized.Add(side2);
                        Facade.strokes_recognized.Add(side3);
                        Facade.strokes_recognized.Add(side4);
                        Facade.strokes_recognized.Add(side5);
                        Facade.strokes_recognized.Add(side6);
                    }                
                }
                
                for (int j = same_vertical.Count - 1; j >= 0; j--)
                {
                    same_vertical.RemoveAt(j);
                }
            }
            
        }
     
        // Busca el texto de las clases. Si la opcion es:
        // - 0=>Nombre de la clase
        // - 1=>Atributos
        // - 2=>Metodos
        public static String FindTextClass(Class c, Strokes strokes_text, int option)
        {
            Strokes reco_name = FormManager.ink_overlay.Ink.CreateStrokes();
            String name = null;

            // Se obtienen los puntos necesarios de la clase
            int s1_X = c.GetS1().GetPoint(0).X;
            int s2_X = c.GetS2().GetPoint(0).X;
            int s3_Y = 0;
            int s4_Y = 0;

            switch (option)
            {
                case 0:
                    s3_Y = c.GetS3().GetPoint(0).Y;
                    s4_Y = c.GetS4().GetPoint(0).Y;
                    break;
                case 1:
                    s3_Y = c.GetS4().GetPoint(0).Y;
                    s4_Y = c.GetS5().GetPoint(0).Y;
                    break;
                case 2:
                    s3_Y = c.GetS5().GetPoint(0).Y;
                    s4_Y = c.GetS6().GetPoint(0).Y;
                    break;
                default:
                    break;

            }

            
            // Se recorren los strokes reconocidos como texto, y los que esten entre s3 y s4 de
            // la clase, forman el nombre, y es lo que se reconoce
            for (int i = 0; i < strokes_text.Count; i++)
            {
                Stroke s = strokes_text[i];

                // Se obtienen los puntos de la esquina superior izquierda del stroke   
                int s_X = s.GetBoundingBox().X;
                int s_Y = s.GetBoundingBox().Y;

                // Si el stroke esta entre los lados 1,2,3 y 4 de la clase, pertenece al
                // nombre de la clase
                if ((s1_X < s_X) && (s2_X > s_X) && (s3_Y < s_Y) && (s4_Y > s_Y))
                {
                    reco_name.Add(s);
                    Facade.strokes_recognized.Add(s);
                }
            }

            name = reco_name.ToString();

            return name;
        }

        // Determina la relacion en funcion de las puntas
        //    ----------     (Ningun stroke en la punta)
        //    --------->     (Se mira el stroke de la punta más cercano al resto de la flecha,
        //                    y se miran los strokes más cercanos a dicho stroke. Si uno es la
        //                    la línea larga de la flecha se trata de una asociación direccionada)
        //    --------<>     (Los strokes más cercanos al stroke más cercano a la línea larga,
        //                    son paralelos)
        //    --------|>     (Los strokes más cercanos al stroke más cercano a la línea larga,
        //                    no son paralelos)
        public static void SearchRelationships()
        {
            // Se ordenan los strokes de menor a mayor longitud
            Facade.strokes_without_recognize.Sort(CompareStrokesByLength);

            // Se le da la vuelta para tenerlos de mayor a menor longitud
            Facade.strokes_without_recognize.Reverse();

            int i = Facade.strokes_without_recognize.Count;
            do
            {
                // Se crean los strokes n1 y n2, correspondientes a los strokes más cercanos 
                // al stroke s (el primero de la lista de los strokes_without_recognize)
                Stroke n1 = null;
                Stroke n2 = null;

                // Será el stroke más cercano a "s"
                Stroke n = null;

                // Se crean los strokes nn1 y nn2, correspondientes a los strokes más cercanos 
                // al stroke n 
                Stroke nn1 = null;
                Stroke nn2 = null;

                // Se mira el primer stroke. Si es el único, se trata de una asociación (----)
                Stroke s = Facade.strokes_without_recognize[0];

                if (i == 1)
                {
                    MessageBox.Show("asociacion porque queda 1 stroke");
                    CreateAssociation(s);
                    i -= 1;
                }
                else
                {
                    // Se miran los strokes mas cercanos a los extremos del primer stroke 
                    // (n1 para el primer extremo y n2 para el segundo)
                    NearbyStrokes(s, ref n1, ref n2, Facade.strokes_without_recognize, 1);

                    // Se comprueba que estén a menos de un 10% de la longitud del stroke 
                    // inicial. Si es así, pertenecen a la relación. En caso contrario se
                    // trata de una asociación (----)( se crea la asociación y se elimina 
                    // el stroke de la lista de no reconocidos) 
                    bool belongs_n1 = BelongsToRelation(s, n1, 0);
                    bool belongs_n2 = BelongsToRelation(s, n2, s.GetPoints().Length - 1);

                    if (belongs_n1 == false && belongs_n2 == false)
                    {
                        MessageBox.Show("Asociacion por no belongs");
                        CreateAssociation(s);
                        i -= 1;
                    }
                    else
                    {
                        // Si no tenemos una asociación, se miran los strokes más cercanos 
                        // a los dos extremos del stroke que estaba más cerca del inicial.  
                        if (belongs_n1 == true)
                            n = n1;
                        if (belongs_n2 == true)
                            n = n2;

                        // Se miran los strokes mas cercanos a los extremos de "n" (nn1 para 
                        // el primer extremo y nn2 para el segundo)
                        NearbyStrokes(n, ref nn1, ref nn2, Facade.strokes_without_recognize, 0);

                        // Si esos dos strokes son paralelos: ----<> (Se crea relación 
                        // y se eliminan strokes de no reconocidos)
                        if (ExtraMath.AreParallels(nn1, nn2))
                        {
                            MessageBox.Show("agregacion");
                            CreateAgregation(s, n, nn1, nn2);
                            i -= 5;
                        }
                        else
                        {
                            // Si no son paralelos y si nn1 o nn2 coinciden con s: ---->
                            if ( FormManager.CompareStrokes(s, nn1) || FormManager.CompareStrokes(s, nn2))
                            {    
                                MessageBox.Show("asociacion direc");
                                CreateAssociationDirectional(s, n, nn1, nn2);
                                i -= 3;
                            }
                            else
                            {
                                MessageBox.Show("herencia");
                                // Si no son paralelos y ni nn1 ni nn2 coinciden con s: 
                                // ----|> (Crear relación y eliminar strokes)
                                CreateGeneralization(s, n, nn1, nn2);
                                i -= 4;
                            }
                        }
                    }
                }
            } while (i > 0) ;
        }

        // Compara los strokes por longitud
        private static int CompareStrokesByLength(Stroke s1, Stroke s2)
        {
            if (s1 == null)
            {
                if (s2 == null)
                {
                    // Si s1==s2==null => son iguales
                    return 0;
                }
                else
                {
                    // Si s1==null y s2!=null, s2 es mayor 
                    return -1;
                }
            }
            else
            {
                // Si s1!=null...
               
                if (s2 == null)
                // ...y s2==null, s1 es mayor.
                {
                    return 1;
                }
                else
                {
                    // ... y s2!=null, comparar la longitud de los 2 strokes
                    // ExtraMath.Distance(s.GetPoint(s.GetPoints().Length-1), s.GetPoint(0))
                    double s1_length = ExtraMath.Distance(s1.GetPoint(s1.GetPoints().Length - 1), s1.GetPoint(0));
                    double s2_length = ExtraMath.Distance(s2.GetPoint(s2.GetPoints().Length - 1), s2.GetPoint(0));

                    int retval = s1_length.CompareTo(s2_length);
                    
                    if (retval != 0)
                    {
                        // Si tienen distinta longitud, el más largo es mayor
                        
                        return retval;
                    }
                    else
                    {
                        // Si tienen el mismo tamaño se toma el primero como mayor
                        
                        return 1;
                    }
                }
            }
        }

        // Crea una relación de asociación y la inserta en la lista de relaciones
        public static void CreateAssociation(Stroke s)
        {
            Class c1 = null;
            Class c2 = null;

            SearchNearbyClasses(s, ref c1, ref c2);

            Relation r = new Relation(0, c1, c2, s);

            Facade.relations.Add(r);

            DeleteStrokeFromList(s, Facade.strokes_without_recognize);    
        }

        // Crea una relación de asociación direccional y la inserta en la lista de relaciones
        public static void CreateAssociationDirectional(Stroke s, Stroke n, Stroke nn1, Stroke nn2)
        {
            Class c1 = null;
            Class c2 = null;

            SearchNearbyClasses(s, ref c1, ref c2);

            Relation r = new Relation(1, c1, c2, s);

            Facade.relations.Add(r);

            DeleteStrokeFromList(s, Facade.strokes_without_recognize);
            DeleteStrokeFromList(n, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn1, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn2, Facade.strokes_without_recognize);
        }

        // Crea una relación de generalización y la inserta en la lista de relaciones
        public static void CreateGeneralization(Stroke s, Stroke n, Stroke nn1, Stroke nn2)
        {
            Class c1 = null;
            Class c2 = null;

            SearchNearbyClasses(s, ref c1, ref c2);

            Relation r = new Relation(2, c1, c2, s);

            Facade.relations.Add(r);

            DeleteStrokeFromList(s, Facade.strokes_without_recognize);
            DeleteStrokeFromList(n, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn1, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn2, Facade.strokes_without_recognize);           
        }

        // Crea una relación de agregación y la inserta en la lista de relaciones
        public static void CreateAgregation(Stroke s, Stroke n, Stroke nn1, Stroke nn2)
        {
            Class c1 = null;
            Class c2 = null;

            SearchNearbyClasses(s, ref c1, ref c2);

            Relation r = new Relation(3, c1, c2, s);

            Facade.relations.Add(r);

            // Se busca el stroke que falta para completar la relación (nn3). Para ello, 
            // se miran los strokes mas cercanos a los extremos de nn1 y nn2
            Stroke nn1a = null; 
            Stroke nn1b = null;
            Stroke nn2a = null;
            Stroke nn2b = null;

            NearbyStrokes(nn1, ref nn1a, ref nn1b, Facade.strokes_without_recognize, 0);
            NearbyStrokes(nn2, ref nn2a, ref nn2b, Facade.strokes_without_recognize, 0);

            Stroke nn3 = null;

            if (FormManager.CompareStrokes(nn1a, nn2a) && !FormManager.CompareStrokes(nn1a, n))
                nn3 = nn1a;
            if (FormManager.CompareStrokes(nn1a, nn2b) && !FormManager.CompareStrokes(nn1a, n))
                nn3 = nn1a;
            if (FormManager.CompareStrokes(nn1b, nn2a) && !FormManager.CompareStrokes(nn1b, n))
                nn3 = nn1b;
            if (FormManager.CompareStrokes(nn1b, nn2b) && !FormManager.CompareStrokes(nn1b, n))
                nn3 = nn1b;

            DeleteStrokeFromList(s, Facade.strokes_without_recognize);
            DeleteStrokeFromList(n, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn1, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn2, Facade.strokes_without_recognize);
            DeleteStrokeFromList(nn3, Facade.strokes_without_recognize);           
        }

        public static void SearchNearbyClasses(Stroke s, ref Class c1, ref Class c2)
        {
            // Para cada clase se crea una lista con sus lados y se ve cual de estos
            // está más cerca de los dos extremos del stroke s. De todos los strokes
            // obtenidos se saca una nueva lista y se ve cuál es el más cercano a s.
            
            float dist1 = 0;
            float dist2 = 0;
            float best_dist1 = Facade.INFINITE;
            float best_dist2 = Facade.INFINITE;

            for (int i = 0; i < Facade.classes.Count; i++)
            {
                Class c = Facade.classes[i];

                List<Stroke> sides = new List<Stroke>();
                sides = c.GetAllSides();

                Stroke s1 = null;
                Stroke s2 = null;

                NearbyStrokes(s, ref s1, ref s2, sides, 0);

                float f_index1 = s1.NearestPoint(s.GetPoint(0), out dist1);
                float f_index2 = s2.NearestPoint(s.GetPoint(s.GetPoints().Length-1), out dist2);

                if (dist1 < best_dist1)
                {
                    best_dist1 = dist1;
                    c1 = c;
                }

                if (dist2 < best_dist2)
                {
                    best_dist2 = dist2;
                    c2 = c;
                }
            }


        }

        // Determina si el stroke "n" pertenece a la relación del stroke "s"
        public static bool BelongsToRelation(Stroke s, Stroke n, int index)
        {
            float dist;

            float f_index = n.NearestPoint(s.GetPoint(index), out dist);

            double stroke_lenght = ExtraMath.Distance(s.GetPoint(s.GetPoints().Length - 1), s.GetPoint(0));

            if (dist < stroke_lenght * Facade.LENGTH_PERCENTAGE)
                return true;
            else
                return false;
        }

        // Determina los strokes más cercanos a los extremos de "s". s1 es el stroke más cercano
        // al extremo inicial y s2 al final. stks es la lista de strokes donde buscaremos esos 
        // strokes más cercanos. index indica la posición de la lista de strokes donde comenzar a
        // mirar.
        public static void NearbyStrokes(Stroke s, ref Stroke s1, ref Stroke s2, List<Stroke> stks, int index)
        {
            float dist1 = 0;
            float dist2 = 0;
            float best_dist1 = Facade.INFINITE;
            float best_dist2 = Facade.INFINITE;

            for (int j = index; j < stks.Count; j++)
            {
                Stroke stk = stks[j];

                if (!FormManager.CompareStrokes(s, stk)) 
                {
                    float f_index1 = stk.NearestPoint(s.GetPoint(0), out dist1);
                    float f_index2 = stk.NearestPoint(s.GetPoint(s.GetPoints().Length - 1), out dist2);

                    if (dist1 < best_dist1)
                    {
                        best_dist1 = dist1;
                        s1 = stk;
                    }

                    if (dist2 < best_dist2)
                    {
                        best_dist2 = dist2;
                        s2 = stk;
                    }
                }
            }
        }

        // Elimina el stroke "s" de la lista "stks"
        public static void DeleteStrokeFromList(Stroke s, List<Stroke> stks)
        {
            for (int i = 0; i < stks.Count; i++)
            {
                if (FormManager.CompareStrokes(s, stks[i]))
                {
                    stks.RemoveAt(i);                   
                }
            }
        }

        // Busca el índice en la lista de clases de la clase pasada por parámetros
        public static String SearchIndexClass(Class c1)
        {
            String index = "0";
            for (int i = 0; i < Facade.classes.Count; i++)
            {
                Class c2 = Facade.classes[i];
                if (FormManager.CompareStrokes(c1.GetS1(), c2.GetS1()))
                    index = i.ToString();
            }
            return index;
        }
    }
}

