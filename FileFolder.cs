using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace z.IO
{
    public static class FileFolder
    {

        public static void GenerateFolder(string pth)
        {
            if (!Directory.Exists(pth)) Directory.CreateDirectory(pth);
        }

        public static void CopyFiles(string source, string dest, string Folder)
        {
            string ppth = Path.Combine(dest, Folder);
            Directory.CreateDirectory(ppth);
            foreach (string fle in Directory.GetFiles(Path.Combine(source, Folder))) System.IO.File.Copy(fle, Path.Combine(ppth, Path.GetFileName(fle)));
        }

        public static void CopyAllFiles(string source, string dest, string rootdir = "")
        {
            if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            foreach (string str in Directory.GetFiles(source))
            {
                if (System.IO.File.Exists(Path.Combine(dest, Path.GetFileName(str)))) System.IO.File.Delete(Path.Combine(dest, Path.GetFileName(str)));
                System.IO.File.Copy(str, Path.Combine(dest, Path.GetFileName(str)));
            } 
            foreach (string dr in Directory.GetDirectories(source))
            {
                string mdst = dr.Replace(source, "").TrimStart('\\');
                mdst = Path.Combine(dest, mdst);
                CopyAllFiles(dr, mdst, source);
            }
        }

    }
}
