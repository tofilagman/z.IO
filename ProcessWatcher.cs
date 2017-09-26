using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

namespace z.IO
{
    /// <summary>
    /// LJ 20150820
    /// </summary>
    public class ProcessWatcher
    {
        public string ProcessName { get; private set; }
        private Timer tmr;
        private bool IsAlive = false;

        public delegate void ProcessChangeHandler(ProcessWatcher Sender, ProcessChangeEventArgs e);
        public event ProcessChangeHandler ProcessChange;

        public ProcessWatcher(string ProcessName)
        {
            this.ProcessName = ProcessName;
            tmr = new Timer();
            tmr.Interval = 1000;
            tmr.Elapsed += tmr_Elapsed;
        }

        public void Start()
        {
            tmr.Start();
        }

        public void Stop()
        {
            tmr.Stop();
        }

        void tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var prc = System.Diagnostics.Process.GetProcesses().Where(x => x.ProcessName.ToLower() == this.ProcessName);

                if (this.IsAlive != prc.Any()) //if change throw
                {
                    this.ProcessChange(this, new ProcessChangeEventArgs() { 
                        IsAlive = prc.Any(),
                        Process = (this.IsAlive) ? prc.FirstOrDefault() : null
                    });
                }

                this.IsAlive = prc.Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    public class ProcessChangeEventArgs
    {
        public Process Process { get; set; }
        public bool IsAlive { get; set; }
    }
}
