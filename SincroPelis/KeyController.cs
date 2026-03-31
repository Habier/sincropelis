using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SincroPelis
{
    public static class KeyController
    {
        public enum Shortcut
        {
            PlayPause,
            SeekBack,
            SeekForward,
            Escape,
            VolumeUp,
            VolumeDown
        }

        public static event Action<Shortcut>? OnShortcutPressed;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_SPACE = 0x20;
        private const int VK_LEFT = 0x25;
        private const int VK_RIGHT = 0x27;
        private const int VK_UP = 0x26;
        private const int VK_DOWN = 0x28;
        private const int VK_ESCAPE = 0x1B;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr _hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc? _hookProc;
        private static GCHandle? _hookProcHandle;
        private static bool _started = false;
        private static readonly int _currentProcessId = Process.GetCurrentProcess().Id;

        public static void Start()
        {
            if (_started || _hookId != IntPtr.Zero) return;
            _started = true;

            _hookProc = HookCallback;
            _hookProcHandle = GCHandle.Alloc(_hookProc, GCHandleType.Normal);

            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule != null)
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public static void Stop()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
            if (_hookProcHandle.HasValue)
            {
                _hookProcHandle.Value.Free();
                _hookProcHandle = null;
            }
            _started = false;
        }

        private static bool IsOurWindowActive()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return false;
            GetWindowThreadProcessId(hWnd, out uint pid);
            return pid == _currentProcessId;
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                if (!IsOurWindowActive()) return CallNextHookEx(_hookId, nCode, wParam, lParam);

                int vkCode = Marshal.ReadInt32(lParam);

                Shortcut? shortcut = vkCode switch
                {
                    VK_SPACE => Shortcut.PlayPause,
                    VK_LEFT => Shortcut.SeekBack,
                    VK_RIGHT => Shortcut.SeekForward,
                    VK_ESCAPE => Shortcut.Escape,
                    VK_UP => Shortcut.VolumeUp,
                    VK_DOWN => Shortcut.VolumeDown,
                    _ => null
                };

                if (shortcut.HasValue)
                {
                    OnShortcutPressed?.Invoke(shortcut.Value);
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public static void Log(string msg)
        {
            Logger.Info(msg);
            Program.myForm?.SendDebug(msg);
        }
    }
}
