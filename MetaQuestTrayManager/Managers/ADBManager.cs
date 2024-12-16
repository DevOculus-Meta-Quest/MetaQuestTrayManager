using System.Diagnostics;
using System.IO;

namespace MetaQuestTrayManager.Managers
{
    public class ADBManager
    {
        private readonly string adbPath;

        public ADBManager()
        {
            adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Binaries", "adb.exe");
        }

        public string RunCommand(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        public List<string> GetConnectedDevices()
        {
            string output = RunCommand("devices");
            var devices = new List<string>();

            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("\tdevice"))
                {
                    devices.Add(line.Split('\t')[0]);
                }
            }
            return devices;
        }
    }
}
