using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;


namespace z.IO.Registry
{
    public class WinRegistry: IDisposable
    {
        RegistryKey rky;

        public WinRegistry(string Company, string AppName)
        {
            if (Microsoft.Win32.Registry.LocalMachine.GetSubKeyNames().Where(x => x == string.Format(@"Software\{0}\{1}", Company, AppName)).Any())
            {
                rky = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(string.Format(@"Software\{0}\{1}", Company, AppName), RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
            }
            else
            {
                rky = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(string.Format(@"Software\{0}\{1}", Company, AppName), RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
        }

        public void SetValue(string Key, object Value)
        {
            rky.SetValue(Key, Value);
        }

        public object GetValue(string key, object DefaultValue)
        {
            return rky.GetValue(key, DefaultValue);
        }

        public virtual void Save()
        {
            rky.Flush();
        }

        public RegistryKey MainKey
        {
            get { return rky; }
        }

        ~WinRegistry()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (rky != null)
            {
                rky.Close();
                rky.Dispose();
            }

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
