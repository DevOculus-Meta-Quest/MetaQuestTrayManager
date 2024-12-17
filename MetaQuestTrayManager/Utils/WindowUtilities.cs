using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MetaQuestTrayManager.Utils
{
    /// <summary>
    /// Provides utilities for interacting with external application windows.
    /// </summary>
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

        // Importing User32.dll functions for window management
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
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        /// <summary>
        /// Represents a rectangle structure for window dimensions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// Minimizes an external window using its handle.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to minimize.</param>
        public static void MinimizeExternalWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero && IsWindowVisible(windowHandle))
            {
                ShowWindow(windowHandle, SW_SHOWMINIMIZED);
            }
        }

        /// <summary>
        /// Shows a previously hidden window using its handle.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to show.</param>
        public static void ShowExternalWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                ShowWindow(windowHandle, SW_SHOW);
            }
        }

        /// <summary>
        /// Hides an external window using its handle.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to hide.</param>
        public static void HideExternalWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                ShowWindow(windowHandle, SW_HIDE);
            }
        }

        /// <summary>
        /// Restores a minimized or maximized window to its normal state.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to restore.</param>
        public static void RestoreExternalWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                ShowWindow(windowHandle, SW_RESTORE);
            }
        }

        /// <summary>
        /// Retrieves the title of the currently active foreground window.
        /// </summary>
        /// <returns>The window title as a string, or null if retrieval fails.</returns>
        public static string GetActiveWindowTitle()
        {
            const int bufferSize = 256;
            StringBuilder buffer = new StringBuilder(bufferSize);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buffer, bufferSize) > 0)
            {
                return buffer.ToString();
            }

            return null;
        }

        /// <summary>
        /// Retrieves the text/title of a window by its handle.
        /// </summary>
        /// <param name="windowHandle">The handle of the window.</param>
        /// <returns>The window title as a string, or an empty string if not found.</returns>
        public static string GetWindowTextByHandle(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return string.Empty;

            int textLength = GetWindowTextLength(windowHandle) + 1;
            StringBuilder buffer = new StringBuilder(textLength);

            if (GetWindowText(windowHandle, buffer, textLength) > 0)
            {
                return buffer.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Brings a specified window to the top of the Z-order and sets focus.
        /// </summary>
        /// <param name="windowHandle">The handle of the window to bring to the top.</param>
        public static void BringWindowToTopAndFocus(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;

            BringWindowToTop(windowHandle);
            SetForegroundWindow(windowHandle);
            SetFocus(windowHandle);
        }

        /// <summary>
        /// Attempts to retrieve the rectangle dimensions of a window.
        /// </summary>
        /// <param name="windowHandle">The handle of the window.</param>
        /// <param name="rect">The rectangle structure to store dimensions.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryGetWindowRect(IntPtr windowHandle, ref RECT rect)
        {
            return windowHandle != IntPtr.Zero && GetWindowRect(windowHandle, ref rect);
        }
    }
}
