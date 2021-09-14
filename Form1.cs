using Dolinay;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;

namespace USB_MOUNT_DAEMON
{
    public partial class Form1 : Form
    {
        // Initiating Variables //
        private DriveDetector driveDetector = null;
        private static string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        List<string> LinkList = new List<string>();
        private string IconPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Drive.ico";

        public Form1()
        {
            InitializeComponent();
            this.Hide();
            // Check if the Program has already been run, currently will ask again on next app-start if you deny permission //
            if (!FirstTimeRun.IsAlreadyInstalled())
            {
                if (MessageBox.Show("Do you want USB2Desktop\nto run at system start?", "First Time Setup", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    FirstTimeRun.RunAtStartup();
            }

            // Generating System-Tray Context Options: ShowForm & Exit //
            notifyIcon1.ContextMenu = new ContextMenu();
            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("Show Debug Window", new EventHandler(ShowDebugWindow)));
            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("Stop USB2Desktop", new EventHandler(Exit)));

            // Create DriveDetector instances and appoint Handlers, DriveDetector.cs is the brainchild of Jan Dolinay //
            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
        }

        // Method to Show the Debug-/Form-Window //
        private void ShowDebugWindow(object sender, EventArgs e) 
        {
            this.Show();
        }

        // Exit the Application //
        private void Exit(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        // Handler for the arrival of a new Drive //
        private void OnDriveArrived(object sender, DriveDetectorEventArgs e) 
        {
            // Write Drivepath to Debug-Window //
            listBox1.Items.Add("Detected USB mounted on: " + e.Drive);
            try
            {
                // Attempt to create a new Shortcut on the User's Desktop with the Name of the Mountpoint (e.g. "F:\ => F") //
                CreateShortcut(Desktop, e.Drive.Substring(0, e.Drive.LastIndexOf(":")), e.Drive);
                // Add the Path of the new Shortcut to the Shortcut-Buffer //
                LinkList.Add(Desktop + "\\" + e.Drive.Substring(0, e.Drive.LastIndexOf(":")));
                // Simple Workaround to refresh the Desktop, using the Toggling of Desktop Icons; answer by SepehrM via StackOverflow //
                DesktopRefresh.ToggleDesktopIcons();
                DesktopRefresh.ToggleDesktopIcons();
            }
            catch 
            { 
                // Ignore //
            }
        }

        // Handler for the removal of a Drive //
        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            // Buffer for a matching Path //
            string Path = "";
            // Notify Debug-Window of the unmount //
            listBox1.Items.Add("Detected USB unmount on " + e.Drive);
            
            // Check Shortcut-Buffer, if the removed Device corresponds to a mounted Drive, if it does, set Pathbuffer to the corresponding Path //
            foreach (string I in LinkList) 
            {
                if (I == Desktop + "\\" + e.Drive.Substring(0, e.Drive.LastIndexOf(":")))
                {
                    Path = I;
                }
            }
            // If the Shortcut from the Pathbuffer can be deleted, delete the Pathbuffer-Content from the Shortcut-Buffer //
            if (RemoveShortcut(Path + ".lnk")) 
            {
                LinkList.Remove(Path);
                // Refresh the Desktop once again //
                DesktopRefresh.ToggleDesktopIcons();
                DesktopRefresh.ToggleDesktopIcons();
            }
            
        }
        // Method to return a status after deleting a File //s
        private bool RemoveShortcut(string Path) 
        {
            // Delete specified File, if the File doesn't exist afterwards return true //
            System.IO.File.Delete(Path);
            if (!System.IO.File.Exists(Path)) 
            {
                return true;
            }
            return false;
        }

        // Create new Shortcut via VBScript; answer by Dzmitry Lahoda via StackOverflow //
        private void CreateShortcut(string Dest, string Name, string Target) 
        {
            // Create instance of the VBScript //
            var wsh = new IWshShell_Class();
            // Create new Shortcut template with the Destination and the Shortcut-Path //
            IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(Dest + "\\" + Name + ".lnk") as IWshRuntimeLibrary.IWshShortcut;
            shortcut.TargetPath = Target;
            // Set the Shortcut's Icon to the Windows-Drive-Icon (SHELL32.dll with an offset of 8) and save it afterwards //
            shortcut.IconLocation = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\System32\\SHELL32.dll, 8";
            shortcut.Save();
        }
        // Hide the Debug-Window on Click of "Exit button" //
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
