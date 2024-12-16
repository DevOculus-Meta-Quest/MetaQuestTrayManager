using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MetaQuestTrayManager.Controls
{
    /// <summary>
    /// A ToolStripTextBox with support for displaying cue (placeholder) text.
    /// </summary>
    public class CueToolStripTextBox : ToolStripTextBox
    {
        private const uint ECM_FIRST = 5376;
        private const uint EM_SETCUEBANNER = ECM_FIRST + 1;

        private string _cueText = string.Empty;
        private bool _showCueTextWithFocus;

        /// <summary>
        /// Initializes a new instance of the <see cref="CueToolStripTextBox"/> class.
        /// </summary>
        public CueToolStripTextBox()
        {
            if (Control != null)
            {
                Control.HandleCreated += OnControlHandleCreated;
            }
        }

        /// <summary>
        /// Initializes a new instance with the specified name.
        /// </summary>
        public CueToolStripTextBox(string name) : base(name)
        {
            if (Control != null)
            {
                Control.HandleCreated += OnControlHandleCreated;
            }
        }

        /// <summary>
        /// Gets or sets the text to display as a cue (placeholder).
        /// </summary>
        public string CueText
        {
            get => _cueText;
            set
            {
                string newValue = value ?? string.Empty;
                if (_cueText == newValue) return;

                _cueText = newValue;
                UpdateCue();
                CueTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cue text remains visible when the control has focus.
        /// </summary>
        public bool ShowCueTextWithFocus
        {
            get => _showCueTextWithFocus;
            set
            {
                if (_showCueTextWithFocus == value) return;

                _showCueTextWithFocus = value;
                UpdateCue();
                ShowCueTextWithFocusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raised when the CueText property changes.
        /// </summary>
        public event EventHandler? CueTextChanged;

        /// <summary>
        /// Raised when the ShowCueTextWithFocus property changes.
        /// </summary>
        public event EventHandler? ShowCueTextWithFocusChanged;

        /// <summary>
        /// Updates the cue text appearance.
        /// </summary>
        private void UpdateCue()
        {
            if (Control != null && Control.IsHandleCreated)
            {
                SendMessage(new HandleRef(Control, Control.Handle), EM_SETCUEBANNER,
                    _showCueTextWithFocus ? new IntPtr(1) : IntPtr.Zero, _cueText);
            }
        }

        /// <summary>
        /// Called when the control handle is created to set the cue text.
        /// </summary>
        private void OnControlHandleCreated(object? sender, EventArgs e) => UpdateCue();

        /// <summary>
        /// Releases resources used by the control.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && Control != null)
            {
                Control.HandleCreated -= OnControlHandleCreated;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Sends a message to the control.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, string lParam);
    }
}
