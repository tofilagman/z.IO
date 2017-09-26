using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.IO
{
    /// <summary>
    /// LJ 20150727
    /// Log Watcher
    /// </summary>
    public class  LogWatcher 
    {
        public delegate void FileChangeHandler(string Text);
        public event FileChangeHandler OnFileChange;

        public LogWatcher(string FileName)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(FileName);

            watcher.NotifyFilter =  NotifyFilters.LastWrite | NotifyFilters.FileName;

            watcher.Filter = Path.GetFileName(FileName);  // "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        [System.Diagnostics.DebuggerHidden]
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (OnFileChange != null) Task.Factory.StartNew(() => {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    unsafe
                    {
                        bool tr = true;
                        while (tr)
                        {
                            try
                            {
                                string file = System.IO. File.ReadLines(e.FullPath).Last();
                                tr = false;
                                OnFileChange(file);
                            }
                            catch (Exception)
                            { }
                        }   
                    } 
                }
            });
        }

    }
}
