using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RecognizingUMLSketchDiagrams
{
    public partial class FormManager : Form
    {
        // Colector
        public static InkOverlay ink_overlay = new InkOverlay();

        public static Strokes strokes_text;
        public static Strokes strokes_shape;
        
        public static Recognizers recognizers = new Recognizers();

        public FormManager()
        {
            InitializeComponent();

            AutoScaleDimensions = new SizeF(AutoScaleDimensions.Width / (AutoScaleDimensions.Width / 4F),
            AutoScaleDimensions.Height / (AutoScaleDimensions.Height / 8F));

            // Se crea un objeto InkOverlay y se pone en el modo ink-and-gesture.
            // Se coge este modo y no gesture-only porque este ultimo necesita que
            // transcurra un tiempo para reconocer la gesture.
            ink_overlay = new InkOverlay(pnlInput.Handle);
            ink_overlay.CollectionMode = CollectionMode.InkAndGesture;

            // Se indica al InkOverlay las gestures que queremos reconocer
            ink_overlay.SetGestureStatus(ApplicationGesture.Scratchout, true);

            // Se conecta el manejador de eventos con las gesture y los stroke
            ink_overlay.Gesture += new InkCollectorGestureEventHandler(inkOverlay_Gesture);
            ink_overlay.Stroke += new InkCollectorStrokeEventHandler(inkOverlay_Stroke);

            // Se habilita el colector
            ink_overlay.Enabled = true;

            strokes_text = ink_overlay.Ink.CreateStrokes();
            strokes_shape = ink_overlay.Ink.CreateStrokes();
        }

        void inkOverlay_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            // 
            throw new NotImplementedException();
        }

        void inkOverlay_Gesture(object sender, InkCollectorGestureEventArgs e)
        {
            // Si se esta seguro del gesture a un nivel de confianza fuerte (Strong) o intermedio (intermediate)
            // borramos. En caso contrario, no se hace nada.
            if ((e.Gestures[0].Confidence == RecognitionConfidence.Strong) ||
                (e.Gestures[0].Confidence == RecognitionConfidence.Intermediate))
            {

                switch (e.Gestures[0].Id)
                {
                    case ApplicationGesture.Scratchout:
                        //Borramos

                        // Se obtiene el BoundingBox del Scratchout
                        Rectangle r = e.Strokes.GetBoundingBox();

                        // Se recorre la coleccion de strokes
                        foreach (Stroke s in ink_overlay.Ink.Strokes)
                        {
                            // Se mira si cada stroke esta incluido en el BoundingBox de Scratchou
                            // y si es asi se borra
                            DeleteInsideRectangle(s, r, e);
                        }

                        break;

                    default:
                        // No se hace nada
                        break;
                }
            }

            // Se asegura de que el stroke se ha borrado
            e.Cancel = false;
            pnlInput.Invalidate();

            throw new NotImplementedException();
        }

        private void DeleteInsideRectangle(Stroke s, Rectangle r, InkCollectorGestureEventArgs e)
        {

            // Se buscan los strokes que tengan al menos un 60% en el boundingbox 
            Strokes borrarStrokes =
                ink_overlay.Ink.HitTest(r, 60);

            // Si se encuentra algo, se borran los strokes afectados
            if (borrarStrokes.Count > 0)
            {
                // El stroke del scratchout no se maneja mas
                e.Cancel = true;

                // Se borran los strokes afectados
                ink_overlay.Ink.DeleteStrokes(borrarStrokes);

                // Se borra el stroke del scratchout
                ink_overlay.Ink.DeleteStrokes(e.Strokes);

                Refresh();
            }
        }

        private void exportarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetPath();
            
            if (Facade.path == null)
                MessageBox.Show("Debe introducir una ruta", "Atención",
         MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
            {
                double dist_between_points = 0;
                List<Point> resampled = new List<Point>();

                Renderer renderer = new Renderer();
                Graphics g = pnlInput.CreateGraphics();

                foreach (Stroke s in ink_overlay.Ink.Strokes)
                {
                    if (!s.Deleted)
                    {
                        // Separar el texto de las figuras
                        Divider(s);

                        for (int i = 0; i < strokes_shape.Count; i++)
                        {
                            // Buscar las esquinas en las figuras
                            dist_between_points = CornerFinder.DeterminingResampleSpacing(strokes_shape[i]);

                            resampled = (List<Point>)CornerFinder.ResamplePoints(strokes_shape[i], dist_between_points);

                            List<int> corners = (List<int>)CornerFinder.GetCorners(resampled);

                            // Dividir las polilíneas en líneas
                            SplitStrokes.SplitStroke(strokes_shape[i], resampled, corners, 0);
                        }
                    }
                }

                for (int i = 0; i < strokes_shape.Count; i++)
                    RecognizeShapes.Clasifier(strokes_shape[i]);
                // Buscar clases
                RecognizeShapes.SearchClasses();
                MessageBox.Show(Facade.classes.Count.ToString());

                // Determinar el nombre de las clases
                SetNameClasses();

                foreach (Class c in Facade.classes)
                    MessageBox.Show(c.GetName());

                // Determinar los atributos de las clases
                SetAttributesClasses();

                // Determinar los metodos de las clases
                SetMethodsClasses();

                // Actualizar la lista de strokes no procesados, es decir, todos los que no formen 
                // parte de las clases. Esto reduce el grupo de strokes a procesar a las relaciones
                // entre clases.
                ActualiceNonRecognizedStrokes();

                // Repetir buscar corner y dividir en líneas (con los strokes que no se han reconocido) 
                for (int i = 0; i < Facade.strokes_without_recognize.Count; i++)
                {
                    if (!Facade.strokes_without_recognize[i].Deleted)
                    {
                        // Buscar las esquinas en las figuras
                        dist_between_points = CornerFinder.DeterminingResampleSpacing(Facade.strokes_without_recognize[i]);

                        resampled = (List<Point>)CornerFinder.ResamplePoints(Facade.strokes_without_recognize[i], dist_between_points);

                        List<int> corners = (List<int>)CornerFinder.GetCorners(resampled);

                        // Dividir las polilíneas en líneas
                        SplitStrokes.SplitStroke(Facade.strokes_without_recognize[i], resampled, corners, 1);
                    }
                }

                if (Facade.strokes_without_recognize.Count > 0)
                    RecognizeShapes.SearchRelationships();


                CreateXML.CreateCHICOFile();
                MessageBox.Show("Exportado");
            }
        }

        public void Divider(Stroke s)
        {
            if (TextShapeDivider.Divide(s) == 1)
                strokes_text.Add(s);
            
            else
                strokes_shape.Add(s);
        }

        public void SetNameClasses()
        {
            foreach (Class c in Facade.classes)
            {
                String name = RecognizeShapes.FindTextClass(c, strokes_text, 0);
                c.SetName(name);
            }       
        }

        public void SetAttributesClasses()
        {
            foreach (Class c in Facade.classes)
            {
                String[] attributes_list = null;
                String attributes = RecognizeShapes.FindTextClass(c, strokes_text, 1);
                if (attributes != null)
                {
                    attributes_list = attributes.Split(new Char[] { ' ' });
                }
                c.SetAttributes(attributes_list);
            }
        }

        public void SetMethodsClasses()
        {
            foreach (Class c in Facade.classes)
            {
                String[] methods_list = null;
                String methods = RecognizeShapes.FindTextClass(c, strokes_text, 2);
                if (methods != null)
                {
                    methods_list = methods.Split(new Char[] { ' ' });
                }
                c.SetMethods(methods_list);
            }
        }

        public static bool CompareStrokes(Stroke s1, Stroke s2)
        {
            // Para ver si dos strokes son iguales se mira el bounding box la X e Y.
            // Si son iguales se devuelve true, si no false.
            int s1_X = s1.GetBoundingBox().X;
            int s1_Y = s1.GetBoundingBox().Y;

            int s2_X = s2.GetBoundingBox().X;
            int s2_Y = s2.GetBoundingBox().Y;

            if ((s1_X == s2_X) && (s1_Y == s2_Y))
                return true;
            else 
                return false;
        }

        public static void ActualiceNonRecognizedStrokes()
        {
            Strokes stks = ink_overlay.Ink.Strokes;

            foreach (Stroke stk in stks)
            {
                bool processed = false;

                foreach (Stroke s in Facade.strokes_recognized)
                {
                    if (CompareStrokes(stk, s)) processed = true;
                }

                if (!processed) Facade.strokes_without_recognize.Add(stk);
            }
        }

        private void GetPath()
        {
            String file_filter = "CHICO files (*.chico)|*.chico|" +
                "All files (*.*)|*.*";
            
            SaveFileDialog save_dialog = new SaveFileDialog();
            
            // Inicializa y muestra el cuadro de diálogo
            save_dialog.Filter = file_filter;

            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                Facade.path = save_dialog.FileName;  
            }
        }
    }
}
