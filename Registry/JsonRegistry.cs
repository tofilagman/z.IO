using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using z.Data;

namespace z.IO.Registry
{
    public class JsonRegistry
    {
        private Pair pair;

        private string RegFile;
        public JsonRegistry(string SettingFile)
        {
            pair = new Pair();
            this.RegFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), SettingFile).Replace("file:///", "");
        }

        public void Load()
        {
            this.Load(this.RegFile);
        }

        public virtual void Load(string tmpPath)
        {
            if (System.IO.File.Exists(tmpPath))
            {
                string h = System.IO.File.ReadAllText(tmpPath).CompressFromUTF16();
                pair = h.ToObject<Pair>();
            }
        }

        public virtual void Save()
        {
            this.Save(this.RegFile);
        }

        public virtual void Save(string tmpFileName)
        {
            var h = pair.ToJson().CompressToUTF16();
            if (!Directory.Exists(Path.GetDirectoryName(tmpFileName))) Directory.CreateDirectory(Path.GetDirectoryName(tmpFileName));
            System.IO.File.WriteAllText(tmpFileName, h);
        }

        public object GetValue(string Key, object AlternativeValue)
        {
            var j = pair.Where(x => x.Key == Key);

            if (!j.Any())
            {
                return AlternativeValue;
            }
            else
            {
                if (AlternativeValue.GetType().IsEnum)
                    return Convert.ToInt32(j.Single().Value);
                else
                    return j.Single().Value;
            }
        }

        public void SetValue(string Key, object Value)
        {
            var j = pair.Where(x => x.Key == Key);

            if (!j.Any())
            {
                pair.Add(Key, (Value.GetType().IsEnum) ? Convert.ToInt32(Value) : Value);
            }
            else
            {
                pair[Key] = (Value.GetType().IsEnum) ? Convert.ToInt32(Value) : Value;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                pair?.Dispose();
            this.pair = null;
        }

        ~JsonRegistry()
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
