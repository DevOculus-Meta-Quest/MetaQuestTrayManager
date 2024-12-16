using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace MetaQuestTrayManager.Utils
{
    public static class ErrorLogger
    {
        private static readonly string LogFilePath;

        static ErrorLogger()
        {
            // Define a custom log directory under AppData\Local\MetaQuestTrayManager\Logs
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MetaQuestTrayManager",
                "Logs");

            // Ensure the directory exists
            Directory.CreateDirectory(logDirectory);

            // Create the log file path
            LogFilePath = Path.Combine(logDirectory, "ErrorLog.txt");
        }

        /// <summary>
        /// Logs an exception to the error log file with optional additional information.
        /// </summary>
        public static void LogError(Exception ex, string additionalInfo = "")
        {
            try
            {
                // Create a string builder to store error details
                var errorDetails = new StringBuilder();

                errorDetails.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                if (!string.IsNullOrEmpty(additionalInfo))
                {
                    errorDetails.AppendLine($"Additional Info: {additionalInfo}");
                }

                errorDetails.AppendLine($"Exception Type: {ex.GetType().Name}");
                errorDetails.AppendLine($"Message: {ex.Message}");
                errorDetails.AppendLine($"StackTrace: {ex.StackTrace}");

                // Log inner exceptions recursively
                if (ex.InnerException != null)
                {
                    errorDetails.AppendLine("Inner Exception:");
                    errorDetails.AppendLine(LogInnerException(ex.InnerException, "  "));
                }

                errorDetails.AppendLine(new string('-', 100)); // Separator for readability

                // Write error details to the log file
                File.AppendAllText(LogFilePath, errorDetails.ToString());
            }
            catch
            {
                // Optionally: Fail silently if logging fails
                // You might choose to display a message or handle this case differently
            }
        }

        /// <summary>
        /// Recursively logs inner exceptions with proper indentation.
        /// </summary>
        private static string LogInnerException(Exception ex, string indent)
        {
            var innerDetails = new StringBuilder();

            innerDetails.AppendLine($"{indent}Exception Type: {ex.GetType().Name}");
            innerDetails.AppendLine($"{indent}Message: {ex.Message}");
            innerDetails.AppendLine($"{indent}StackTrace: {ex.StackTrace}");

            // Handle further nested inner exceptions
            if (ex.InnerException != null)
            {
                innerDetails.AppendLine($"{indent}Inner Exception:");
                innerDetails.AppendLine(LogInnerException(ex.InnerException, indent + "  "));
            }

            return innerDetails.ToString();
        }
    }
}
