using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace OotD.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var bitness = ValidateOutlookInstallation();

            switch (bitness.ToLowerInvariant())
            {
                case "x64":
                    {
                        var processStartInfo = new ProcessStartInfo("OotD.x64.exe")
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        Process.Start(processStartInfo);
                        break;
                    }
                case "x86":
                    {
                        var processStartInfo = new ProcessStartInfo("OotD.x86.exe")
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        Process.Start(processStartInfo);
                        break;
                    }
            }
        }

        /// <summary>
        /// Validates that Outlook is installed and gets the bitness of the installed version.
        /// </summary>
        /// <returns></returns>
        private static string ValidateOutlookInstallation()
        {
            string outlookPath = String.Empty;
            double versionSubKey = 0;

            // first make sure they have Office/Outlook 2000 (9.0) or higher installed by looking for 
            // the version subkeys in HKLM.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office"))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        if (Double.TryParse(subkey, NumberStyles.Float, new NumberFormatInfo(), out var version))
                        {
                            if (version >= 11 || version > versionSubKey)
                            {
                                versionSubKey = version;
                            }
                        }
                    }
                }
            }

            if (versionSubKey <= 0)
            {
                Console.WriteLine($"Could not determine Office version");
            }

            Console.WriteLine($"Office version {versionSubKey} detected");

            // now check for the existence of the actual Outlook.exe.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
            {
                if (key != null) outlookPath = (string)key.GetValue("Path");
                if (outlookPath != null)
                {
                    if (!File.Exists(Path.Combine(outlookPath, "Outlook.exe")))
                    {
                        // throw
                    }
                }
            }

            Console.WriteLine($"Outlook path reported as {outlookPath}");

            if (!String.IsNullOrEmpty(outlookPath))
            {
                // throw?
            }

            // now check for bitness
            var outlookKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
            string bitness = null;

            if (outlookKey != null)
            {
                bitness = (string)outlookKey.GetValue("Bitness");
            }
            else
            {
                outlookKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");

                if (outlookKey != null)
                {
                    bitness = (string)outlookKey.GetValue("Bitness");
                }
            }

            if (outlookKey == null)
            {
                Console.WriteLine("Unable to get Bitness");
            }
            else if (!string.IsNullOrWhiteSpace(bitness))
            {
                Console.WriteLine($"Outlook Bitness is: {bitness}");
            }

            return bitness;
        }
    }
}
