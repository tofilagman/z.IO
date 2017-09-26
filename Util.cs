using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.IO
{
    public static class Util
    {

        public static string PathCombine(params string[] path)
        {
            return System.IO.Path.Combine(path);
        }

        public static string AppPath
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().Location;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int SendMessage(System.IntPtr hWnd, int wMsg, System.IntPtr wParam, System.IntPtr lParam);

        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        /// <summary>
        /// Scrolls the vertical scroll bar of a multi-line text box to the bottom.
        /// </summary>
        /// <param name="tb">The text box to scroll</param>
        public static void ScrollToBottom(System.Windows.Forms.Control tb)
        {
            if (System.Environment.OSVersion.Platform != System.PlatformID.Unix)
                SendMessage(tb.Handle, WM_VSCROLL, new System.IntPtr(SB_BOTTOM), System.IntPtr.Zero);
        }

        public static Bitmap ResizeImage(Image OriginalImage, int Width, int Height)
        {
            Size NewSize = new Size(Width, Height);
            PixelFormat Format = OriginalImage.PixelFormat;
            Format = PixelFormat.Format24bppRgb;
            Bitmap NewImage = new Bitmap(NewSize.Width, NewSize.Height); //, OriginalImage.PixelFormat
            Graphics Canvas = Graphics.FromImage(NewImage);
            Canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            Canvas.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            Canvas.DrawImage(OriginalImage, new Rectangle(new Point(0, 0), NewSize));
            return NewImage;

        }

        public static void ExecShell(string Command, bool CreateNoWindow = true)
        {
            try
            {
                using (var cmd = new Process())
                {
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = CreateNoWindow;
                    cmd.StartInfo.UseShellExecute = false;

                    cmd.Start();

                    cmd.StandardInput.WriteLine("@echo off");
                    cmd.StandardInput.WriteLine(Command);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ExecShell(Action<StreamWriter> Writer, bool CreateNoWindow = true)
        {
            try
            {
                using (var cmd = new Process())
                {
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = CreateNoWindow;
                    cmd.StartInfo.UseShellExecute = false;

                    cmd.Start();

                    Writer(cmd.StandardInput);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void RegisterDLL(string ApplicationPath, string DLLFileName, bool silent = true)
        {
            try
            {
                Process cmd = new Process();

                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;

                cmd.Start();

                cmd.StandardInput.WriteLine("@echo off");
                cmd.StandardInput.WriteLine(string.Format("cd {0}", ApplicationPath));
                cmd.StandardInput.WriteLine(string.Format("{0}:", ApplicationPath.Substring(0, 1)));
                cmd.StandardInput.WriteLine("regsvr32 {0} {1}", DLLFileName, (silent) ? "/s" : "");
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
