using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace z.IO
{
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = true)]
    public class DeployFile : Attribute
    {
        public DeployFile(string file)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //if (File.Exists(Path.Combine(targetPath, file)))
            //{
            //    FileInfo fi = new FileInfo(Path.Combine(targetPath, file));
            //    FileInfo fo = new FileInfo(Path.Combine(path, file));

            //    if (fi.LastWriteTime < fo.LastWriteTime)
            //    {
            //        File.Delete(Path.Combine(targetPath, file));
            //        File.Copy(Path.Combine(path, file), Path.Combine(targetPath, file));
            //    }
            //}
            //else
            //{
            //    File.Copy(Path.Combine(path, file), Path.Combine(targetPath, file));
            //}

        }
    }
}
