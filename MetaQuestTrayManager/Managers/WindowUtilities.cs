using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MetaQuestTrayManager.Managers
{
    public static class WindowUtilities
    {
        // Constants for window states
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        // Importing User32.dll functions
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        // Struct for window rectangle dimensions
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int Length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// Retrieves the dimensions of a window.
        /// </summary>
        public static bool TryGetWindowRect(IntPtr windowHandle, ref RECT rect)
        {
            return windowHandle != IntPtr.Zero && GetWindowRect(windowHandle, ref rect);
        }

        /// <summary>
        /// Minimizes an external window using its handle.
        /// </summary>
        public static void MinimizeWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero && IsWindowVisible(windowHandle))
            {
                ShowWindow(windowHandle, SW_MINIMIZE);
            }
        }

        /// <summary>
        /// Restores an external window to its original size.
        /// </summary>
        public static void RestoreWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                ShowWindow(windowHandle, SW_RESTORE);
                SetForegroundWindow(windowHandle);
            }
        }

        /// <summary>
        /// Hides an external window using its handle.
        /// </summary>
        public static void HideWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
                ShowWindow(hwnd, SW_HIDE);
        }

        /// <summary>
        /// Retrieves the title of the currently active window.
        /// </summary>
        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves the text of a specified window handle.
        /// </summary>
        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1; // Add 1 for safety
            var buff = new StringBuilder(length);

            if (GetWindowText(hWnd, buff, length) > 0)
                return buff.ToString();

            return string.Empty;
        }

        /// <summary>
        /// Brings a window to the top and focuses it.
        /// </summary>
        public static void BringWindowToTopAndFocus(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;

            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);
            SetFocus(hWnd);
        }

        /// <summary>
        /// Sends a specific external window (by process name) to the tray by hiding it.
        /// </summary>
        public static void MinimizeToTray(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero && IsWindowVisible(handle))
                {
                    ShowWindow(handle, SW_HIDE);
                    Debug.WriteLine($"Window '{processName}' minimized to tray.");
                }
            }
        }

        /// <summary>
        /// Restores a minimized window (by process name) from the tray.
        /// </summary>
        public static void RestoreFromTray(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    ShowWindow(handle, SW_RESTORE);
                    SetForegroundWindow(handle);
                    Debug.WriteLine($"Window '{processName}' restored.");
                }
            }
        }
    }
}
