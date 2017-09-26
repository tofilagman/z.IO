using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace z.IO
{
    public static class Garbage
    {
        static bool m_bTipAction = true;

        public static void SetWorkingSet()
        {
            //Needs Admin rights on the machine

            try
            {
                System.Diagnostics.Process loProcess = System.Diagnostics.Process.GetCurrentProcess();
                if (m_bTipAction == true)
                {
                    loProcess.MaxWorkingSet = (IntPtr)((int)loProcess.MaxWorkingSet - 1);
                    loProcess.MinWorkingSet = (IntPtr)((int)loProcess.MinWorkingSet - 1);
                }
                else
                {
                    loProcess.MaxWorkingSet = (IntPtr)((int)loProcess.MaxWorkingSet + 1);
                    loProcess.MinWorkingSet = (IntPtr)((int)loProcess.MinWorkingSet + 1);
                }
                m_bTipAction = !m_bTipAction;
            }
            catch (System.Exception)
            {
            }
            finally
            {
                GC.Collect();
            }
        }

        public static void KillProc(string ProcessName)
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
                cmd.StandardInput.WriteLine("TASKKILL /im {0} /f", ProcessName);
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
