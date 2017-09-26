using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace z.IO.Registry
{
    public class AppRegistry : IDisposable
    {
        public DataSet DataSource { get; set; }

        private string RegFile;
        private const string CnsGeneralData = "General";
        protected const int cnsKey = 47;

        public AppRegistry(string SettingFile)
        {
            this.DataSource = this.LoadSchema();
            this.RegFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), SettingFile).Replace("file:///", "");
        }

        public virtual DataSet LoadSchema()
        {
            DataSet ds = new DataSet("InSys");

            DataTable dtGen = new DataTable(CnsGeneralData);
            dtGen.Columns.Add("Key", typeof(string));
            dtGen.Columns.Add("Value", typeof(object));

            ds.Tables.Add(dtGen);

            return ds;
        }

        public void Load()
        {
            this.Load(this.RegFile);
        }

        public virtual void Load(string tmpPath)
        {
            if (System.IO.File.Exists(tmpPath))
            {

                string h = System.IO.File.ReadAllText(tmpPath);
                foreach (DataTable dt in z.Data.JsonClient.Common.ToDataSet(EncryptA(h, cnsKey)).Tables)
                {
                    if (this.DataSource.Tables.Contains(dt.TableName) && dt.Columns.Count > 0)
                    {
                        this.DataSource.Tables.Remove(dt.TableName);

                        if (dt.Columns.Contains("ID"))
                        {
                            dt.Columns["ID"].AutoIncrement = true;
                            dt.Columns["ID"].AutoIncrementSeed = 1;
                        }

                        this.DataSource.Tables.Add(dt.Copy());
                    }
                }

            }

            //load General Data
            //DataTable dt = this.DataSource.Tables[CnsGeneralData];
        }

        public object GetValue(string Key, object AlternativeValue = null)
        {
            var j = (from k in this.DataSource.Tables[CnsGeneralData].AsEnumerable()
                     where Convert.ToString(k["Key"]) == Key
                     select k).SingleOrDefault();

            if (j == null)
            {
                return AlternativeValue;
            }
            else
            {
                if (AlternativeValue != null && AlternativeValue.GetType().IsEnum)
                {
                    return Convert.ToInt32(j["Value"]);
                }
                else
                {
                    return j["Value"];
                }
            }
        }

        public void SetValue(string Key, object Value)
        {
            var j = (from k in this.DataSource.Tables[CnsGeneralData].AsEnumerable()
                     where Convert.ToString(k["Key"]) == Key
                     select k).SingleOrDefault();

            if (j == null)
            {
                this.DataSource.Tables[CnsGeneralData].Rows.Add(new object[] { Key, (Value.GetType().IsEnum) ? Convert.ToInt32(Value) : Value });
            }
            else
            {
                j["Value"] = (Value.GetType().IsEnum) ? Convert.ToInt32(Value) : Value;
            }
        }

        public virtual void Save()
        {
            this.Save(this.RegFile);
        }

        public virtual void Save(string tmpFileName)
        {
            var h = z.Data.JsonClient.Common.ToString(this.DataSource);
            if (!Directory.Exists(Path.GetDirectoryName(tmpFileName))) Directory.CreateDirectory(Path.GetDirectoryName(tmpFileName));
            System.IO.File.WriteAllText(tmpFileName, EncryptA(h, cnsKey));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.DataSource != null) this.DataSource.Dispose();
            }
            this.DataSource = null;
        }

        protected string EncryptA(string vData, int vKey)
        {
            char a = '\0';
            string s = "";
            int j = 0;
            foreach (char a_loopVariable in vData)
            {
                a = a_loopVariable;
                j = Convert.ToInt32(a);  //Strings.Asc(a);
                a = Convert.ToChar(j ^ vKey); //Strings.Chr((j ^ vKey));
                s += a;
            }
            return s;
        }

        ~AppRegistry()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
