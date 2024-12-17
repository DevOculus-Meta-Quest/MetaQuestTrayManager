using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MetaQuestTrayManager.Managers
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 256;
        private const int WM_KEYUP = 257;

        private KBDLLHookProc _hookProcDelegate;
        private IntPtr _hookID = IntPtr.Zero;

        public event Action<Keys> KeyDown = delegate { };
        public event Action<Keys> KeyUp = delegate { };

        public KeyboardHook()
        {
            _hookProcDelegate = HookCallback;
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProcDelegate, GetModuleHandle(), 0);

            if (_hookID == IntPtr.Zero)
                throw new Exception("Failed to set global keyboard hook.");
        }

        ~KeyboardHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                if (wParam == (IntPtr)WM_KEYDOWN)
                    KeyDown?.Invoke((Keys)hookStruct.vkCode);
                else if (wParam == (IntPtr)WM_KEYUP)
                    KeyUp?.Invoke((Keys)hookStruct.vkCode);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private IntPtr GetModuleHandle()
        {
            return Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().ManifestModule);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KBDLLHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr KBDLLHookProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
