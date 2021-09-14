using System;
using System.IO;
using Microsoft.Win32;

namespace USB_MOUNT_DAEMON
{
    class FirstTimeRun
    {
        public static bool IsAlreadyInstalled() 
        {
            // Open the Startup base Key for the current user (not machine-wide) and try to get the sub-value of our Program //
            object subKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run").GetValue("USB2Desktop");
            // return true if object pair exists and install is thereby complete //
            if (subKey == null)
                return false;
            else
                return true;
        }

        public static void RunAtStartup()
        {
            // Copy this application from its current running Dir to <User>/Appdata/Roaming //
            File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\USB2Desktop.exe");
            // Create new Registry-Value-Pair in the current user's Startup Key and attach our Appdata-File-Path to it //
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rkApp.SetValue("USB2Desktop", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\USB2Desktop.exe");
        }
    }
}
