﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;

namespace DataButler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            UpdateCheck();
            SetupRegistry();
            if (DontRun()) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Restore(Environment.GetCommandLineArgs()[1]));
        }

        static void UpdateCheck()
        {
            try
            {
                var latestVersionString = XDocument.Load("http://i1047/DataButler/DataButler.application").Descendants().ToArray()[1].Attribute("version").Value;
                var latestVersion = Convert.ToInt16(latestVersionString.Replace(".", ""));
                var installedVersion = Convert.ToInt16(Application.ProductVersion.Replace(".", ""));
                var updateString = string.Format("An update to DataButler is ready. Update?{0}{0}Latest version: {1}{0}Installed version:{2}", Environment.NewLine, latestVersionString, Application.ProductVersion);
                if (latestVersion > installedVersion && MessageBox.Show(updateString, "Updates", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start("http://i1047/DataButler/publish.htm");
                    Application.Exit();
                }
            }
            catch (Exception)
            {
                //Let them pass
            }            
        }

        static bool DontRun()
        {
            if (Environment.GetCommandLineArgs().Count() != 2) return true;
            if (File.Exists(Environment.GetCommandLineArgs()[1])) return false;
            MessageBox.Show("Invalid database backup.", "DataButler");
            return true;
        }

        static void SetupRegistry()
        {
            var dataButlerPath = string.Format(@"{0} ""%1""", Process.GetCurrentProcess().MainModule.FileName);
            SetupContextMenuAssociations(dataButlerPath);
        }

        static void SetupContextMenuAssociations(string dataButlerPath)
        {
            var bakHive = @"Software\Classes\.bak";
            if (!HiveExists(bakHive)) CreateHive(bakHive);
            SetDefaultHiveValue(bakHive, "DataButler");

            var commandHive = @"Software\Classes\DataButler\shell\Restore via DataButler\command";
            if (!HiveExists(commandHive)) CreateHive(commandHive);
            SetDefaultHiveValue(commandHive, dataButlerPath);

            var openCommandHive = @"Software\Classes\DataButler\shell\open\command";
            if (!HiveExists(openCommandHive)) CreateHive(openCommandHive);
            SetDefaultHiveValue(openCommandHive, dataButlerPath);
        }

        static bool HiveExists(string hive)
        {
            var softwareKey = Registry.CurrentUser.OpenSubKey(hive);
            return softwareKey != null;
        }

        static void CreateHive(string bakHive)
        {
            Registry.CurrentUser.CreateSubKey(bakHive);
        }

        static void SetDefaultHiveValue(string hive, string defaultValue)
        {
            var key = Registry.CurrentUser.OpenSubKey(hive, RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(null, defaultValue);
        }
    }
}
