using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace z.IO
{
    public static class Extensions
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        public static void InvokeOnUiThreadIfRequired(this System.Windows.Forms.Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        public static string BrowseFolder(this string selectedpath, bool ShowNewFolder = false)
        {
            using (FolderBrowserDialog fd = new FolderBrowserDialog())
            {
                fd.SelectedPath = selectedpath;
                fd.ShowNewFolderButton = ShowNewFolder;
                fd.RootFolder = Environment.SpecialFolder.MyComputer;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    return fd.SelectedPath;
                }
                else
                {
                    return selectedpath;
                }
            }
        }

        public static string BrowseFolder(this Control form, bool ShowNewFolder = false)
        {
            return "".BrowseFolder(ShowNewFolder);
        }

        public static string BrowseFile(this string mFile, string filter = "All Files|*.*")
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = mFile;
                ofd.Filter = filter;
                ofd.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
                else
                {
                    return mFile;
                }
            }
        }

        public static string BrowseFile(this Control form, string filter = "All Files|*.*")
        {
            return "".BrowseFile(filter);
        }

        public static Bitmap LoadBitmap(this string path)
        {
            //Open file in read only mode
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            //Get a binary reader for the file stream
            using (BinaryReader reader = new BinaryReader(stream))
            {
                //copy the content of the file into a memory stream
                var memoryStream = new MemoryStream(reader.ReadBytes((int)stream.Length));
                //make a new Bitmap object the owner of the MemoryStream
                return new Bitmap(memoryStream);
            }
        }

        /// <summary>
        ///  Get the Serial Com of the Device
        /// </summary>
        /// <param name="regkey">\Device\ProlificSerial0</param>
        /// <returns></returns>
        public static string FindSerial(this string regkey)
        {
            SerialFinder sf = new SerialFinder(regkey);
            string sCom = "";
            bool bSearch = sf.SearchforCom(ref sCom);
            if (bSearch == false) throw new Exception("Can not find the virtual serial port that can be used");
            return sCom;
        }

        public static byte[] ImageToByte(this string Image)
        {
            if (!System.IO.File.Exists(Image)) return new byte[0];
            System.Drawing.Imaging.ImageFormat ftr = System.Drawing.Imaging.ImageFormat.Png;

            switch (Path.GetExtension(Image).ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    ftr = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
            }
            return ImageToByte(System.Drawing.Image.FromFile(Image), ftr);
        }

        public static byte[] ImageToByte(this System.Drawing.Image imageIn, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static byte[] ImageToByte(this System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public static Image ByteToImage(this byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }

        public static int FromHex(this string value)
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
            }
            return Int32.Parse(value, NumberStyles.HexNumber);
        }

        public static string ToHex(this int value)
        {
            return String.Format("0x{0:X}", value);
        }

        public static FileContext ConvertToBase64(this string Filename)
        {
            var mc = new FileContext();
            mc.Filename = Filename;
            mc.ContentType = Filename.GetContentType();
            mc.DataString64 = string.Format("data:{0};base64,{1}", mc.ContentType, Convert.ToBase64String(GetFileBytes(Filename)));
            return mc;
        }

        /// <summary>
        /// Get the Base64 string array only
        /// </summary>
        /// <param name="imagebyte"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] imagebyte)
        {
            return Convert.ToBase64String(imagebyte);
        }


        public static byte[] FromBase64(this string base64string)
        {
            return Convert.FromBase64String(base64string.Split(new string[] { "base64," }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        public static string ToBase64Data(this byte[] imagebyte, string ContentType)
        {
            return string.Format("data:{0};base64,{1}", ContentType, Convert.ToBase64String(imagebyte));
        }

        public static byte[] GetFileBytes(this string Filename)
        {
            using (var filereader = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                using (var ms = new MemoryStream())
                {
                    filereader.CopyTo(ms);
                    ms.Position = 0;
                    byte[] buffer = ms.ToArray();
                    ms.Close();
                    return buffer;
                }
            }
        }

        public static void ToFile(this byte[] data, string Filename)
        {
            using (var filewriter = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                filewriter.Write(data, 0, data.Length);
            }
        }

        public static string GetFileName(this string FilePath, bool IncludeExtension = true)
        {
            return (IncludeExtension) ? Path.GetFileName(FilePath) : Path.GetFileNameWithoutExtension(FilePath);
        }

        public static void GetFileFromBytes(this byte[] buffer, string FileName)
        {
            using (FileStream fileStream = new FileStream(FileName, FileMode.Create))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    fileStream.WriteByte(buffer[i]);
                }
                fileStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < fileStream.Length; i++)
                {
                    if (buffer[i] != fileStream.ReadByte())
                    {
                        throw new Exception("Error Writing File");
                    }
                }
                //success
            }
        }

        public static string GetContentType(this string FileName, string AlternateExtension = "")
        {
            return new ContentType().GetContentType(Path.GetExtension(FileName), AlternateExtension);
        }

        public static string GetContentTypeInExt(this string Extension)
        {
            return new ContentType().GetContentType(Extension.ToLower());
        }

        [System.Diagnostics.DebuggerHidden]
        public static bool IsFileLocked(this string filePath)
        {
            try
            {
                using (System.IO.File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
                return errorCode == 32 || errorCode == 33;
            }
            return false;
        }
    }
}
