using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;


namespace RecognizingUMLSketchDiagrams
{
    public class CreateXML
    {
        public static void CreateCHICOFile()
        {
            // El archivo se va a guardar en la ruta indicada en el cuadro de diálogo de guardar
            using (StreamWriter stream_writer = new StreamWriter(Facade.path))
            {
                // Se escribe la cabecera
                stream_writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                
                stream_writer.Write("<com.chico.chico:Model xmi:version=\"2.0\" xmlns:xmi=\"http://www.omg.org/XMI\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:Domain_Package=\"Domain_Package\" xmlns:com.chico.chico=\"com.chico.chico\">\n");
                

                // Se recorre la lista de clases para ir escribiendo el contenido de cada una en el archivo
                for (int i = 0; i < Facade.classes.Count; i++)
                {
                    Class c = Facade.classes[i];
                    String name = c.GetName();
                    stream_writer.Write("\t<itsClasses name =" + "\"" + name + "\"" + ">\n");
                    

                    if (c.GetAttributes() != null)
                    {

                        for (int j = 0; j < c.GetAttributes().Length; j++)
                        {
                            String attribute = c.GetAttributes().GetValue(j).ToString();
                            stream_writer.Write("\t\t<itsAttributes name =" + "\"" + attribute + "\"" + "/>\n");       
                        }
                    }

                    if (c.GetMethods()!=null)
                    {
                        for (int j = 0; j < c.GetMethods().Length; j++)
                        {
                            String met = c.GetMethods().GetValue(j).ToString();
                            stream_writer.Write("\t\t<itsMethods name =" + "\"" + met + "\"" + "/>\n");
                        }
                    }
                    stream_writer.Write("\t</itsClasses>\n");
                }

                // Se recorre la lista de relaciones para ir escribiendo el contenido de cada 
                // una en el archivo
                for (int i = 0; i < Facade.relations.Count; i++)
                {
                    Relation r = Facade.relations[i];
                    String source = "//@itsClasses." + RecognizeShapes.SearchIndexClass(r.GetStartClass());
                    String target = "//@itsClasses." + RecognizeShapes.SearchIndexClass(r.GetEndClass());;
                    String relation_type = "";

                    switch (r.GetType())
                    {
                        case 0:
                            relation_type = "Domain_Package:Bidirectional_Association";
                            break;
                        case 1:
                            relation_type = "Domain_Package:Association";
                            break;
                        case 2:
                            relation_type = "Domain_Package:Generalization";
                            break;
                        case 3:
                            relation_type = "Domain_Package:Aggregation";
                            break;
                    }
                    stream_writer.Write("\t<itsRelationships xsi:type=" + "\"" + relation_type + "\"");
                    stream_writer.Write(" source=" + "\"" + source + "\"");
                    stream_writer.Write(" target=" + "\"" + target + "\""); 
                    stream_writer.Write("/>\n");
                }
                stream_writer.Write("</com.chico.chico:Model>");
            }
        }
    }
}
