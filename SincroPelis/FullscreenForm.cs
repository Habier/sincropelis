using System;
using System.Drawing;
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

        public FullscreenForm(VideoView videoView, Action? onClosed = null)
        {
            _videoView = videoView ?? throw new ArgumentNullException(nameof(videoView));
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
            this.KeyDown += FullscreenForm_KeyDown;
            this.FormClosed += FullscreenForm_FormClosed;
            this.ResumeLayout(false);
        }

        private void FullscreenForm_Load(object? sender, EventArgs e)
        {
            try
            {
                // store original parent and docking
                _originalParent = _videoView.Parent;
                _originalDock = _videoView.Dock;
                _originalBounds = _videoView.Bounds;
                if (_originalParent != null)
                {
                    _originalIndex = _originalParent.Controls.GetChildIndex(_videoView);
                    _originalParent.Controls.Remove(_videoView);
                }

                // add to this form
                _videoView.Dock = DockStyle.Fill;
                _videoView.Parent = this;
                this.Controls.Add(_videoView);
                _videoView.Visible = true;
                try { _videoView.Focus(); } catch { }
            }
            catch { }
        }

        private void FullscreenForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void FullscreenForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            try
            {
                // move videoView back to original parent
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
