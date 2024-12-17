using MetaQuestTrayManager.Utils;
using MetaQuestTrayManager.Managers.Steam;
using MetaQuestTrayManager.Managers.Oculus;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace MetaQuestTrayManager.Managers
{
    /// <summary>
    /// Manages various UI-related functionalities and interactions for the application.
    /// </summary>
    public class UIManager
    {
        private readonly MainWindow _window;

        /// <summary>
        /// Initializes the UIManager with a reference to the main window.
        /// </summary>
        /// <param name="window">The main application window.</param>
        public UIManager(MainWindow window) => _window = window;

        /// <summary>
        /// Shows a warning if Desktop+ is not installed.
        /// </summary>
        public void ShowDesktopPlusNotInstalledWarning()
        {
            _window.Dispatcher.Invoke(() =>
            {
                _window.Topmost = false; // Temporarily disable topmost
                MessageBox.Show(_window, "Desktop+ is not installed. Some functionality may be limited.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _window.Topmost = true; // Restore topmost
            });
        }

        /// <summary>
        /// Displays a warning about missing .NET frameworks.
        /// </summary>
        /// <param name="missingFrameworks">List of missing frameworks.</param>
        public void ShowFrameworkNotInstalledWarning(string missingFrameworks)
        {
            _window.Dispatcher.Invoke(() =>
            {
                _window.Topmost = false;
                MessageBox.Show(_window,
                    $"The following .NET Frameworks are not installed: {missingFrameworks}.\n" +
                    "Please install the required frameworks to ensure all features work correctly.\n\n" +
                    "Download from: https://dotnet.microsoft.com/download/dotnet",
                    "Framework Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                _window.Topmost = true;
            });
        }

        /// <summary>
        /// Notifies the user that the program must be run with administrator permissions.
        /// </summary>
        public void NotifyNotElevated()
        {
            _window.Dispatcher.Invoke(() =>
            {
                _window.lbl_CurrentSetting.Content = "Run as Admin Required";
                MessageBox.Show(_window,
                    "This program must be run with Admin Permissions.\n\n" +
                    "Right-click the program file and select 'Run as administrator'.\n\n" +
                    "Alternatively, go to Properties -> Compatibility and check 'Run this program as an administrator'.",
                    "Admin Permissions Required", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        /// <summary>
        /// Updates the SteamVR beta status label in the UI.
        /// </summary>
        public void UpdateSteamVRBetaStatus()
        {
            _window.Dispatcher.Invoke(() =>
            {
                try
                {
                    bool isSteamVRBeta = SteamAppChecker.IsSteamVRBeta();
                    _window.lbl_SteamVR_BetaStatus.Content = isSteamVRBeta ? "Beta Edition" : "Normal Edition";
                }
                catch (Exception ex)
                {
                    ShowMessageBox($"An error occurred while checking SteamVR beta status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        /// <summary>
        /// Updates the current setting label in the UI.
        /// </summary>
        /// <param name="text">The text to display in the label.</param>
        public void UpdateStatusLabel(string text)
        {
            _window.Dispatcher.Invoke(() => _window.lbl_CurrentSetting.Content = text);
        }

        /// <summary>
        /// Updates the SteamVR status label in the UI.
        /// </summary>
        /// <param name="text">The text to display in the label.</param>
        public void UpdateSteamVRStatusLabel(string text)
        {
            _window.Dispatcher.Invoke(() => _window.lbl_SteamVR_Status.Content = text);
        }

        /// <summary>
        /// Updates the Desktop+ installation status label in the UI.
        /// </summary>
        /// <param name="isInstalled">Indicates if Desktop+ is installed.</param>
        public void UpdateDesktopPlusStatusLabel(bool isInstalled)
        {
            string statusText = isInstalled ? "Installed: True" : "Installed: False";

            _window.Dispatcher.Invoke(() =>
            {
                _window.lbl_DesktopPlusStatus.Content = statusText;
            });
        }

        /// <summary>
        /// Enables or disables a button in the UI.
        /// </summary>
        /// <param name="buttonName">The name of the button to enable or disable.</param>
        /// <param name="isEnabled">True to enable the button; false to disable it.</param>
        public void EnableButton(string buttonName, bool isEnabled)
        {
            _window.Dispatcher.Invoke(() =>
            {
                switch (buttonName)
                {
                    case "Diagnostics":
                        _window.btn_Diagnostics.IsEnabled = isEnabled;
                        break;
                    case "OpenSettings":
                        _window.btn_OpenSettings.IsEnabled = isEnabled;
                        break;
                    default:
                        throw new ArgumentException($"Invalid button name: {buttonName}", nameof(buttonName));
                }
            });
        }

        /// <summary>
        /// Displays a message box in the UI.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        public void ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            _window.Dispatcher.Invoke(() => MessageBox.Show(_window, message, title, buttons, icon));
        }

        /// <summary>
        /// Opens the Oculus Dash installation directory in the file explorer.
        /// </summary>
        public void OpenDashLocation()
        {
            _window.Dispatcher.Invoke(() =>
            {
                if (OculusRunning.Oculus_Is_Installed)
                {
                    if (Directory.Exists(OculusRunning.Oculus_Dash_Directory))
                    {
                        FileExplorerUtilities.ShowFileInDirectory(OculusRunning.Oculus_Dash_Directory);
                    }
                    else
                    {
                        ShowMessageBox("Oculus Dash directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    ShowMessageBox("Oculus is not installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        /// <summary>
        /// Executes a specified UI action on the main window's dispatcher.
        /// </summary>
        /// <param name="form">The window on which the action should be performed.</param>
        /// <param name="doAction">The action to execute.</param>
        public static void DoAction(Window form, Action doAction)
        {
            if (form == null || doAction == null)
                throw new ArgumentNullException(form == null ? nameof(form) : nameof(doAction));

            try
            {
                form.Dispatcher.Invoke(doAction, DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error performing UI action.");
            }
        }
    }
}
