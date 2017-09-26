using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace z.IO.Registry
{
  public  class XmlRegistry : IDisposable
    {

      private Dictionary<string, object> dct = new Dictionary<string, object>();
      private string mPath;

      public XmlRegistry(string path)
      {
          this.mPath = path;
      }

      public void Load()
      {
          using (XmlTextReader rd = new XmlTextReader(mPath))
          {
              while (rd.Read())
              {
                  if (rd.Name == "add") if (HasAttribute(rd, "key")) dct.Add(rd.GetAttribute("key"), rd.GetAttribute("value"));
              }
          }
      }

      public void Save()
      {
          XmlDocument document = new XmlDocument();
          document.Load(mPath);

          foreach (KeyValuePair<string, object> hj in dct)
          {
              XmlNode j = GetNode(document, hj.Key);  //document.SelectNodes("//appSettings").Item(0).ChildNodes.Cast<XmlNode>().Where(x => x.Attributes["key"].Value == hj.Key);

              if (j != null) //j.Any()
              {
                  //j.SingleOrDefault().Attributes["value"].Value = hj.Value.ToString();
                  j.Attributes["value"].Value = hj.Value.ToString();
              }
              else
              {
                  XmlElement apt = document.CreateElement("add");
                  apt.SetAttribute("key", hj.Key);
                  apt.SetAttribute("value", hj.Value.ToString());
                  document.SelectNodes("//appSettings").Item(0).AppendChild(apt);
              }
          }

          document.Save(mPath);
      }

      public object GetValue(string Key, object defaultvalue)
      {
          if (dct.ContainsKey(Key))
          {
              return dct[Key];
          }
          else
          {
              return defaultvalue;
          }
      }

      public void SetValue(string Key, object Value)
      {
          if (dct.ContainsKey(Key))
          {
              dct[Key] = Value;
          }
          else
          {
              dct.Add(Key, Value);
          }
      }

      bool HasAttribute(XmlTextReader rd, string Name)
      {
          List<string> k = new List<string>();
          if (rd.HasAttributes)
          {
              while (rd.MoveToNextAttribute()) k.Add(rd.Name);
              rd.MoveToElement();
          }
          return k.Where(r => r == Name).Any();
      }

      XmlNode GetNode(XmlDocument document,string keyName)
      {
          foreach (XmlNode x in document.SelectNodes("//appSettings").Item(0).ChildNodes)
          {
              if (x.NodeType == XmlNodeType.Element && x.Attributes.Count > 0)
              {
                  if (x.Attributes["key"].Value == keyName) return x;
              }
          }
          return null;
      }

      ~XmlRegistry()
      {
          Dispose();
      }

      public void Dispose()
      {
          dct = null;
          GC.Collect();
          GC.SuppressFinalize(this);
      }
    }
}
