using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MetaQuestTrayManager.Tests.Managers
{
    public static class MetaQuestControllerTest
    {
        public static void RunTest()
        {
            // Create window settings
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings
            {
                Title = "MetaQuest Controller Test",
                ClientSize = new OpenTK.Mathematics.Vector2i(800, 600), // Updated property
                StartVisible = true
            };

            // Use a GameWindow to test joystick/controller inputs
            using (var window = new GameWindow(gameWindowSettings, nativeWindowSettings))
            {
                Console.WriteLine("=== MetaQuest Controller Test ===");

                // Event: Window Loaded
                window.Load += () =>
                {
                    Console.WriteLine("Window Loaded. Checking for controllers...");
                };

                // Event: Joystick Connected or Disconnected
                window.JoystickConnected += (JoystickEventArgs e) =>
                {
                    if (e.IsConnected)
                    {
                        Console.WriteLine($"Controller {e.JoystickId} connected.");
                    }
                    else
                    {
                        Console.WriteLine($"Controller {e.JoystickId} disconnected.");
                    }
                };

                // Event: Update frame to poll inputs
                window.UpdateFrame += (FrameEventArgs args) =>
                {
                    // Loop through joystick IDs (0 to 15)
                    for (int i = 0; i < 16; i++)
                    {
                        if (GLFW.JoystickPresent(i))
                        {
                            Console.WriteLine($"\nController {i} is connected.");

                            // Get joystick axes
                            var axes = GLFW.GetJoystickAxes(i);
                            if (axes != null)
                            {
                                for (int axisIndex = 0; axisIndex < axes.Length; axisIndex++)
                                {
                                    Console.WriteLine($"Axis {axisIndex}: {axes[axisIndex]:F3}");
                                }
                            }

                            // Get joystick buttons
                            var buttons = GLFW.GetJoystickButtons(i);
                            if (buttons != null)
                            {
                                for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                                {
                                    // Compare with JoystickInputAction.Press
                                    string state = buttons[buttonIndex] == JoystickInputAction.Press ? "Pressed" : "Released";
                                    Console.WriteLine($"Button {buttonIndex}: {state}");
                                }
                            }
                        }
                    }
                };

                // Run the window loop
                window.Run();
            }

            Console.WriteLine("\nTest Complete. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
