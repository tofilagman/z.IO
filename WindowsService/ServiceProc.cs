using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace z.IO.WindowsService
{
    public class ServiceProc : IDisposable
    {
        private EventLog myLog;
        public string ProcName { get; private set; }
        public string ServicePath { get; private set; }
        public string[] Arguments { get; private set; }

        public delegate void WriteEvent(EventLogEntry Entry);
        public event WriteEvent OnStatus;
        private ServiceController service;

        private ServiceControllerStatus status = ServiceControllerStatus.StartPending;
        public delegate void ServiceStatusChange(ServiceControllerStatus status);
        public event ServiceStatusChange OnServiceStatusChange;

        private System.Timers.Timer timer;

        public ServiceProc(string ProcName, string ServicePath, params string[] Arguments)
        {
            this.ProcName = ProcName;
            this.ServicePath = ServicePath.Replace("file:///", "");
            this.Arguments = Arguments;

            this.service = new ServiceController(this.ProcName);

            //this.status = this.service.Status;
            StartListener();
            this.Listen();
        }

        public ServiceProc(string ProcName)
        {

            this.ProcName = ProcName;
            this.ServicePath = "";
            this.service = new ServiceController(this.ProcName);
            this.status = this.service.Status;
            StartListener();
            this.Listen();

        }

        void StartListener()
        {
            this.timer = new System.Timers.Timer(1000);
            this.timer.Enabled = true;
            this.timer.Elapsed += (a, b) =>
            {
                if (this.OnServiceStatusChange != null && this.service != null)
                {
                    try
                    {
                        this.service.WaitForStatus(this.status, new TimeSpan(1000));
                        if (this.status != this.service.Status)
                        {
                            this.status = this.service.Status;
                            this.OnServiceStatusChange(this.service.Status);
                        }
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        this.status = this.service.Status;
                        this.OnServiceStatusChange(this.service.Status);
                    }
                }
            };
            this.timer.Start();
        }

        public void StartService(int timeoutMilliseconds)
        {
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                service.Start(this.Arguments);
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        public void StopService(int timeoutMilliseconds)
        {
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch
            {
                // ...
            }
        }



        public void RestartService(int timeoutMilliseconds, Action OnRestarted)
        {
            try
            {
                StopService(timeoutMilliseconds);
                var tmr = new System.Timers.Timer(1000);
                tmr.Elapsed += (a, b) =>
                {
                    service.Refresh();
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        StartService(timeoutMilliseconds);
                    }
                    else if (service.Status == ServiceControllerStatus.Running)
                    {
                        tmr.Stop();
                        tmr.Dispose();
                        OnRestarted();
                    }
                };
                tmr.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendCommand(int Command, int timeoutMilliseconds)
        {
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                this.service.ExecuteCommand(Command);
                this.service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InstallService()
        {
            try
            {
                if (this.ServicePath == "") throw new Exception("Service Not Defined");
                ManagedInstallerClass.InstallHelper(new string[] { this.ServicePath });

                if (!EventLog.SourceExists(ProcName))
                {
                    EventLog.CreateEventSource(ProcName, "log_" + ProcName);
                }

                Listen();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UninstallService()
        {
            try
            {
                if (this.ServicePath == "") throw new Exception("Service Not Defined");
                ManagedInstallerClass.InstallHelper(new string[] { "/u", this.ServicePath });

                if (EventLog.SourceExists(ProcName))
                {
                    EventLog.DeleteEventSource(ProcName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Listen()
        {
            if (EventLog.SourceExists(ProcName))
            {
                this.myLog = new EventLog();
                this.myLog.Source = this.ProcName;

                // set event handler
                myLog.EntryWritten += (a, b) =>
                {
                    if (b.Entry.Source != ProcName) return;
                    if (this.OnStatus != null) this.OnStatus(b.Entry);
                };

                myLog.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// You must reference: System.ServiceProcess
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="IsInstalled"></param>
        public void CheckServiceStatus(Action<ServiceControllerStatus> Status, Action<bool> IsInstalled = null)
        {
            if (IsServiceInstalled)
            {
                Status(service.Status);
                if (IsInstalled != null) IsInstalled(true);
            }
            else
            {
                if (IsInstalled != null) IsInstalled(false);
            }
        }

        public bool IsServiceInstalled
        {
            get
            {
                service.Refresh();
                return ServiceController.GetServices().Any(x => x.ServiceName == this.ProcName);
            }
        }

        public string ComputerName
        {
            set
            {
                this.service.MachineName = value;
            }
        }

        public static List<string> ListOfService()
        {
            return (from j in ServiceController.GetServices() where j.Status == ServiceControllerStatus.Running orderby j.ServiceName select j.ServiceName).ToList();
        }

        public static void WriteEntry(string ServiceName, string Message, EventLogEntryType logtype = EventLogEntryType.Information)
        {
            EventLog.WriteEntry(ServiceName, Message, logtype);
        }

        public static void WriteEntry(string ServiceName, string Message, EventLogEntryType logtype, params object[] args)
        {
            WriteEntry(ServiceName, string.Format(Message, args), logtype);
        }

        ~ServiceProc()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (OnStatus != null) OnStatus = null;
            if (timer != null) timer.Dispose();
            service.Dispose();
            service = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        //public class StatusChange : IDisposable
        //{

        //    private System.Timers.Timer timer;
        //    private ServiceControllerStatus status = ServiceControllerStatus.StartPending;
        //    public delegate void ServiceStatusChange(ServiceControllerStatus status);
        //    public event ServiceStatusChange OnServiceStatusChange;

        //    public StatusChange(ServiceController service)
        //    {
        //        this.timer = new System.Timers.Timer(1);
        //        this.timer.Elapsed += (a, b) =>
        //        {
        //            //if(status == null){
        //            //    this.status = service.Status;
        //            //    if (OnServiceStatusChange != null) OnServiceStatusChange(service.Status);
        //            //}
        //            //else
        //            //{
        //            if (this.status != service.Status)
        //            {
        //                this.status = service.Status;
        //                if (OnServiceStatusChange != null) OnServiceStatusChange(service.Status);
        //            }

        //            //}
        //        };
        //    }

        //    public void Start()
        //    {
        //        this.timer.Start();
        //    }

        //    public void Stop()
        //    {
        //        this.timer.Stop();
        //        this.status = ServiceControllerStatus.Stopped;
        //    }

        //    ~StatusChange()
        //    {
        //        Dispose();
        //    }

        //    public void Dispose()
        //    {
        //        timer.Dispose();
        //        GC.Collect();
        //        GC.SuppressFinalize(this);
        //    }
        //}
    }
}
