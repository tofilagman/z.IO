
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace z.IO
{
    /// <summary>
    /// LJ 20140918
    /// </summary>
    public class Scheduler : IDisposable
    {

        public string Name { get; private set; }
        public string Folder { get; private set; }

        private TaskService task;
        private TaskDefinition def;

        public Scheduler()
        {
            this.task = new TaskService();
        }

        public Scheduler(string name, string folder)
        {
            this.Name = name;
            this.Folder = folder;
            this.task = new TaskService();
        }

        bool CheckFolderExist(string name)
        {
            return this.task.RootFolder.SubFolders.Where(x => x.Name == name).Any();
        }

        public void DefineTask(string Author, string Description)
        {
            def = task.NewTask();
            def.RegistrationInfo.Description = Description;
            def.RegistrationInfo.Author = Author;
            def.RegistrationInfo.Date = DateTime.Now;
        }

        public void AddTrigger(DateTime dtime)
        {
            DailyTrigger trger = new DailyTrigger();
            trger.DaysInterval = 1;
            trger.StartBoundary = dtime;
            def.Triggers.Add(trger);
        }

        public void DefineAction(string AppPath, string args = "")
        {
            ExecAction action = new ExecAction(AppPath, args);
            def.Actions.Add(action);

            def.Principal.RunLevel = TaskRunLevel.Highest;
            def.Settings.Priority = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            //def.Settings.RunOnlyIfLoggedOn = false;
        }

        public void Register()
        {
            TaskFolder tf = task.RootFolder;
            if (!CheckFolderExist(this.Folder))
            {
                tf.CreateFolder(this.Folder);
            }

            tf.SubFolders[this.Folder].RegisterTaskDefinition(this.Name, def, TaskCreation.Create, "SYSTEM", null, TaskLogonType.ServiceAccount);
        }

        public TaskCollection GetRegisteredTasks(string folder)
        {
            if (this.CheckFolderExist(folder))
            {
                return task.RootFolder.SubFolders[folder].Tasks;
            }
            else
            {
                throw new Exception("Folder not exists");
            }
        }

        public Task GetTask(string folder, string pname)
        {
            return this.GetRegisteredTasks(folder).Where(x => x.Name == pname).SingleOrDefault();
        }

        ~Scheduler()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (def != null) def.Dispose();
            task.Dispose();

            GC.Collect();
            GC.SuppressFinalize(this);
        }

       
    }
}
