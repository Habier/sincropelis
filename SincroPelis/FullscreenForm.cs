using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace SincroPelis
{
    public class FullscreenForm : Form
    {
        private VideoView _videoView;
        private Control? _originalParent;
        private DockStyle _originalDock;
        private Rectangle _originalBounds;
        private int _originalIndex = -1;
        private Action? _onClosedCallback;
        private MediaPlayer? _mediaPlayer;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LEFT = 0x25;
        private const int VK_RIGHT = 0x27;
        private const int VK_SPACE = 0x20;

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
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _hookProc;

        public FullscreenForm(VideoView videoView, MediaPlayer? mediaPlayer, Action? onClosed = null)
        {
            _videoView = videoView ?? throw new ArgumentNullException(nameof(videoView));
            _mediaPlayer = mediaPlayer;
            _onClosedCallback = onClosed;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = System.Drawing.Color.Black;
            this.KeyPreview = true;
            this.TopMost = true;

            this.Load += FullscreenForm_Load;
            this.FormClosed += FullscreenForm_FormClosed;
            this.ResumeLayout(false);
        }

        private void FullscreenForm_Load(object? sender, EventArgs e)
        {
            try
            {
                _originalParent = _videoView.Parent;
                _originalDock = _videoView.Dock;
                _originalBounds = _videoView.Bounds;
                if (_originalParent != null)
                {
                    _originalIndex = _originalParent.Controls.GetChildIndex(_videoView);
                    _originalParent.Controls.Remove(_videoView);
                }

                _videoView.Dock = DockStyle.Fill;
                _videoView.Parent = this;
                this.Controls.Add(_videoView);
                _videoView.Visible = true;
                this.Focus();
                this.BringToFront();
                this.Activate();

                _hookProc = HookCallback;
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    if (curModule != null)
                        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            catch { }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == VK_SPACE || vkCode == VK_LEFT || vkCode == VK_RIGHT || vkCode == 0x1B) // Space, Left, Right, Escape
                {
                    try
                    {
                        if (vkCode == VK_SPACE)
                        {
                            if (_mediaPlayer != null)
                            {
                                if (_mediaPlayer.IsPlaying)
                                {
                                    _mediaPlayer.Pause();
                                    Program.client.TrySend("pause");
                                }
                                else
                                {
                                    _mediaPlayer.Play();
                                    Program.client.TrySend("play");
                                }
                            }
                            return (IntPtr)1;
                        }
                        else if (vkCode == VK_LEFT)
                        {
                            var newTime = Math.Max(0, _mediaPlayer!.Time - 5000);
                            _mediaPlayer.Time = (long)newTime;
                            Program.client.TrySend("seekby:-5");
                            return (IntPtr)1;
                        }
                        else if (vkCode == VK_RIGHT)
                        {
                            var newTime = Math.Min(_mediaPlayer!.Length, _mediaPlayer.Time + 5000);
                            _mediaPlayer.Time = (long)newTime;
                            Program.client.TrySend("seekby:5");
                            return (IntPtr)1;
                        }
                        else if (vkCode == 0x1B) // Escape
                        {
                            this.BeginInvoke(new Action(() => this.Close()));
                            return (IntPtr)1;
                        }
                    }
                    catch { }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void FullscreenForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }

            try
            {
                this.Controls.Remove(_videoView);
                if (_originalParent != null)
                {
                    _videoView.Dock = _originalDock;
                    _videoView.Bounds = _originalBounds;
                    _originalParent.Controls.Add(_videoView);
                    if (_originalIndex >= 0)
                        _originalParent.Controls.SetChildIndex(_videoView, _originalIndex);
                }

                _onClosedCallback?.Invoke();
            }
            catch { }
        }
    }
}
