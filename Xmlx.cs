using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace z.IO
{
    public class Xmlx : IDisposable
    {

        StringBuilder sb { get; set; }
        XmlWriterSettings ws { get; set; }
        DataSet ds { get; set; }

        string filename { get; set; }

        /// <summary>
        /// Writes Dataset to XML
        /// </summary>
        /// <param name="ds"></param>
        public Xmlx(DataSet ds)
        {
            sb = new StringBuilder();
            ws = new XmlWriterSettings();
            ws.Indent = true;
            this.ds = ds;
        }


        /// <summary>
        /// Writes DataTable to XML
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="DocName"></param>
        public Xmlx(DataTable dt, string DocName)
        {
            sb = new StringBuilder();
            ws = new XmlWriterSettings();
            ws.Indent = true;
            this.ds = new DataSet(DocName);
            this.ds.Tables.Add(dt.Copy());
            this.ds.Tables[0].TableName = (dt.TableName == "") ? "Table1" : dt.TableName;
        }

        /// <summary>
        /// Reads an Xml
        /// </summary>
        /// <param name="xmlfile"></param>
        public Xmlx(string xmlfile)
        {
            filename = xmlfile;
        }

        public Xmlx Write()
        {
            using (var wr = XmlWriter.Create(sb, ws))
            {
                wr.WriteStartDocument(true);
                wr.WriteStartElement(ds.DataSetName);

                foreach (DataTable dt in ds.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        wr.WriteStartElement(dt.TableName);
                        foreach (DataColumn dc in dt.Columns)
                        {
                            wr.WriteStartElement(dc.ColumnName);
                            wr.WriteString(dr[dc.ColumnName].ToString());
                            wr.WriteEndElement();
                        }
                        wr.WriteEndElement();
                    }
                }

                wr.WriteEndDocument();
            }

            return this;
        }

        public DataSet ToDataSet()
        {
            DataSet ds = null;
            using(XmlReader rd = XmlReader.Create(new StringReader(string.Join("",System.IO. File.ReadAllLines(this.filename))))){
               while(rd.Read()){
                   switch(rd.NodeType){
                       case XmlNodeType.Element:
                           if(ds == null) ds = new DataSet(rd.Name);
                           break;
                       case  XmlNodeType.Text:
                           break;
                       case XmlNodeType.XmlDeclaration:
                       case XmlNodeType.ProcessingInstruction:
                           break;
                       case XmlNodeType.Comment:
                           break;
                       case XmlNodeType.EndElement:
                           break;
                   }
               }
            }

            return ds;
        }

        public string ToXmlString()
        {
            return this.sb.ToString();
        }

        public void Dispose()
        {
            sb = null;
            ws = null;
        }
    }
}
