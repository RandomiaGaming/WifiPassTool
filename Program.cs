using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace WifiPassTool
{
    public static class Program
    {
        public static void Main()
        {
            string[] wifiNames = GetWifiNames();
            if (!(wifiNames is null))
            {
                foreach (string wifiName in wifiNames)
                {
                    string wifiPassword = GetWifiPassword(wifiName);
                    if (!(wifiPassword is null))
                    {
                        Console.WriteLine(wifiName + ": " + wifiPassword);
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        public static string GetWifiPassword(string wifiName)
        {
            ProcessStartInfo commandStartInfo = new ProcessStartInfo();
            commandStartInfo.Arguments = "wlan show profile name=\"" + wifiName + "\" key=clear";
            commandStartInfo.CreateNoWindow = true;
            commandStartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\netsh.exe";
            commandStartInfo.LoadUserProfile = true;
            commandStartInfo.RedirectStandardOutput = true;
            commandStartInfo.UseShellExecute = false;
            commandStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            commandStartInfo.WorkingDirectory = null;

            Process commandProcess = Process.Start(commandStartInfo);

            commandProcess.WaitForExit(5000);

            if (!commandProcess.HasExited)
            {
                Console.WriteLine("Netsh.exe has timed out while preforming command \"wlan show profile name='" + wifiName + "' key=clear\". Try restarting your computer or closing background apps.");
                return null;
            }

            string stdOut = commandProcess.StandardOutput.ReadToEnd().Replace("\r", "");
            string[] stdOutSplit = stdOut.Split('\n');

            if (stdOutSplit.Length is 0)
            {
                Console.WriteLine("Netsh.exe returned no output for wifi profile \"" + wifiName + "\". Try restarting your computer or running the application again.");
                return null;
            }

            if (stdOutSplit[0] is "The Wireless AutoConfig Service (wlansvc) is not running.")
            {
                Console.WriteLine("Wlansvc is not running. Most likely this is because your computer does not support wifi. Wired connections do not have a password.");
                return null;
            }

            if (stdOutSplit[0] == "Profile \"" + wifiName + "\" is not found on the system.")
            {
                Console.WriteLine("Wifi profile \"" + wifiName + "\" was not found on this system.");
                return null;
            }

            string unformattedOutput = null;
            for (int i = 0; i < stdOutSplit.Length; i++)
            {
                if (stdOutSplit[i].Contains("Key Content"))
                {
                    unformattedOutput = stdOutSplit[i];
                    break;
                }
            }

            if (unformattedOutput is null)
            {
                Console.WriteLine("Netsh.exe returned no password for wifi profile \"" + wifiName + "\". Try running as a different user or running as administrator.");
                return null;
            }

            int index = unformattedOutput.IndexOf(": ");
            if (index is -1 || index + 2 >= unformattedOutput.Length)
            {
                Console.WriteLine("Malformatted password string from netsh.exe for wifi profile \"" + wifiName + "\".");
                return null;
            }

            return unformattedOutput.Substring(index + 2);
        }
        public static string[] GetWifiNames()
        {
            ProcessStartInfo commandStartInfo = new ProcessStartInfo();
            commandStartInfo.Arguments = "wlan show profile";
            commandStartInfo.CreateNoWindow = true;
            commandStartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\netsh.exe";
            commandStartInfo.LoadUserProfile = true;
            commandStartInfo.RedirectStandardOutput = true;
            commandStartInfo.UseShellExecute = false;
            commandStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            commandStartInfo.WorkingDirectory = null;

            Process commandProcess = Process.Start(commandStartInfo);

            commandProcess.WaitForExit(5000);

            if (!commandProcess.HasExited)
            {
                Console.WriteLine("Netsh.exe has timed out while preforming command \"wlan show profile\". Try restarting your computer or closing background apps.");
                return null;
            }

            string stdOut = commandProcess.StandardOutput.ReadToEnd().Replace("\r", "");
            string[] stdOutSplit = stdOut.Split('\n');

            if (stdOutSplit.Length is 0)
            {
                Console.WriteLine("Netsh.exe returned no output. Try restarting your computer or running the application again.");
                return null;
            }

            if (stdOutSplit[0] is "The Wireless AutoConfig Service (wlansvc) is not running.")
            {
                Console.WriteLine("Wlansvc is not running. Most likely this is because your computer does not support wifi. Wired connections do not have a password.");
                return null;
            }

            int startIndex = -1;
            for (int i = 0; i < stdOutSplit.Length; i++)
            {
                if (stdOutSplit[i] is "User profiles")
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex is -1)
            {
                Console.WriteLine("User profiles list was not found in output from netsh.exe.");
                return null;
            }

            int endIndex = stdOutSplit.Length - 1;
            for (int i = startIndex; i < stdOutSplit.Length; i++)
            {
                if (stdOutSplit[i] is "")
                {
                    endIndex = i;
                    break;
                }
            }

            if (endIndex - startIndex < 3)
            {
                Console.WriteLine("Malformatted output from netsh.exe.");
                return null;
            }

            string[] unformattedOutput = new string[endIndex - (startIndex + 2)];

            Array.Copy(stdOutSplit, startIndex + 2, unformattedOutput, 0, unformattedOutput.Length);

            if (unformattedOutput.Length is 1 && unformattedOutput[0].Replace(" ", "") is "<None>")
            {
                Console.WriteLine("No wifi profiles were found on this system. Try running as a different user or running as administrator.");
                return null;
            }

            List<string> output = new List<string>();

            for (int i = 0; i < unformattedOutput.Length; i++)
            {
                int index = unformattedOutput[i].IndexOf(": ");
                if (index is -1 || index + 2 >= unformattedOutput[i].Length)
                {
                    Console.WriteLine("Malformatted wifi profile from netsh.exe.");
                }
                else
                {
                    output.Add(unformattedOutput[i].Substring(index + 2));
                }
            }
            return output.ToArray();
        }
    }
}