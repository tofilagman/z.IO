using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using IWshRuntimeLibrary;
using System.IO;

namespace z.IO
{
  public sealed class ShortCut
    {
        public static void CreateStartupFolderShortcut(string filename, string execpath, string description, string icon, string Arguments)
        {
            WshShellClass wshShell = new WshShellClass();
            IWshRuntimeLibrary.IWshShortcut shortcut;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            // Create the shortcut
            shortcut =
              (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                startUpFolderPath + "\\" +
                filename + ".lnk");

            shortcut.TargetPath = execpath;
            shortcut.Arguments = Arguments;
            shortcut.WorkingDirectory = execpath;
            shortcut.Description = description;
            shortcut.IconLocation = icon;
            shortcut.Save();
        }

        public static void DeleteStartupFolderShortcuts(string targetExeName)
        {
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName).Replace("-s", "").Trim();

                if (shortcutTargetFile.EndsWith(targetExeName,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.Delete(fi.FullName);
                }
            }
        }

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link =
                  (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return String.Empty; // Not found
        }
    }
}
