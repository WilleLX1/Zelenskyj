using System.Media;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Zelenskyj
{
    public partial class Form1 : Form
    {
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly Random _rand = new();
        private Rectangle _bounds;          // Desktop bounds across all monitors
        private float _vx, _vy;             // Velocity (pixels per tick)
        private float _speed = 0.40f; // 40% of current speed (try 0.2f for very slow)

        // Sound
        private SoundPlayer _boopPlayer;
        private DateTime _lastBoop = DateTime.MinValue;
        private readonly TimeSpan _boopCooldown = TimeSpan.FromMilliseconds(120);
        private AudioEnforcer _audio;

        public Form1()
        {
            InitializeComponent();
            
            this.FormClosing += Form1_FormClosing;
            
            try
            {
                _boopPlayer = new SoundPlayer(@"hello-biden-its-zelensky.wav"); // put next to your .exe
                _boopPlayer.LoadAsync();                    // non-blocking
            }
            catch
            {
                // Fallback will use SystemSounds
            }

            _audio = new AudioEnforcer();
            _audio.Start();

            // Make the window look like a floating image
            this.FormBorderStyle = FormBorderStyle.None; // borderless
            this.ControlBox = false;
            this.ShowIcon = false;
            this.TopMost = true;
            this.KeyPreview = true;                      // allow hotkey on form
            this.BackColor = Color.Magenta;              // any rare color
            this.TransparencyKey = Color.Magenta;        // make it transparent

            // Size window exactly to the picture and put picture at (0,0)
            this.ClientSize = ZelenskyjBox.Size;
            ZelenskyjBox.Location = Point.Empty;

            // Movement setup
            _bounds = SystemInformation.VirtualScreen;   // all monitors

            // Start somewhere random within bounds
            var startX = _rand.Next(_bounds.Left, _bounds.Right - this.Width);
            var startY = _rand.Next(_bounds.Top, _bounds.Bottom - this.Height);
            this.Location = new Point(startX, startY);

            // Random velocity (both direction and speed)
            _vx = (_rand.Next(3, 8)) * (_rand.Next(2) == 0 ? -1 : 1);
            _vy = (_rand.Next(3, 8)) * (_rand.Next(2) == 0 ? -1 : 1);

            // 60 FPS-ish
            _timer.Interval = 16;
            _timer.Tick += (_, __) => Step();
            _timer.Start();

            // Optional: hide from taskbar (still Alt-Tab visible); if you also want to hide from Alt-Tab,
            // make it a tool window: ShowInTaskbar = false; and set FormBorderStyle.None (already set)
            this.ShowInTaskbar = false;

            // Nice touch: prevent resizing grip
            this.SizeGripStyle = SizeGripStyle.Hide;
        }

        private void Step()
        {
            bool bounced = false;

            float nextX = this.Left + _vx * _speed;
            float nextY = this.Top + _vy * _speed;

            // Bounce horizontally
            if (nextX < _bounds.Left)
            {
                nextX = _bounds.Left;
                _vx = Math.Abs(_vx);
                bounced = true;
            }
            else if (nextX + this.Width > _bounds.Right)
            {
                nextX = _bounds.Right - this.Width;
                _vx = -Math.Abs(_vx);
                bounced = true;
            }

            // Bounce vertically
            if (nextY < _bounds.Top)
            {
                nextY = _bounds.Top;
                _vy = Math.Abs(_vy);
                bounced = true;
            }
            else if (nextY + this.Height > _bounds.Bottom)
            {
                nextY = _bounds.Bottom - this.Height;
                _vy = -Math.Abs(_vy);
                bounced = true;
            }

            this.Location = new Point((int)nextX, (int)nextY);

            if (bounced) PlayBounce();

            // Occasionally spice up movement: small random nudge
            if (_rand.Next(0, 500) == 0)
            {
                _vx += _rand.Next(-1, 2);
                _vy += _rand.Next(-1, 2);
                _vx = Math.Max(-10, Math.Min(10, _vx));
                _vy = Math.Max(-10, Math.Min(10, _vy));
            }
        }

        private void PlayBounce()
        {
            // Throttle so overlapping hits don't spam audio
            if (DateTime.UtcNow - _lastBoop < _boopCooldown) return;
            _lastBoop = DateTime.UtcNow;

            if (_boopPlayer != null)
                _boopPlayer.Play();           // async, non-blocking
            else
                SystemSounds.Asterisk.Play();  // fallback beep
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Block all close attempts unless we set a flag (we won't here)
            e.Cancel = true;
        }

        // Kill switch: Ctrl+Shift+X to exit cleanly
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Kill switch (you already have this)
            if (keyData == (Keys.Control | Keys.Shift | Keys.X))
            {
                _timer.Stop();
                _audio?.Dispose();
                this.FormClosing -= Form1_FormClosing;
                Close();
                return true;
            }

            // Speed down: Ctrl+Shift+Minus
            if (keyData == (Keys.Control | Keys.Shift | Keys.OemMinus))
            {
                _speed = Math.Max(0.05f, _speed - 0.05f); // floor at 5%
                return true;
            }

            // Speed up: Ctrl+Shift+Plus
            if (keyData == (Keys.Control | Keys.Shift | Keys.Oemplus))
            {
                _speed = Math.Min(3.0f, _speed + 0.05f); // cap at 300%
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Nothing needed here
        }
    }
}
