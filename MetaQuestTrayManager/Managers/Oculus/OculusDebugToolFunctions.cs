using MetaQuestTrayManager.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MetaQuestTrayManager.Managers.Oculus
{
    /// <summary>
    /// Manages interactions with the Oculus Debug Tool CLI.
    /// </summary>
    public class OculusDebugToolFunctions : IDisposable
    {
        private const string OculusDebugToolPath = @"C:\\Program Files\\Oculus\\Support\\oculus-diagnostics\\OculusDebugToolCLI.exe";
        private Process _process;
        private StreamWriter _streamWriter;

        /// <summary>
        /// Initializes the Oculus Debug Tool process.
        /// </summary>
        public OculusDebugToolFunctions() => InitializeProcess();

        /// <summary>
        /// Executes a single command asynchronously.
        /// </summary>
        /// <param name="command">The command to send to the Oculus Debug Tool.</param>
        public async Task ExecuteCommandAsync(string command)
        {
            try
            {
                if (_streamWriter == null)
                    throw new InvalidOperationException("The Oculus Debug Tool process is not initialized.");

                Debug.WriteLine($"Sending command: {command}");
                await _streamWriter.WriteLineAsync(command);
                await _streamWriter.FlushAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing command: {ex.Message}");
                ErrorLogger.LogError(ex, "Failed to execute command in Oculus Debug Tool.");
            }
        }

        /// <summary>
        /// Executes commands from a file via the Oculus Debug Tool.
        /// </summary>
        /// <param name="tempFilePath">The path to the file containing the commands.</param>
        public void ExecuteCommandWithFile(string tempFilePath)
        {
            if (!File.Exists(tempFilePath))
            {
                Debug.WriteLine($"File not found: {tempFilePath}");
                return;
            }

            try
            {
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = OculusDebugToolPath,
                        Arguments = $"-f \"{tempFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                // Capture output and errors
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();

                // Log results or handle accordingly
                Debug.WriteLine($"Output: {output}\nError: {error}");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "An error occurred while executing the command file in Oculus Debug Tool.");
            }
        }

        /// <summary>
        /// Initializes the Oculus Debug Tool process for interactive commands.
        /// </summary>
        private void InitializeProcess()
        {
            try
            {
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = OculusDebugToolPath,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _process.OutputDataReceived += (sender, args) => Debug.WriteLine(args.Data);
                _process.ErrorDataReceived += (sender, args) => Debug.WriteLine(args.Data);

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                _streamWriter = _process.StandardInput;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to initialize Oculus Debug Tool CLI process.");
                throw;
            }
        }

        /// <summary>
        /// Disposes the Oculus Debug Tool process and resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _streamWriter?.Close();
                _streamWriter?.Dispose();
                _process?.Kill();
                _process?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }
    }
}
