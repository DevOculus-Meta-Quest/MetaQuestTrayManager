using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;

namespace MetaQuestTrayManager.Managers
{
    public static class GetControllers
    {
        // Dictionary to hold controller names and IDs
        public static Dictionary<string, int> Controllers { get; private set; } = new Dictionary<string, int>();

        public static bool ControllersFound = false;
        public static string SelectedDevice = string.Empty;
        private static int SelectedJoystick = -1;

        /// <summary>
        /// Detect all connected controllers and populate the Controllers dictionary.
        /// </summary>
        public static void DetectAllControllers()
        {
            Controllers.Clear();
            ControllersFound = false;

            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.StartVisible = false; // Avoid displaying a window during detection

            using (var window = new GameWindow(gameWindowSettings, nativeWindowSettings))
            {
                Console.WriteLine("Detecting connected controllers...");

                // Iterate over possible joystick IDs (0-15)
                for (int id = 0; id < 16; id++)
                {
                    if (GLFW.JoystickPresent(id))
                    {
                        string name = GLFW.GetJoystickName(id);
                        if (!string.IsNullOrEmpty(name))
                        {
                            Controllers[name] = id;
                            Console.WriteLine($"Found Controller: {name} (ID: {id})");
                        }
                    }
                }

                ControllersFound = Controllers.Count > 0;
                if (!ControllersFound)
                    Console.WriteLine("No controllers detected.");
            }
        }

        /// <summary>
        /// Select a controller from the Controllers dictionary.
        /// </summary>
        /// <param name="controllerName">The name of the controller to select.</param>
        public static void SelectController(string controllerName)
        {
            if (Controllers.TryGetValue(controllerName, out int joystickId))
            {
                SelectedJoystick = joystickId;
                SelectedDevice = controllerName;
                Console.WriteLine($"Selected Controller: {controllerName} (ID: {joystickId})");
            }
            else
            {
                Console.WriteLine($"Controller '{controllerName}' not found.");
            }
        }

        /// <summary>
        /// Capture button input from the selected controller.
        /// </summary>
        public static void CaptureButtonInput()
        {
            if (SelectedJoystick == -1)
            {
                Console.WriteLine("No controller selected.");
                return;
            }

            Console.WriteLine("Listening for button input... Press a button to capture.");
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.StartVisible = false;

            using (var window = new GameWindow(gameWindowSettings, nativeWindowSettings))
            {
                while (!window.IsExiting)
                {
                    GLFW.PollEvents();

                    // Get buttons from the selected joystick
                    var buttons = GLFW.GetJoystickButtons(SelectedJoystick);
                    if (!buttons.IsEmpty) // Check if the ReadOnlySpan is not empty
                    {
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            if ((int)buttons[i] == 1) // Cast to int for comparison
                            {
                                Console.WriteLine($"Button {i} Pressed on Controller '{SelectedDevice}'");
                                // Placeholder for voice activation logic
                                Console.WriteLine("Voice Activation Triggered! (Add your logic here)");
                                return; // Exit after capturing one button press
                            }
                        }
                    }
                }
            }
        }
    }
}
