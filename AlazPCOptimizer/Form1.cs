using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
namespace AlazPCOptimizer
{
    internal static class Gfx
    {
        public static GraphicsPath RoundRect(RectangleF r, float radius)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 0f)
            {
                p.AddRectangle(r);
                return p;
            }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
    internal sealed class Theme
    {
        public Color Bg, Card, Text, Sub, Accent, Accent2, Btn, BtnHover, BtnPressed, Border, Track, LogBg, LogFore;

        public static readonly Theme Dark = new Theme
        {
            Bg = Color.FromArgb(18, 20, 28),
            Card = Color.FromArgb(28, 32, 44),
            Text = Color.FromArgb(235, 238, 245),
            Sub = Color.FromArgb(140, 148, 165),
            Accent = Color.FromArgb(99, 102, 241),
            Accent2 = Color.FromArgb(56, 189, 248),
            Btn = Color.FromArgb(34, 39, 54),
            BtnHover = Color.FromArgb(46, 53, 74),
            BtnPressed = Color.FromArgb(26, 30, 42),
            Border = Color.FromArgb(52, 58, 78),
            Track = Color.FromArgb(36, 41, 56),
            LogBg = Color.FromArgb(12, 14, 20),
            LogFore = Color.FromArgb(57, 255, 120)
        };

        public static readonly Theme Light = new Theme
        {
            Bg = Color.FromArgb(240, 243, 250),
            Card = Color.FromArgb(255, 255, 255),
            Text = Color.FromArgb(25, 30, 45),
            Sub = Color.FromArgb(100, 110, 130),
            Accent = Color.FromArgb(99, 102, 241),
            Accent2 = Color.FromArgb(56, 189, 248),
            Btn = Color.FromArgb(255, 255, 255),
            BtnHover = Color.FromArgb(232, 236, 248),
            BtnPressed = Color.FromArgb(220, 225, 242),
            Border = Color.FromArgb(210, 216, 232),
            Track = Color.FromArgb(218, 224, 238),
            LogBg = Color.FromArgb(246, 251, 247),
            LogFore = Color.FromArgb(0, 128, 56)
        };
    }
    internal sealed class RoundButton : Button
    {
        public int Radius = 18;
        public Color Normal, Hover, Pressed, Border, Fore, Accent1, Accent2;
        public bool UseAccent;
        public string Icon = "";
        public string SubText = "";   // ana metnin altında küçük yazı
        private static readonly Font SubFont = new Font("Segoe UI", 8.5F);

        private static readonly Font IconFont = new Font("Segoe UI Emoji", 20f);
        private bool hot;
        private bool down;

        public RoundButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Fore = Color.White;
            Normal = Color.Gray;
            Hover = Color.DarkGray;
            Pressed = Color.DimGray;
            Border = Color.Empty;
            Accent1 = Color.RoyalBlue;
            Accent2 = Color.DeepSkyBlue;
        }

        protected override bool ShowFocusCues { get { return false; } }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color parentBg = Parent != null ? Parent.BackColor : BackColor;
            g.Clear(parentBg);

            if (Width < 4 || Height < 4) return;

            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath path = Gfx.RoundRect(rect, Radius))
            {
                if (UseAccent)
                {
                    using (LinearGradientBrush br = new LinearGradientBrush(ClientRectangle, Accent1, Accent2, LinearGradientMode.Horizontal))
                    {
                        g.FillPath(br, path);
                    }
                    if (down)
                    {
                        using (SolidBrush ov = new SolidBrush(Color.FromArgb(45, Color.Black))) g.FillPath(ov, path);
                    }
                    else if (hot)
                    {
                        using (SolidBrush ov = new SolidBrush(Color.FromArgb(35, Color.White))) g.FillPath(ov, path);
                    }
                }
                else
                {
                    Color c = down ? Pressed : (hot ? Hover : Normal);
                    using (SolidBrush br = new SolidBrush(c)) g.FillPath(br, path);
                    if (Border != Color.Empty)
                    {
                        using (Pen pen = new Pen(Border, 1f)) g.DrawPath(pen, path);
                    }
                }

                Color textColor = UseAccent ? Color.White : Fore;
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                                        TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

                if (!string.IsNullOrEmpty(Icon))
                {
                    Rectangle iconRect = new Rectangle(0, 8, Width, (int)(Height * 0.48));
                    Rectangle textRect = new Rectangle(8, (int)(Height * 0.52), Width - 16, (int)(Height * 0.44));
                    TextRenderer.DrawText(g, Icon, IconFont, iconRect, textColor, flags);
                    TextRenderer.DrawText(g, Text, Font, textRect, textColor, flags);
                }
                else if (!string.IsNullOrEmpty(SubText))
                {
                    int mainH = (int)(Height * 0.60f);
                    TextRenderer.DrawText(g, Text, Font, new Rectangle(8, (int)(Height * 0.04f), Width - 16, mainH), textColor, flags);
                    Color subColor = UseAccent ? Color.FromArgb(222, 228, 255) : Color.FromArgb(Fore.R * 3 / 4 + 40, Fore.G * 3 / 4 + 40, Fore.B * 3 / 4 + 40);
                    TextRenderer.DrawText(g, SubText, SubFont, new Rectangle(8, (int)(Height * 0.04f) + mainH - 2, Width - 16, (int)(Height * 0.30f)), subColor, flags);
                }
                else
                {
                    TextRenderer.DrawText(g, Text, Font, new Rectangle(8, 0, Width - 16, Height), textColor, flags);
                }

                if (!Enabled)
                {
                    using (SolidBrush dim = new SolidBrush(Color.FromArgb(140, parentBg))) g.FillPath(dim, path);
                }
            }
        }
    }
    internal sealed class RoundPanel : Panel
    {
        public int Radius = 20;

        public RoundPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : SystemColors.Control);
            if (Width < 4 || Height < 4) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width - 1, Height - 1), Radius))
            using (SolidBrush br = new SolidBrush(BackColor))
            {
                g.FillPath(br, p);
            }
        }
    }

 
    internal sealed class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
        }
    }


    internal sealed class IconTile : Control
    {
        public Color Fill = Color.Gray;
        public Bitmap Picture;
        public string Letter = "?";
        public int Radius = 14;

        private static readonly Font LetterFont = new Font("Segoe UI", 15F, FontStyle.Bold);

        public IconTile()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            if (Width < 4 || Height < 4) return;

            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width - 1, Height - 1), Radius))
            using (SolidBrush br = new SolidBrush(Fill))
            {
                g.FillPath(br, p);
            }

            if (Picture != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                int s = (int)(Math.Min(Width, Height) * 0.72f);
                g.DrawImage(Picture, new Rectangle((Width - s) / 2, (Height - s) / 2, s, s));
            }
            else
            {
                TextRenderer.DrawText(g, Letter, LetterFont, ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }


    internal sealed class RoundScrollBar : Control
    {
        public Color TrackColor = Color.Gray;
        public Color ThumbColor = Color.DimGray;
        public Color HotColor = Color.RoyalBlue;

        public event EventHandler Scrolled;

        private int total;
        private int page = 1;
        private int pos;
        private bool hot;
        private bool dragging;
        private int dragOffset;

        public RoundScrollBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        public bool IsDragging { get { return dragging; } }
        public int Value { get { return pos; } }
        public int MaxValue { get { return Math.Max(0, total - page); } }
        private bool CanScroll { get { return total > page && Height > 0; } }


        public void SetState(int totalSize, int pageSize, int val)
        {
            int nt = Math.Max(0, totalSize);
            int np = Math.Max(1, pageSize);
            int maxv = Math.Max(0, nt - np);
            int nv = Math.Max(0, Math.Min(maxv, val));
            if (nt == total && np == page && nv == pos) return;
            total = nt;
            page = np;
            pos = nv;
            Invalidate();
        }

        private Rectangle ThumbRect()
        {
            int h = Height;
            if (!CanScroll) return new Rectangle(0, 0, Width, h);
            int th = (int)((long)h * page / total);
            th = Math.Max(th, Width * 3);
            th = Math.Min(th, h);
            int travel = h - th;
            int y = MaxValue == 0 ? 0 : (int)((long)travel * pos / MaxValue);
            return new Rectangle(0, y, Width, th);
        }

        private void MoveTo(int v)
        {
            v = Math.Max(0, Math.Min(MaxValue, v));
            if (v == pos) return;
            pos = v;
            Invalidate();
            EventHandler h = Scrolled;
            if (h != null) h(this, EventArgs.Empty);
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !CanScroll) return;
            Rectangle th = ThumbRect();
            if (th.Contains(e.Location))
            {
                dragging = true;
                dragOffset = e.Y - th.Y;
                Capture = true;
                Invalidate();
            }
            else
            {
                MoveTo(pos + (e.Y < th.Y ? -page : page));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!dragging) return;
            Rectangle th = ThumbRect();
            int travel = Height - th.Height;
            if (travel <= 0) return;
            int y = Math.Max(0, Math.Min(travel, e.Y - dragOffset));
            MoveTo((int)Math.Round((double)y * MaxValue / travel));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (dragging)
            {
                dragging = false;
                Capture = false;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            if (!CanScroll || Width < 4) return;

            int tw = Math.Max(2, Width / 3);
            using (GraphicsPath tp = Gfx.RoundRect(new RectangleF((Width - tw) / 2f, 0, tw, Height), tw / 2f))
            using (SolidBrush tb = new SolidBrush(TrackColor))
            {
                g.FillPath(tb, tp);
            }

            Rectangle th = ThumbRect();
            int pad = Math.Max(2, Width / 4);
            float w = Width - pad * 2f;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(pad, th.Y, w, th.Height), w / 2f))
            using (SolidBrush br = new SolidBrush((hot || dragging) ? HotColor : ThumbColor))
            {
                g.FillPath(br, p);
            }
        }
    }


    internal sealed class SmoothBar : Control
    {
        public Color Track = Color.Gray;
        public Color Fill1 = Color.RoyalBlue;
        public Color Fill2 = Color.DeepSkyBlue;
        public Color Fore = Color.White;

        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private float shown;
        private float target;

        public SmoothBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            timer.Interval = 15;
            timer.Tick += OnTick;
            timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            float diff = target - shown;
            if (diff > 0.001f)
            {
                float step = diff * 0.07f + 0.35f;
                if (step >= diff) shown = target;
                else shown += step;
                Invalidate();
            }
        }

     
        public void Reset()
        {
            target = 0f;
            shown = 0f;
            Invalidate();
        }

        public void SetTarget(int t)
        {
            target = Math.Max(0, Math.Min(100, t));
        }

    
        public async Task AnimateToAsync(int t)
        {
            SetTarget(t);
            while (shown < target - 0.01f)
            {
                await Task.Delay(15);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) timer.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            if (Width < 6 || Height < 6) return;

            using (GraphicsPath tp = Gfx.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f), Height / 2f))
            using (SolidBrush tb = new SolidBrush(Track))
            {
                g.FillPath(tb, tp);
            }

            float w = (Width - 2f) * shown / 100f;
            if (w >= 2f)
            {
                using (GraphicsPath fp = Gfx.RoundRect(new RectangleF(1f, 1f, w, Height - 3f), (Height - 3f) / 2f))
                using (LinearGradientBrush fb = new LinearGradientBrush(new RectangleF(1f, 1f, Width - 2f, Height - 3f), Fill1, Fill2, LinearGradientMode.Horizontal))
                {
                    g.FillPath(fb, fp);
                }
            }

            string txt = ((int)Math.Round(shown)).ToString(CultureInfo.InvariantCulture) + "%";
            TextRenderer.DrawText(g, txt, Font, ClientRectangle, shown > 50f ? Color.White : Fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }


    internal sealed class RoundSelect : Control
    {
        public string[] Items = new string[0];
        public int Radius = 17;
        public Color Normal, Hover, Border, Fore, PopupBg, ItemHover, Accent1, Accent2;

        public event EventHandler SelectedIndexChanged;

        private int selected;
        private bool hot;
        private SelectPopup popup;
        private DateTime closedAt = DateTime.MinValue;

        public RoundSelect()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Normal = Color.Gray;
            Hover = Color.DarkGray;
            Border = Color.DimGray;
            Fore = Color.White;
            PopupBg = Color.DimGray;
            ItemHover = Color.Gray;
            Accent1 = Color.RoyalBlue;
            Accent2 = Color.DeepSkyBlue;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get { return selected; }
            set
            {
                if (value < 0 || value >= Items.Length || value == selected) return;
                selected = value;
                Invalidate();
                EventHandler h = SelectedIndexChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            OpenPopup();
        }

        private void OpenPopup()
        {
            if ((DateTime.UtcNow - closedAt).TotalMilliseconds < 300) return;
            if (popup != null && !popup.IsDisposed)
            {
                popup.Close();
                return;
            }

            popup = new SelectPopup(this);
            popup.FormClosed += delegate
            {
                closedAt = DateTime.UtcNow;
                popup = null;
                Invalidate();
            };
            Point loc = PointToScreen(new Point(0, Height + 4));
            Rectangle wa = Screen.FromControl(this).WorkingArea;
            if (loc.Y + popup.Height > wa.Bottom - 8)
                loc.Y = Math.Max(wa.Top + 8, wa.Bottom - 8 - popup.Height);
            popup.Location = loc;
            popup.Show(FindForm());
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            if (Width < 20 || Height < 10) return;

            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f), Radius))
            {
                using (SolidBrush br = new SolidBrush(hot ? Hover : Normal)) g.FillPath(br, p);
                using (Pen pen = new Pen(popup != null ? Accent1 : Border, 1f)) g.DrawPath(pen, p);
            }

            string txt = "🌐  " + (selected < Items.Length ? Items[selected] : "");
            TextRenderer.DrawText(g, txt, Font, new Rectangle(14, 0, Width - 44, Height), Fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            using (Pen cp = new Pen(Fore, 2f))
            {
                cp.StartCap = LineCap.Round;
                cp.EndCap = LineCap.Round;
                cp.LineJoin = LineJoin.Round;
                float cy = Height / 2f;
                g.DrawLines(cp, new PointF[]
                {
                    new PointF(Width - 28f, cy - 2.5f),
                    new PointF(Width - 22f, cy + 3f),
                    new PointF(Width - 16f, cy - 2.5f)
                });
            }
        }
    }

    internal sealed class SelectPopup : Form
    {
        private readonly RoundSelect host;

        public SelectPopup(RoundSelect owner)
        {
            host = owner;
            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            KeyPreview = true;
            BackColor = host.PopupBg;

            int n = host.Items.Length;
            int ih = Math.Min(host.Height, 30);
            int gap = 3;
            int pad = Math.Max(4, host.Height / 6);
            Size = new Size(host.Width, pad * 2 + n * ih + (n - 1) * gap);

            for (int i = 0; i < n; i++)
            {
                RoundButton b = new RoundButton();
                b.SetBounds(pad, pad + i * (ih + gap), host.Width - pad * 2, ih);
                b.Radius = Math.Max(6, ih / 2 - 1);
                b.Text = host.Items[i];
                b.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                b.Normal = host.PopupBg;
                b.Hover = host.ItemHover;
                b.Pressed = host.ItemHover;
                b.Border = Color.Empty;
                b.Fore = host.Fore;
                if (i == host.SelectedIndex)
                {
                    b.UseAccent = true;
                    b.Accent1 = host.Accent1;
                    b.Accent2 = host.Accent2;
                }
                int idx = i;
                b.Click += delegate
                {
                    host.SelectedIndex = idx;
                    Close();
                };
                Controls.Add(b);
            }

            Deactivate += delegate { if (!IsDisposed) Close(); };
            KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            Resize += delegate { ApplyRegion(); };
            Load += delegate { ApplyRegion(); };
        }

        private void ApplyRegion()
        {
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), host.Radius))
            {
                Region = new Region(p);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f), host.Radius))
            using (Pen pen = new Pen(host.Border, 1f))
            {
                e.Graphics.DrawPath(pen, p);
            }
        }
    }

    internal sealed class AlertForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private readonly Theme t;
        private readonly float k;

        private int S(int v) { return (int)Math.Round(v * k); }

        public static bool Ask(IWin32Window owner, Theme theme, string title, string body, string okText, string cancelText)
        {
            using (AlertForm f = new AlertForm(theme, title, body, okText, cancelText))
            {
                return f.ShowDialog(owner) == DialogResult.OK;
            }
        }

        private AlertForm(Theme theme, string title, string body, string okText, string cancelText)
        {
            t = theme;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                k = g.DpiX / 96f;
            }

            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            BackColor = t.Bg;

            Color danger = Color.FromArgb(239, 68, 68);
            int width = S(480);

            IconTile icon = new IconTile();
            icon.SetBounds(S(26), S(26), S(52), S(52));
            icon.Radius = S(16);
            icon.Fill = danger;
            icon.Letter = "!";

            Label lblT = new Label();
            lblT.AutoSize = false;
            lblT.UseMnemonic = false;
            lblT.SetBounds(S(94), S(24), S(360), S(32));
            lblT.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblT.ForeColor = danger;
            lblT.Text = title;
            lblT.MouseDown += Drag;

            Font bodyFont = new Font("Segoe UI", 10F);
            Size sz = TextRenderer.MeasureText(body, bodyFont, new Size(S(358), 4000),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            Label lblB = new Label();
            lblB.AutoSize = false;
            lblB.UseMnemonic = false;
            lblB.SetBounds(S(94), S(62), S(358), sz.Height + S(12));
            lblB.Font = bodyFont;
            lblB.ForeColor = t.Text;
            lblB.Text = body;
            lblB.MouseDown += Drag;

            int btnY = S(62) + lblB.Height + S(22);

            RoundButton okBtn = new RoundButton();
            okBtn.SetBounds(width - S(26) - S(150), btnY, S(150), S(40));
            okBtn.Radius = S(20);
            okBtn.Text = okText;
            okBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            okBtn.Normal = danger;
            okBtn.Hover = Color.FromArgb(248, 113, 113);
            okBtn.Pressed = Color.FromArgb(220, 38, 38);
            okBtn.Fore = Color.White;
            okBtn.Border = Color.Empty;
            okBtn.DialogResult = DialogResult.OK;

            RoundButton cancelBtn = new RoundButton();
            cancelBtn.SetBounds(okBtn.Left - S(12) - S(130), btnY, S(130), S(40));
            cancelBtn.Radius = S(20);
            cancelBtn.Text = cancelText;
            cancelBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            cancelBtn.Normal = t.Btn;
            cancelBtn.Hover = t.BtnHover;
            cancelBtn.Pressed = t.BtnPressed;
            cancelBtn.Border = t.Border;
            cancelBtn.Fore = t.Text;
            cancelBtn.DialogResult = DialogResult.Cancel;

            Controls.Add(icon);
            Controls.Add(lblT);
            Controls.Add(lblB);
            Controls.Add(okBtn);
            Controls.Add(cancelBtn);

            CancelButton = cancelBtn;
            ClientSize = new Size(width, btnY + S(40) + S(26));

            MouseDown += Drag;
            Resize += delegate { ApplyRegion(); };
            Load += delegate { ApplyRegion(); };
        }

        private void ApplyRegion()
        {
            if (Width < 10 || Height < 10) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), S(26)))
            {
                Region = new Region(p);
            }
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f), S(26)))
            using (Pen pen = new Pen(Color.FromArgb(239, 68, 68), 1.5f))
            {
                e.Graphics.DrawPath(pen, p);
            }
        }
    }

    internal sealed class StartupEntry
    {
        public string Name = "";
        public string ValueName = "";
        public string Command = "";
        public string SourceFile = "";
        public string Tag = "";
        public RegistryHive HiveId;
        public string ApprovedPath = "";
        public bool Enabled = true;
        public Bitmap IconBmp;

        private const string RunBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RunWow = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run";
        private const string ApprovedBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\";
        private static readonly string[] Exts = { ".exe", ".bat", ".cmd", ".com", ".msc", ".lnk", ".scr" };

        public static List<StartupEntry> LoadAll()
        {
            List<StartupEntry> list = new List<StartupEntry>();
            CollectRun(list, RegistryHive.CurrentUser, "HKCU Run", RunBase, ApprovedBase + "Run");
            CollectRun(list, RegistryHive.LocalMachine, "HKLM Run", RunBase, ApprovedBase + "Run");
            CollectRun(list, RegistryHive.LocalMachine, "HKLM Run x86", RunWow, ApprovedBase + "Run32");
            CollectFolder(list, Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                RegistryHive.CurrentUser, "Startup folder");
            CollectFolder(list, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),
                RegistryHive.LocalMachine, "Startup folder (all)");

            list.Sort(delegate (StartupEntry a, StartupEntry b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
            return list;
        }

        private static bool IsEnabledState(byte[] state)
        {
            return !(state != null && state.Length > 0 && (state[0] & 1) == 1);
        }

        private static void CollectRun(List<StartupEntry> list, RegistryHive hive, string tag, string runPath, string approvedPath)
        {
            try
            {
                using (RegistryKey bk = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
                using (RegistryKey rk = bk.OpenSubKey(runPath))
                using (RegistryKey ak = bk.OpenSubKey(approvedPath))
                {
                    if (rk == null) return;
                    foreach (string name in rk.GetValueNames())
                    {
                        if (string.IsNullOrEmpty(name)) continue;

                        string cmd = Convert.ToString(rk.GetValue(name));
                        if (cmd == null) cmd = "";

                        byte[] state = ak != null ? ak.GetValue(name) as byte[] : null;

                        StartupEntry en = new StartupEntry();
                        en.Name = name;
                        en.ValueName = name;
                        en.Command = cmd;
                        en.Tag = tag;
                        en.HiveId = hive;
                        en.ApprovedPath = approvedPath;
                        en.Enabled = IsEnabledState(state);
                        list.Add(en);
                    }
                }
            }
            catch { }
        }

        private static void CollectFolder(List<StartupEntry> list, string folder, RegistryHive hive, string tag)
        {
            try
            {
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;

                string approvedPath = ApprovedBase + "StartupFolder";
                using (RegistryKey bk = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
                using (RegistryKey ak = bk.OpenSubKey(approvedPath))
                {
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        string fn = Path.GetFileName(file);
                        if (string.Equals(fn, "desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;

                        string target = file;
                        if (string.Equals(Path.GetExtension(file), ".lnk", StringComparison.OrdinalIgnoreCase))
                        {
                            target = ResolveShortcut(file);
                        }

                        byte[] state = ak != null ? ak.GetValue(fn) as byte[] : null;

                        StartupEntry en = new StartupEntry();
                        en.Name = Path.GetFileNameWithoutExtension(file);
                        en.ValueName = fn;
                        en.Command = target;
                        en.SourceFile = file;
                        en.Tag = tag;
                        en.HiveId = hive;
                        en.ApprovedPath = approvedPath;
                        en.Enabled = IsEnabledState(state);
                        list.Add(en);
                    }
                }
            }
            catch { }
        }

        private static string ResolveShortcut(string lnk)
        {
            object sh = null;
            object sc = null;
            try
            {
                Type t = Type.GetTypeFromProgID("WScript.Shell");
                if (t == null) return lnk;
                sh = Activator.CreateInstance(t);
                sc = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, sh, new object[] { lnk });
                object target = sc.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, sc, null);
                string s = target as string;
                return string.IsNullOrEmpty(s) ? lnk : s;
            }
            catch { return lnk; }
            finally
            {
                try { if (sc != null) Marshal.ReleaseComObject(sc); } catch { }
                try { if (sh != null) Marshal.ReleaseComObject(sh); } catch { }
            }
        }

   
        public static string ParseExePath(string cmd)
        {
            if (string.IsNullOrEmpty(cmd) || cmd.Trim().Length == 0) return "";
            string c = Environment.ExpandEnvironmentVariables(cmd.Trim());
            string path;

            if (c.StartsWith("\""))
            {
                int q = c.IndexOf('"', 1);
                path = q > 1 ? c.Substring(1, q - 1) : c.Trim('"');
            }
            else
            {
                string lower = c.ToLowerInvariant();
                int best = -1;
                foreach (string ext in Exts)
                {
                    int i = lower.IndexOf(ext, StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        int end = i + ext.Length;
                        if (best < 0 || end < best) best = end;
                    }
                }
                if (best > 0) path = c.Substring(0, best);
                else path = c.Split(' ')[0];
            }

            path = path.Trim();
            if (path.Length > 0 && !File.Exists(path) && !Path.IsPathRooted(path))
            {
                path = FindOnPath(path);
            }
            return path;
        }

        private static string FindOnPath(string name)
        {
            try
            {
                string n = name;
                if (Path.GetExtension(n).Length == 0) n += ".exe";

                string p0 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), n);
                if (File.Exists(p0)) return p0;

                string env = Environment.GetEnvironmentVariable("PATH");
                if (env != null)
                {
                    foreach (string dir in env.Split(';'))
                    {
                        if (dir.Length == 0) continue;
                        try
                        {
                            string p = Path.Combine(dir.Trim().Trim('"'), n);
                            if (File.Exists(p)) return p;
                        }
                        catch { }
                    }
                }
            }
            catch { }
            return name;
        }
        public Bitmap LoadIcon()
        {
            try
            {
                string p = ParseExePath(Command);
                if ((p.Length == 0 || !File.Exists(p)) && SourceFile.Length > 0) p = SourceFile;
                if (p.Length == 0 || !File.Exists(p)) return null;

                using (System.Drawing.Icon ic = System.Drawing.Icon.ExtractAssociatedIcon(p))
                {
                    if (ic == null) return null;
                    return ic.ToBitmap();
                }
            }
            catch { return null; }
        }
        public string SetEnabled(bool enable)
        {
            try
            {
                using (RegistryKey bk = RegistryKey.OpenBaseKey(HiveId, RegistryView.Registry64))
                using (RegistryKey ak = bk.CreateSubKey(ApprovedPath))
                {
                    byte[] data = new byte[12];
                    if (enable)
                    {
                        data[0] = 2;
                    }
                    else
                    {
                        data[0] = 3;
                        BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc()).CopyTo(data, 4);
                    }
                    ak.SetValue(ValueName, data, RegistryValueKind.Binary);
                }
                Enabled = enable;
                return null;
            }
            catch (UnauthorizedAccessException) { return "ADMIN"; }
            catch (System.Security.SecurityException) { return "ADMIN"; }
            catch (Exception ex) { return ex.Message; }
        }
    }
    internal sealed class StartupForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private readonly Theme t;
        private readonly Func<string, object[], string> tr;
        private readonly Action<string> log;
        private readonly List<StartupEntry> entries;
        private readonly float k;                       // DPI ölçeği

        private int filter;                             // 0 = hepsi, 1 = aktif, 2 = devre dışı
        private int offset;                             // kaydırma konumu (px)
        private int contentH;                           // liste içerik yüksekliği (px)

        private readonly Label lblTitle = new Label();
        private readonly Label lblInfo = new Label();
        private readonly RoundButton closeX = new RoundButton();
        private readonly RoundButton closeBtn = new RoundButton();
        private readonly RoundButton[] chips = new RoundButton[3];
        private readonly BufferedPanel viewport = new BufferedPanel();
        private readonly BufferedPanel content = new BufferedPanel();
        private readonly RoundScrollBar scroll = new RoundScrollBar();
        private readonly ToolTip tip = new ToolTip();
        private readonly List<Bitmap> icons = new List<Bitmap>();

        private static readonly Color[] Palette =
        {
            Color.FromArgb(99, 102, 241), Color.FromArgb(14, 165, 233), Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11), Color.FromArgb(244, 63, 94), Color.FromArgb(168, 85, 247)
        };

        private int S(int v) { return (int)Math.Round(v * k); }

        public StartupForm(Theme theme, Func<string, object[], string> translate, Action<string> logger)
        {
            t = theme;
            tr = translate;
            log = logger;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                k = g.DpiX / 96f;
            }

            Cursor prev = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            entries = StartupEntry.LoadAll();
            foreach (StartupEntry en in entries)
            {
                Bitmap bmp = en.LoadIcon();
                en.IconBmp = bmp;
                if (bmp != null) icons.Add(bmp);
            }
            Cursor.Current = prev;

            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;   // Alaz logosu (görev çubuğu / Alt-Tab)
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(S(660), S(600));
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            BackColor = t.Bg;
            KeyPreview = true;

            BuildUi();

            KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            Resize += delegate { ApplyRegion(); };
            Load += delegate { ApplyRegion(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tip.Dispose();
                foreach (Bitmap b in icons) b.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ApplyRegion()
        {
            if (Width < 10 || Height < 10) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), S(26)))
            {
                Region = new Region(p);
            }
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetOffset(offset - e.Delta * S(70) / 120);
        }

        private void StyleBtn(RoundButton b)
        {
            b.Normal = t.Btn;
            b.Hover = t.BtnHover;
            b.Pressed = t.BtnPressed;
            b.Border = t.Border;
            b.Fore = t.Text;
        }

        private void BuildUi()
        {
            SuspendLayout();

            lblTitle.Text = "📋 " + tr("startup", null);
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = t.Text;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(S(28), S(14));
            lblTitle.MouseDown += Drag;

            lblInfo.Font = new Font("Segoe UI", 9.5F);
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(S(31), S(54));
            lblInfo.MouseDown += Drag;

            closeX.SetBounds(S(598), S(18), S(38), S(34));
            closeX.Radius = S(12);
            closeX.Text = "✕";
            closeX.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            StyleBtn(closeX);
            closeX.Hover = Color.FromArgb(232, 65, 80);
            closeX.Pressed = Color.FromArgb(190, 45, 60);
            closeX.Click += delegate { Close(); };
            for (int i = 0; i < chips.Length; i++)
            {
                RoundButton c = new RoundButton();
                c.SetBounds(S(24) + i * S(160), S(92), S(150), S(34));
                c.Radius = S(17);
                c.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                StyleBtn(c);
                c.Accent1 = t.Accent;
                c.Accent2 = t.Accent2;
                int idx = i;
                c.Click += delegate
                {
                    filter = idx;
                    offset = 0;
                    UpdateHeader();
                    Rebuild();
                };
                chips[i] = c;
                Controls.Add(c);
            }
            viewport.SetBounds(S(24), S(140), S(598), S(380));
            viewport.BackColor = t.Bg;

            content.BackColor = t.Bg;
            content.SetBounds(0, 0, S(598), S(10));
            viewport.Controls.Add(content);

            scroll.SetBounds(S(626), S(140), S(14), S(380));
            scroll.TrackColor = t.Track;
            scroll.ThumbColor = Color.FromArgb(150, t.Sub);
            scroll.HotColor = t.Accent;
            scroll.Scrolled += delegate { SetOffset(scroll.Value); };

            closeBtn.SetBounds(S(496), S(536), S(140), S(40));
            closeBtn.Radius = S(20);
            closeBtn.Text = tr("close", null);
            closeBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            closeBtn.UseAccent = true;
            closeBtn.Accent1 = t.Accent;
            closeBtn.Accent2 = t.Accent2;
            closeBtn.Click += delegate { Close(); };

            Controls.Add(lblTitle);
            Controls.Add(lblInfo);
            Controls.Add(closeX);
            Controls.Add(viewport);
            Controls.Add(scroll);
            Controls.Add(closeBtn);

            MouseDown += Drag;

            ResumeLayout(false);

            UpdateHeader();
            Rebuild();
        }

        private void UpdateHeader()
        {
            int active = 0;
            foreach (StartupEntry en in entries) if (en.Enabled) active++;
            int all = entries.Count;

            lblInfo.ForeColor = t.Sub;
            lblInfo.Text = tr("startupInfo", new object[] { all, active });

            string[] names = { tr("all", null), tr("active", null), tr("inactive", null) };
            int[] counts = { all, active, all - active };
            for (int i = 0; i < chips.Length; i++)
            {
                chips[i].Text = names[i] + " (" + counts[i] + ")";
                chips[i].UseAccent = (i == filter);
                chips[i].Invalidate();
            }
        }

        private void SetOffset(int v)
        {
            int max = Math.Max(0, contentH - viewport.Height);
            offset = Math.Max(0, Math.Min(max, v));
            content.Top = -offset;
            scroll.SetState(contentH, viewport.Height, offset);
        }

        private void Rebuild()
        {
            content.SuspendLayout();

            while (content.Controls.Count > 0)
            {
                Control c = content.Controls[0];
                content.Controls.RemoveAt(0);
                c.Dispose();
            }

            List<StartupEntry> shown = new List<StartupEntry>();
            foreach (StartupEntry en in entries)
            {
                if (filter == 0 || (filter == 1 && en.Enabled) || (filter == 2 && !en.Enabled)) shown.Add(en);
            }

            if (shown.Count == 0)
            {
                Label empty = new Label();
                empty.AutoSize = false;
                empty.Text = tr("filterEmpty", null);
                empty.ForeColor = t.Sub;
                empty.Font = new Font("Segoe UI", 11F);
                empty.TextAlign = ContentAlignment.MiddleCenter;
                empty.SetBounds(0, S(120), viewport.Width, S(40));
                content.Controls.Add(empty);
                contentH = viewport.Height;
            }
            else
            {
                int y = 0;
                foreach (StartupEntry en in shown)
                {
                    AddCard(en, y);
                    y += S(80);
                }
                contentH = y;
            }

            content.Size = new Size(viewport.Width, Math.Max(1, contentH));
            content.ResumeLayout(true);
            SetOffset(offset);
        }

        private void AddCard(StartupEntry entry, int y)
        {
            RoundPanel card = new RoundPanel();
            card.SetBounds(0, y, S(598), S(70));
            card.Radius = S(18);
            card.BackColor = t.Card;
            int h = 7;
            foreach (char ch in entry.Name) h = unchecked(h * 31 + ch);
            h &= 0x7fffffff;

            IconTile tile = new IconTile();
            tile.SetBounds(S(14), S(13), S(44), S(44));
            tile.Radius = S(14);
            tile.Picture = entry.IconBmp;
            tile.Fill = entry.IconBmp != null ? t.Bg : Palette[h % Palette.Length];
            string nm = entry.Name.Trim();
            tile.Letter = nm.Length > 0 ? nm.Substring(0, 1).ToUpperInvariant() : "?";

            Label lblName = new Label();
            lblName.AutoSize = false;
            lblName.SetBounds(S(70), S(12), S(246), S(22));
            lblName.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblName.AutoEllipsis = true;
            lblName.Text = entry.Name;

            Label lblPath = new Label();
            lblPath.AutoSize = false;
            lblPath.SetBounds(S(70), S(36), S(246), S(20));
            lblPath.Font = new Font("Segoe UI", 8.5F);
            lblPath.ForeColor = t.Sub;
            lblPath.AutoEllipsis = true;
            lblPath.Text = entry.Command;
            tip.SetToolTip(lblPath, entry.Command);

            Label lblStatus = new Label();
            lblStatus.AutoSize = false;
            lblStatus.SetBounds(S(324), S(14), S(134), S(20));
            lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblStatus.AutoEllipsis = true;

            Label lblTag = new Label();
            lblTag.AutoSize = false;
            lblTag.SetBounds(S(324), S(36), S(134), S(20));
            lblTag.Font = new Font("Segoe UI", 8.5F);
            lblTag.ForeColor = t.Sub;
            lblTag.AutoEllipsis = true;
            lblTag.Text = entry.Tag;

            RoundButton btn = new RoundButton();
            btn.SetBounds(S(464), S(17), S(120), S(36));
            btn.Radius = S(18);
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Fore = Color.White;
            btn.Border = Color.Empty;

            Action refresh = delegate
            {
                bool on = entry.Enabled;
                lblName.ForeColor = on ? t.Text : t.Sub;
                lblStatus.Text = "● " + tr(on ? "statusOn" : "statusOff", null);
                lblStatus.ForeColor = on ? Color.FromArgb(34, 197, 94) : t.Sub;
                btn.Text = tr(on ? "disable" : "enable", null);
                if (on)
                {
                    btn.Normal = Color.FromArgb(239, 68, 68);
                    btn.Hover = Color.FromArgb(248, 113, 113);
                    btn.Pressed = Color.FromArgb(220, 38, 38);
                }
                else
                {
                    btn.Normal = Color.FromArgb(34, 197, 94);
                    btn.Hover = Color.FromArgb(74, 222, 128);
                    btn.Pressed = Color.FromArgb(22, 163, 74);
                }
                btn.Invalidate();
            };

            btn.Click += delegate
            {
                bool target = !entry.Enabled;
                string err = entry.SetEnabled(target);
                if (err == null)
                {
                    UpdateHeader();
                    log(tr(target ? "startupEnabledLog" : "startupDisabledLog", new object[] { entry.Name }));
                    if (filter == 0) refresh();   // filtre "Hepsi" ise sadece kartı güncelle
                    else Rebuild();               // aksi halde liste filtreye göre yeniden oluşur
                }
                else
                {
                    lblInfo.ForeColor = Color.FromArgb(239, 68, 68);
                    lblInfo.Text = "⚠ " + (err == "ADMIN" ? tr("needAdmin", null) : err);
                }
            };

            refresh();

            card.Controls.Add(tile);
            card.Controls.Add(lblName);
            card.Controls.Add(lblPath);
            card.Controls.Add(lblStatus);
            card.Controls.Add(lblTag);
            card.Controls.Add(btn);
            content.Controls.Add(card);
        }
    }
    internal sealed class BloatApp
    {
        public string Name = "";      // ekranda görünen ad
        public string Id = "";        // Appx paket adı
        public bool Removed;
        public bool Busy;

        private static readonly string[][] Known =
        {
            new[] { "Xbox Game Bar", "Microsoft.XboxGamingOverlay" },
            new[] { "Xbox", "Microsoft.GamingApp" },
            new[] { "Weather", "Microsoft.BingWeather" },
            new[] { "News", "Microsoft.BingNews" },
            new[] { "Get Help", "Microsoft.GetHelp" },
            new[] { "Tips", "Microsoft.Getstarted" },
            new[] { "Solitaire Collection", "Microsoft.MicrosoftSolitaireCollection" },
            new[] { "Media Player (Groove)", "Microsoft.ZuneMusic" },
            new[] { "Movies & TV", "Microsoft.ZuneVideo" },
            new[] { "People", "Microsoft.People" },
            new[] { "Feedback Hub", "Microsoft.WindowsFeedbackHub" },
            new[] { "Mixed Reality Portal", "Microsoft.MixedReality.Portal" },
            new[] { "3D Viewer", "Microsoft.Microsoft3DViewer" },
            new[] { "Paint 3D", "Microsoft.MSPaint" },
            new[] { "Microsoft 365 (Office)", "Microsoft.MicrosoftOfficeHub" },
            new[] { "Phone Link", "Microsoft.YourPhone" },
            new[] { "Maps", "Microsoft.WindowsMaps" },
            new[] { "Skype", "Microsoft.SkypeApp" },
            new[] { "Clipchamp", "Clipchamp.Clipchamp" },
            new[] { "Microsoft Teams (Personal)", "MicrosoftTeams" },
            new[] { "Cortana", "Microsoft.549981C3F5F10" },
            new[] { "Power Automate", "Microsoft.PowerAutomateDesktop" },
            new[] { "Microsoft To Do", "Microsoft.Todos" }
        };

        private static bool RunPs(string script, int timeoutMs, out string stdout, out string stderr, out int exitCode)
        {
            stdout = "";
            stderr = "";
            exitCode = -1;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe",
                    "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"" + script + "\"");
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                System.Text.StringBuilder so = new System.Text.StringBuilder();
                System.Text.StringBuilder se = new System.Text.StringBuilder();

                using (Process p = Process.Start(psi))
                {
                    if (p == null) return false;
                    p.OutputDataReceived += delegate (object s, DataReceivedEventArgs e) { if (e.Data != null) so.AppendLine(e.Data); };
                    p.ErrorDataReceived += delegate (object s, DataReceivedEventArgs e) { if (e.Data != null) se.AppendLine(e.Data); };
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    if (!p.WaitForExit(timeoutMs))
                    {
                        try { p.Kill(); } catch { }
                        stdout = so.ToString();
                        stderr = "timeout";
                        return false;
                    }
                    p.WaitForExit();
                    stdout = so.ToString();
                    stderr = se.ToString();
                    exitCode = p.ExitCode;
                    return true;
                }
            }
            catch (Exception ex)
            {
                stderr = ex.Message;
                return false;
            }
        }
        public static List<BloatApp> LoadInstalled()
        {
            List<BloatApp> list = new List<BloatApp>();
            string so, se;
            int code;
            if (!RunPs("Get-AppxPackage | ForEach-Object { $_.Name }", 60000, out so, out se, out code) || code != 0)
                return list;

            HashSet<string> installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in so.Split('\n'))
            {
                string n = line.Trim();
                if (n.Length > 0) installed.Add(n);
            }

            foreach (string[] kn in Known)
            {
                if (installed.Contains(kn[1]))
                {
                    BloatApp a = new BloatApp();
                    a.Name = kn[0];
                    a.Id = kn[1];
                    list.Add(a);
                }
            }

            list.Sort(delegate (BloatApp a, BloatApp b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
            return list;
        }
        public string Remove()
        {
            string so, se;
            int code;
            string script = "$ErrorActionPreference='Stop'; Get-AppxPackage -Name '" + Id + "' | Remove-AppxPackage";
            bool ok = RunPs(script, 180000, out so, out se, out code);
            if (ok && code == 0) return null;

            string err = se == null ? "" : se.Trim();
            string low = err.ToLowerInvariant();
            if (low.Contains("0x80070005") || low.Contains("access is denied") || low.Contains("unauthorized"))
                return "ADMIN";

            if (err.StartsWith("#<", StringComparison.Ordinal) || err.Length == 0)
                return "PowerShell error (exit code " + code.ToString(CultureInfo.InvariantCulture) + ")";

            string first = err.Split('\n')[0].Trim();
            if (first.Length > 150) first = first.Substring(0, 150) + "…";
            return first;
        }
    }
    internal sealed class BloatForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private readonly Theme t;
        private readonly Func<string, object[], string> tr;
        private readonly Action<string> log;
        private readonly List<BloatApp> apps;
        private readonly float k;                       // DPI ölçeği

        private int offset;
        private int contentH;

        private readonly Label lblTitle = new Label();
        private readonly Label lblInfo = new Label();
        private readonly RoundButton closeX = new RoundButton();
        private readonly RoundButton closeBtn = new RoundButton();
        private readonly BufferedPanel viewport = new BufferedPanel();
        private readonly BufferedPanel content = new BufferedPanel();
        private readonly RoundScrollBar scroll = new RoundScrollBar();
        private readonly ToolTip tip = new ToolTip();

        private static readonly Color[] Palette =
        {
            Color.FromArgb(99, 102, 241), Color.FromArgb(14, 165, 233), Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11), Color.FromArgb(244, 63, 94), Color.FromArgb(168, 85, 247)
        };

        private int S(int v) { return (int)Math.Round(v * k); }

        public BloatForm(Theme theme, Func<string, object[], string> translate, Action<string> logger, List<BloatApp> items)
        {
            t = theme;
            tr = translate;
            log = logger;
            apps = items;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                k = g.DpiX / 96f;
            }

            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;   // Alaz logosu (görev çubuğu / Alt-Tab)
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(S(660), S(600));
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            BackColor = t.Bg;
            KeyPreview = true;

            BuildUi();

            KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            Resize += delegate { ApplyRegion(); };
            Load += delegate { ApplyRegion(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) tip.Dispose();
            base.Dispose(disposing);
        }

        private void ApplyRegion()
        {
            if (Width < 10 || Height < 10) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), S(26)))
            {
                Region = new Region(p);
            }
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetOffset(offset - e.Delta * S(70) / 120);
        }

        private void BuildUi()
        {
            SuspendLayout();

            lblTitle.Text = "📦 " + tr("bloat", null);
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = t.Text;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(S(28), S(14));
            lblTitle.MouseDown += Drag;

            lblInfo.Font = new Font("Segoe UI", 9.5F);
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(S(31), S(54));
            lblInfo.MouseDown += Drag;

            closeX.SetBounds(S(598), S(18), S(38), S(34));
            closeX.Radius = S(12);
            closeX.Text = "✕";
            closeX.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            closeX.Normal = t.Btn;
            closeX.Hover = Color.FromArgb(232, 65, 80);
            closeX.Pressed = Color.FromArgb(190, 45, 60);
            closeX.Border = t.Border;
            closeX.Fore = t.Text;
            closeX.Click += delegate { Close(); };

            viewport.SetBounds(S(24), S(96), S(598), S(424));
            viewport.BackColor = t.Bg;

            content.BackColor = t.Bg;
            content.SetBounds(0, 0, S(598), S(10));
            viewport.Controls.Add(content);

            scroll.SetBounds(S(626), S(96), S(14), S(424));
            scroll.TrackColor = t.Track;
            scroll.ThumbColor = Color.FromArgb(150, t.Sub);
            scroll.HotColor = t.Accent;
            scroll.Scrolled += delegate { SetOffset(scroll.Value); };

            closeBtn.SetBounds(S(496), S(536), S(140), S(40));
            closeBtn.Radius = S(20);
            closeBtn.Text = tr("close", null);
            closeBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            closeBtn.UseAccent = true;
            closeBtn.Accent1 = t.Accent;
            closeBtn.Accent2 = t.Accent2;
            closeBtn.Click += delegate { Close(); };

            Controls.Add(lblTitle);
            Controls.Add(lblInfo);
            Controls.Add(closeX);
            Controls.Add(viewport);
            Controls.Add(scroll);
            Controls.Add(closeBtn);

            MouseDown += Drag;

            ResumeLayout(false);

            UpdateHeader();
            BuildList();
        }

        private void UpdateHeader()
        {
            int remaining = 0;
            foreach (BloatApp a in apps) if (!a.Removed) remaining++;
            lblInfo.ForeColor = t.Sub;
            lblInfo.Text = tr("bloatInfo", new object[] { remaining });
        }

        private void SetOffset(int v)
        {
            int max = Math.Max(0, contentH - viewport.Height);
            offset = Math.Max(0, Math.Min(max, v));
            content.Top = -offset;
            scroll.SetState(contentH, viewport.Height, offset);
        }

        private void BuildList()
        {
            content.SuspendLayout();

            if (apps.Count == 0)
            {
                Label empty = new Label();
                empty.AutoSize = false;
                empty.UseMnemonic = false;
                empty.Text = tr("bloatNone", null);
                empty.ForeColor = t.Sub;
                empty.Font = new Font("Segoe UI", 11F);
                empty.TextAlign = ContentAlignment.MiddleCenter;
                empty.SetBounds(0, S(120), viewport.Width, S(40));
                content.Controls.Add(empty);
                contentH = viewport.Height;
            }
            else
            {
                int y = 0;
                foreach (BloatApp a in apps)
                {
                    AddCard(a, y);
                    y += S(80);
                }
                contentH = y;
            }

            content.Size = new Size(viewport.Width, Math.Max(1, contentH));
            content.ResumeLayout(true);
            SetOffset(0);
        }

        private void AddCard(BloatApp app, int y)
        {
            RoundPanel card = new RoundPanel();
            card.SetBounds(0, y, S(598), S(70));
            card.Radius = S(18);
            card.BackColor = t.Card;

            int h = 7;
            foreach (char ch in app.Name) h = unchecked(h * 31 + ch);
            h &= 0x7fffffff;

            IconTile tile = new IconTile();
            tile.SetBounds(S(14), S(13), S(44), S(44));
            tile.Radius = S(14);
            tile.Fill = Palette[h % Palette.Length];
            string nm = app.Name.Trim();
            tile.Letter = nm.Length > 0 ? nm.Substring(0, 1).ToUpperInvariant() : "?";

            Label lblName = new Label();
            lblName.AutoSize = false;
            lblName.UseMnemonic = false;
            lblName.SetBounds(S(70), S(12), S(380), S(22));
            lblName.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblName.AutoEllipsis = true;
            lblName.Text = app.Name;

            Label lblId = new Label();
            lblId.AutoSize = false;
            lblId.UseMnemonic = false;
            lblId.SetBounds(S(70), S(36), S(380), S(20));
            lblId.Font = new Font("Segoe UI", 8.5F);
            lblId.ForeColor = t.Sub;
            lblId.AutoEllipsis = true;
            lblId.Text = app.Id;
            tip.SetToolTip(lblId, app.Id);

            RoundButton btn = new RoundButton();
            btn.SetBounds(S(464), S(17), S(120), S(36));
            btn.Radius = S(18);
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Fore = Color.White;
            btn.Border = Color.Empty;

            Action refresh = delegate
            {
                if (app.Removed)
                {
                    lblName.ForeColor = t.Sub;
                    btn.Text = tr("bloatRemoved", null);
                    btn.Normal = t.Btn;
                    btn.Hover = t.Btn;
                    btn.Pressed = t.Btn;
                    btn.Fore = t.Sub;
                    btn.Border = t.Border;
                    btn.Enabled = false;
                }
                else if (app.Busy)
                {
                    lblName.ForeColor = t.Text;
                    btn.Text = tr("bloatRemoving", null);
                    btn.Enabled = false;
                }
                else
                {
                    lblName.ForeColor = t.Text;
                    btn.Text = tr("bloatRemove", null);
                    btn.Normal = Color.FromArgb(239, 68, 68);
                    btn.Hover = Color.FromArgb(248, 113, 113);
                    btn.Pressed = Color.FromArgb(220, 38, 38);
                    btn.Fore = Color.White;
                    btn.Border = Color.Empty;
                    btn.Enabled = true;
                }
                btn.Invalidate();
            };

            btn.Click += async delegate
            {
                if (app.Removed || app.Busy) return;
                string body = tr("alertRemoveHead", new object[] { app.Name }) + "\n\n" + tr("alertBody", null);
                bool yes = AlertForm.Ask(this, t, tr("alertTitle", null), body, tr("bloatRemove", null), tr("alertCancel", null));
                if (!yes) return;

                app.Busy = true;
                refresh();

                string err = await Task.Run(() => app.Remove());

                app.Busy = false;
                if (IsDisposed) return;

                if (err == null)
                {
                    app.Removed = true;
                    UpdateHeader();
                    log(tr("bloatRemovedLog", new object[] { app.Name }));
                }
                else
                {
                    string shown = err == "ADMIN" ? tr("needAdmin", null) : err;
                    lblInfo.ForeColor = Color.FromArgb(239, 68, 68);
                    lblInfo.Text = "⚠ " + shown;
                    log(tr("bloatFail", new object[] { app.Name, shown }));
                }
                refresh();
            };

            refresh();

            card.Controls.Add(tile);
            card.Controls.Add(lblName);
            card.Controls.Add(lblId);
            card.Controls.Add(btn);
            content.Controls.Add(card);
        }
    }
    public partial class Form1 : Form, IMessageFilter
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, IntPtr pszRootPath, uint dwFlags);

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int EM_GETLINECOUNT = 0x00BA;
        private const int EM_LINESCROLL = 0x00B6;
        private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
        private const uint SHERB_NOCONFIRMATION = 0x1;
        private const uint SHERB_NOPROGRESSUI = 0x2;
        private const uint SHERB_NOSOUND = 0x4;
        private const int QuickCount = 4;   // temp, dns, ram, bin (başlangıç programları dahil değil)
        private static readonly string[] LangNames = { "English", "Türkçe", "Deutsch", "Español", "Français", "Italiano", "Polski", "Português", "Ελληνικά", "Русский", "Hrvatski", "العربية", "中文", "日本語", "한국어" };

        private static readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>
        {
            { "subtitle", new[] {
                "System Optimization Suite",
                "Sistem Optimizasyon Aracı",
                "System-Optimierungssuite",
                "Suite de optimización del sistema",
                "Suite d'optimisation système",
                "Suite di ottimizzazione del sistema",
                "Zestaw do optymalizacji systemu",
                "Suíte de otimização do sistema",
                "Σουίτα βελτιστοποίησης συστήματος",
                "Набор инструментов для оптимизации системы",
                "Paket za optimizaciju sustava",
                "مجموعة تحسين النظام",
                "系统优化工具套件",
                "システム最適化スイート",
                "시스템 최적화 도구" } },
            { "quick", new[] {
                "Quick Boost",
                "Hızlı Güçlendirme",
                "Schnell-Boost",
                "Impulso rápido",
                "Boost Rapide",
                "Boost rapido",
                "Szybkie przyspieszenie",
                "Boost rápido",
                "Γρήγορη ενίσχυση",
                "Быстрое ускорение",
                "Brzo ubrzanje",
                "تعزيز سريع",
                "快速加速",
                "クイックブースト",
                "빠른 부스트" } },
            { "quickSub", new[] {
                "All in one",
                "Hepsi bir arada",
                "Alles in einem",
                "Todo en uno",
                "Tout-en-un",
                "Tutto in uno",
                "Wszystko w jednym",
                "Tudo em um",
                "Όλα σε ένα",
                "Всё в одном",
                "Sve u jednom",
                "كل شيء في واحد",
                "多合一",
                "オールインワン",
                "올인원" } },
            { "temp", new[] {
                "Clean Temp Files",
                "Geçici Dosyaları Temizle",
                "Temp-Dateien löschen",
                "Limpiar archivos temporales",
                "Nettoyer les fichiers temp.",
                "Pulisci file temporanei",
                "Wyczyść pliki tymczasowe",
                "Limpar arquivos temporários",
                "Εκκαθάριση προσωρινών αρχείων",
                "Очистить временные файлы",
                "Očisti privremene datoteke",
                "تنظيف الملفات المؤقتة",
                "清理临时文件",
                "一時ファイルを削除",
                "임시 파일 정리" } },
            { "dns", new[] {
                "Flush DNS",
                "DNS Önbelleğini Temizle",
                "DNS-Cache leeren",
                "Vaciar caché DNS",
                "Vider le cache DNS",
                "Svuota cache DNS",
                "Wyczyść pamięć podręczną DNS",
                "Limpar cache DNS",
                "Εκκαθάριση προσωρινής μνήμης DNS",
                "Очистить кэш DNS",
                "Očisti DNS predmemoriju",
                "مسح ذاكرة DNS المؤقتة",
                "刷新 DNS 缓存",
                "DNSキャッシュをクリア",
                "DNS 캐시 비우기" } },
            { "ram", new[] {
                "Optimize RAM",
                "RAM'i Optimize Et",
                "RAM optimieren",
                "Optimizar RAM",
                "Optimiser la RAM",
                "Ottimizza RAM",
                "Optymalizuj RAM",
                "Otimizar RAM",
                "Βελτιστοποίηση RAM",
                "Оптимизировать ОЗУ",
                "Optimiziraj RAM",
                "تحسين الذاكرة (RAM)",
                "优化内存",
                "RAMを最適化",
                "RAM 최적화" } },
            { "bin", new[] {
                "Empty Recycle Bin",
                "Çöp Kutusunu Boşalt",
                "Papierkorb leeren",
                "Vaciar la papelera",
                "Vider la corbeille",
                "Svuota cestino",
                "Opróżnij Kosz",
                "Esvaziar lixeira",
                "Άδειασμα Κάδου ανακύκλωσης",
                "Очистить корзину",
                "Isprazni koš za smeće",
                "إفراغ سلة المحذوفات",
                "清空回收站",
                "ごみ箱を空にする",
                "휴지통 비우기" } },
            { "startup", new[] {
                "Startup Programs",
                "Başlangıç Programları",
                "Autostart-Programme",
                "Programas de inicio",
                "Programmes au démarrage",
                "Programmi all'avvio",
                "Programy startowe",
                "Programas de inicialização",
                "Προγράμματα εκκίνησης",
                "Автозагрузка",
                "Programi pri pokretanju",
                "برامج بدء التشغيل",
                "启动程序",
                "スタートアッププログラム",
                "시작 프로그램" } },
            { "log", new[] {
                "Activity Log",
                "İşlem Geçmişi",
                "Aktivitätsprotokoll",
                "Registro de actividad",
                "Journal d'activité",
                "Registro attività",
                "Dziennik aktywności",
                "Registro de atividades",
                "Αρχείο καταγραφής δραστηριότητας",
                "Журнал действий",
                "Dnevnik aktivnosti",
                "سجل النشاط",
                "活动日志",
                "アクティビティログ",
                "활동 로그" } },
            { "themeDark", new[] {
                "🌙 Dark Mode",
                "🌙 Koyu Mod",
                "🌙 Dunkel",
                "🌙 Modo oscuro",
                "🌙 Mode sombre",
                "🌙 Modalità scura",
                "🌙 Tryb ciemny",
                "🌙 Modo escuro",
                "🌙 Σκούρο θέμα",
                "🌙 Тёмная тема",
                "🌙 Tamni način",
                "🌙 الوضع الداكن",
                "🌙 深色模式",
                "🌙 ダークモード",
                "🌙 다크 모드" } },
            { "themeLight", new[] {
                "☀ Light Mode",
                "☀ Açık Mod",
                "☀ Hell",
                "☀ Modo claro",
                "☀ Mode clair",
                "☀ Modalità chiara",
                "☀ Tryb jasny",
                "☀ Modo claro",
                "☀ Φωτεινό θέμα",
                "☀ Светлая тема",
                "☀ Svijetli način",
                "☀ الوضع الفاتح",
                "☀ 浅色模式",
                "☀ ライトモード",
                "☀ 라이트 모드" } },
            { "ready", new[] {
                "Alaz PC Optimizer is ready.",
                "Alaz PC Optimizer hazır.",
                "Alaz PC Optimizer ist bereit.",
                "Alaz PC Optimizer está listo.",
                "Alaz PC Optimizer est prêt.",
                "Alaz PC Optimizer è pronto.",
                "Alaz PC Optimizer jest gotowy.",
                "O Alaz PC Optimizer está pronto.",
                "Το Alaz PC Optimizer είναι έτοιμο.",
                "Alaz PC Optimizer готов к работе.",
                "Alaz PC Optimizer je spreman.",
                "Alaz PC Optimizer جاهز.",
                "Alaz PC Optimizer 已就绪。",
                "Alaz PC Optimizer の準備ができました。",
                "Alaz PC Optimizer가 준비되었습니다." } },
            { "start", new[] {
                "Starting: {0}",
                "Başlatılıyor: {0}",
                "Start: {0}",
                "Iniciando: {0}",
                "Démarrage : {0}",
                "Avvio: {0}",
                "Uruchamianie: {0}",
                "Iniciando: {0}",
                "Εκκίνηση: {0}",
                "Запуск: {0}",
                "Pokretanje: {0}",
                "جارٍ البدء: {0}",
                "正在启动：{0}",
                "開始中: {0}",
                "시작 중: {0}" } },
            { "tempDone", new[] {
                "Temp cleanup done: {0} files removed, {1:0.0} MB freed.",
                "Temp temizliği tamamlandı: {0} dosya silindi, {1:0.0} MB boşaltıldı.",
                "Temp-Bereinigung abgeschlossen: {0} Dateien gelöscht, {1:0.0} MB freigegeben.",
                "Limpieza de temporales completada: {0} archivos eliminados, {1:0.0} MB liberados.",
                "Nettoyage terminé : {0} fichiers supprimés, {1:0.0} Mo libérés.",
                "Pulizia dei temporanei completata: {0} file rimossi, {1:0.0} MB liberati.",
                "Czyszczenie plików tymczasowych zakończone: usunięto {0} plików, zwolniono {1:0.0} MB.",
                "Limpeza de temporários concluída: {0} arquivos removidos, {1:0.0} MB liberados.",
                "Η εκκαθάριση προσωρινών ολοκληρώθηκε: διαγράφηκαν {0} αρχεία, ελευθερώθηκαν {1:0.0} MB.",
                "Очистка временных файлов завершена: удалено файлов — {0}, освобождено {1:0.0} МБ.",
                "Čišćenje privremenih datoteka dovršeno: obrisano datoteka: {0}, oslobođeno {1:0.0} MB.",
                "اكتمل تنظيف الملفات المؤقتة: تم حذف {0} ملف وتحرير {1:0.0} MB.",
                "临时文件清理完成：已删除 {0} 个文件，释放 {1:0.0} MB。",
                "一時ファイルのクリーンアップが完了しました: {0} 個のファイルを削除し、{1:0.0} MB を解放しました。",
                "임시 파일 정리 완료: 파일 {0}개 삭제, {1:0.0} MB 확보." } },
            { "dnsDone", new[] {
                "DNS cache flushed successfully.",
                "DNS önbelleği başarıyla temizlendi.",
                "DNS-Cache erfolgreich geleert.",
                "Caché DNS vaciada correctamente.",
                "Cache DNS vidé avec succès.",
                "Cache DNS svuotata correttamente.",
                "Pamięć podręczna DNS została wyczyszczona.",
                "Cache DNS limpo com sucesso.",
                "Η προσωρινή μνήμη DNS εκκαθαρίστηκε επιτυχώς.",
                "Кэш DNS успешно очищен.",
                "DNS predmemorija uspješno je očišćena.",
                "تم مسح ذاكرة DNS المؤقتة بنجاح.",
                "DNS 缓存已成功刷新。",
                "DNSキャッシュを正常にクリアしました。",
                "DNS 캐시를 성공적으로 비웠습니다." } },
            { "dnsFail", new[] {
                "DNS flush failed (run as administrator): {0}",
                "DNS temizlenemedi (yönetici olarak çalıştırın): {0}",
                "DNS-Leerung fehlgeschlagen (als Administrator ausführen): {0}",
                "Error al vaciar DNS (ejecuta como administrador): {0}",
                "Échec du vidage DNS (exécutez en administrateur) : {0}",
                "Svuotamento DNS non riuscito (esegui come amministratore): {0}",
                "Nie udało się wyczyścić DNS (uruchom jako administrator): {0}",
                "Falha ao limpar o DNS (execute como administrador): {0}",
                "Η εκκαθάριση DNS απέτυχε (εκτελέστε ως διαχειριστής): {0}",
                "Не удалось очистить DNS (запустите от имени администратора): {0}",
                "Čišćenje DNS-a nije uspjelo (pokrenite kao administrator): {0}",
                "فشل مسح DNS (شغّل البرنامج كمسؤول): {0}",
                "DNS 刷新失败（请以管理员身份运行）：{0}",
                "DNSのクリアに失敗しました（管理者として実行してください）: {0}",
                "DNS 비우기 실패(관리자 권한으로 실행하세요): {0}" } },
            { "ramDone", new[] {
                "RAM optimized. Working set trimmed by {0:0.0} MB.",
                "RAM optimize edildi. Çalışma kümesi {0:0.0} MB azaltıldı.",
                "RAM optimiert. Arbeitssatz um {0:0.0} MB reduziert.",
                "RAM optimizada. Conjunto de trabajo reducido en {0:0.0} MB.",
                "RAM optimisée. Jeu de travail réduit de {0:0.0} Mo.",
                "RAM ottimizzata. Working set ridotto di {0:0.0} MB.",
                "RAM zoptymalizowany. Zestaw roboczy zmniejszono o {0:0.0} MB.",
                "RAM otimizada. Conjunto de trabalho reduzido em {0:0.0} MB.",
                "Η RAM βελτιστοποιήθηκε. Το σύνολο εργασίας μειώθηκε κατά {0:0.0} MB.",
                "ОЗУ оптимизирована. Рабочий набор уменьшен на {0:0.0} МБ.",
                "RAM je optimiziran. Radni skup smanjen za {0:0.0} MB.",
                "تم تحسين الذاكرة. تم تقليص مجموعة العمل بمقدار {0:0.0} MB.",
                "内存已优化。工作集减少了 {0:0.0} MB。",
                "RAMを最適化しました。ワーキングセットを {0:0.0} MB 削減しました。",
                "RAM이 최적화되었습니다. 작업 집합이 {0:0.0} MB 줄었습니다." } },
            { "binDone", new[] {
                "Recycle Bin emptied.",
                "Çöp Kutusu boşaltıldı.",
                "Papierkorb geleert.",
                "Papelera vaciada.",
                "Corbeille vidée.",
                "Cestino svuotato.",
                "Kosz został opróżniony.",
                "Lixeira esvaziada.",
                "Ο Κάδος ανακύκλωσης άδειασε.",
                "Корзина очищена.",
                "Koš za smeće je ispražnjen.",
                "تم إفراغ سلة المحذوفات.",
                "回收站已清空。",
                "ごみ箱を空にしました。",
                "휴지통을 비웠습니다." } },
            { "binEmpty", new[] {
                "Recycle Bin is already empty.",
                "Çöp Kutusu zaten boş.",
                "Der Papierkorb ist bereits leer.",
                "La papelera ya está vacía.",
                "La corbeille est déjà vide.",
                "Il cestino è già vuoto.",
                "Kosz jest już pusty.",
                "A lixeira já está vazia.",
                "Ο Κάδος ανακύκλωσης είναι ήδη άδειος.",
                "Корзина уже пуста.",
                "Koš za smeće je već prazan.",
                "سلة المحذوفات فارغة بالفعل.",
                "回收站已经是空的。",
                "ごみ箱はすでに空です。",
                "휴지통이 이미 비어 있습니다." } },
            { "binFail", new[] {
                "Recycle Bin error code: 0x{0:X}",
                "Çöp Kutusu hata kodu: 0x{0:X}",
                "Papierkorb-Fehlercode: 0x{0:X}",
                "Código de error de la papelera: 0x{0:X}",
                "Code d'erreur de la corbeille : 0x{0:X}",
                "Codice errore cestino: 0x{0:X}",
                "Kod błędu Kosza: 0x{0:X}",
                "Código de erro da lixeira: 0x{0:X}",
                "Κωδικός σφάλματος Κάδου: 0x{0:X}",
                "Код ошибки корзины: 0x{0:X}",
                "Kôd pogreške koša za smeće: 0x{0:X}",
                "رمز خطأ سلة المحذوفات: 0x{0:X}",
                "回收站错误代码：0x{0:X}",
                "ごみ箱のエラーコード: 0x{0:X}",
                "휴지통 오류 코드: 0x{0:X}" } },
            { "startupHead", new[] {
                "Startup programs found: {0}",
                "Bulunan başlangıç programları: {0}",
                "Gefundene Autostart-Programme: {0}",
                "Programas de inicio encontrados: {0}",
                "Programmes au démarrage trouvés : {0}",
                "Programmi all'avvio trovati: {0}",
                "Znalezione programy startowe: {0}",
                "Programas de inicialização encontrados: {0}",
                "Βρέθηκαν προγράμματα εκκίνησης: {0}",
                "Найдено программ автозагрузки: {0}",
                "Pronađeni programi pri pokretanju: {0}",
                "برامج بدء التشغيل التي تم العثور عليها: {0}",
                "找到的启动程序：{0}",
                "見つかったスタートアッププログラム: {0}",
                "발견된 시작 프로그램: {0}" } },
            { "startupNone", new[] {
                "No startup entries found.",
                "Başlangıç kaydı bulunamadı.",
                "Keine Autostart-Einträge gefunden.",
                "No se encontraron entradas de inicio.",
                "Aucune entrée de démarrage trouvée.",
                "Nessuna voce di avvio trovata.",
                "Nie znaleziono wpisów startowych.",
                "Nenhuma entrada de inicialização encontrada.",
                "Δεν βρέθηκαν καταχωρήσεις εκκίνησης.",
                "Записи автозагрузки не найдены.",
                "Nema stavki pri pokretanju.",
                "لم يتم العثور على أي إدخالات بدء تشغيل.",
                "未找到启动项。",
                "スタートアップ項目が見つかりません。",
                "시작 항목을 찾을 수 없습니다." } },
            { "startupInfo", new[] {
                "{0} startup apps ({1} active)",
                "{0} başlangıç uygulaması ({1} aktif)",
                "{0} Autostart-Apps ({1} aktiv)",
                "{0} aplicaciones de inicio ({1} activas)",
                "{0} applications au démarrage ({1} actives)",
                "{0} app all'avvio ({1} attive)",
                "Aplikacje startowe: {0} (aktywne: {1})",
                "{0} aplicativos de inicialização ({1} ativos)",
                "{0} εφαρμογές εκκίνησης ({1} ενεργές)",
                "Приложений в автозагрузке: {0} (активных: {1})",
                "Aplikacije pri pokretanju: {0} (aktivnih: {1})",
                "{0} تطبيق بدء تشغيل ({1} نشط)",
                "{0} 个启动应用（{1} 个已启用）",
                "スタートアップアプリ {0} 件（有効 {1} 件）",
                "시작 앱 {0}개(활성 {1}개)" } },
            { "all", new[] {
                "All",
                "Tümü",
                "Alle",
                "Todos",
                "Tous",
                "Tutti",
                "Wszystkie",
                "Todos",
                "Όλες",
                "Все",
                "Sve",
                "الكل",
                "全部",
                "すべて",
                "전체" } },
            { "active", new[] {
                "Active",
                "Aktif",
                "Aktiv",
                "Activos",
                "Actifs",
                "Attivi",
                "Aktywne",
                "Ativos",
                "Ενεργές",
                "Активные",
                "Aktivne",
                "نشط",
                "已启用",
                "有効",
                "활성" } },
            { "inactive", new[] {
                "Disabled",
                "Devre Dışı",
                "Deaktiviert",
                "Desactivados",
                "Désactivés",
                "Disattivati",
                "Wyłączone",
                "Desativados",
                "Απενεργοποιημένες",
                "Отключённые",
                "Onemogućene",
                "معطّل",
                "已禁用",
                "無効",
                "비활성" } },
            { "filterEmpty", new[] {
                "No startup apps in this view.",
                "Bu görünümde başlangıç uygulaması yok.",
                "Keine Autostart-Apps in dieser Ansicht.",
                "No hay aplicaciones de inicio en esta vista.",
                "Aucune application dans cette vue.",
                "Nessuna app all'avvio in questa vista.",
                "Brak aplikacji startowych w tym widoku.",
                "Nenhum aplicativo de inicialização nesta visualização.",
                "Δεν υπάρχουν εφαρμογές εκκίνησης σε αυτή την προβολή.",
                "В этом представлении нет приложений автозагрузки.",
                "Nema aplikacija pri pokretanju u ovom prikazu.",
                "لا توجد تطبيقات بدء تشغيل في هذا العرض.",
                "此视图中没有启动应用。",
                "この表示にスタートアップアプリはありません。",
                "이 보기에는 시작 앱이 없습니다." } },
            { "statusOn", new[] {
                "Runs at startup",
                "Başlangıçta çalışıyor",
                "Startet beim Hochfahren",
                "Se ejecuta al iniciar",
                "Lancé au démarrage",
                "Si avvia con il sistema",
                "Uruchamia się przy starcie",
                "Executa na inicialização",
                "Εκτελείται κατά την εκκίνηση",
                "Запускается при старте",
                "Pokreće se pri pokretanju",
                "يعمل عند بدء التشغيل",
                "开机时运行",
                "起動時に実行",
                "시작 시 실행됨" } },
            { "statusOff", new[] {
                "Disabled",
                "Devre dışı",
                "Deaktiviert",
                "Desactivado",
                "Désactivé",
                "Disattivato",
                "Wyłączone",
                "Desativado",
                "Απενεργοποιημένη",
                "Отключено",
                "Onemogućeno",
                "معطّل",
                "已禁用",
                "無効",
                "비활성" } },
            { "disable", new[] {
                "Disable",
                "Devre Dışı Bırak",
                "Deaktivieren",
                "Desactivar",
                "Désactiver",
                "Disattiva",
                "Wyłącz",
                "Desativar",
                "Απενεργοποίηση",
                "Отключить",
                "Onemogući",
                "تعطيل",
                "禁用",
                "無効にする",
                "비활성화" } },
            { "enable", new[] {
                "Enable",
                "Geri Etkinleştir",
                "Aktivieren",
                "Activar",
                "Réactiver",
                "Attiva",
                "Włącz",
                "Ativar",
                "Ενεργοποίηση",
                "Включить",
                "Omogući",
                "تفعيل",
                "启用",
                "有効にする",
                "활성화" } },
            { "close", new[] {
                "Close",
                "Kapat",
                "Schließen",
                "Cerrar",
                "Fermer",
                "Chiudi",
                "Zamknij",
                "Fechar",
                "Κλείσιμο",
                "Закрыть",
                "Zatvori",
                "إغلاق",
                "关闭",
                "閉じる",
                "닫기" } },
            { "needAdmin", new[] {
                "Administrator rights are required for this item.",
                "Bu öğe için yönetici yetkisi gerekiyor.",
                "Für dieses Element sind Administratorrechte erforderlich.",
                "Este elemento requiere derechos de administrador.",
                "Droits administrateur requis pour cet élément.",
                "Per questo elemento sono necessari i privilegi di amministratore.",
                "Ten element wymaga uprawnień administratora.",
                "Este item requer direitos de administrador.",
                "Αυτό το στοιχείο απαιτεί δικαιώματα διαχειριστή.",
                "Для этого элемента требуются права администратора.",
                "Za ovu stavku potrebna su administratorska prava.",
                "يتطلب هذا العنصر صلاحيات المسؤول.",
                "此项目需要管理员权限。",
                "この項目には管理者権限が必要です。",
                "이 항목에는 관리자 권한이 필요합니다." } },
            { "startupDisabledLog", new[] {
                "Disabled from startup: {0}",
                "Başlangıçtan devre dışı bırakıldı: {0}",
                "Vom Autostart deaktiviert: {0}",
                "Desactivado del inicio: {0}",
                "Désactivé du démarrage : {0}",
                "Disattivato dall'avvio: {0}",
                "Wyłączono z autostartu: {0}",
                "Desativado da inicialização: {0}",
                "Απενεργοποιήθηκε από την εκκίνηση: {0}",
                "Отключено в автозагрузке: {0}",
                "Onemogućeno pri pokretanju: {0}",
                "تم التعطيل من بدء التشغيل: {0}",
                "已从启动项中禁用：{0}",
                "スタートアップから無効化しました: {0}",
                "시작 프로그램에서 비활성화됨: {0}" } },
            { "startupEnabledLog", new[] {
                "Re-enabled at startup: {0}",
                "Başlangıca geri eklendi: {0}",
                "Im Autostart wieder aktiviert: {0}",
                "Reactivado en el inicio: {0}",
                "Réactivé au démarrage : {0}",
                "Riattivato all'avvio: {0}",
                "Ponownie włączono w autostarcie: {0}",
                "Reativado na inicialização: {0}",
                "Ενεργοποιήθηκε ξανά στην εκκίνηση: {0}",
                "Снова включено в автозагрузке: {0}",
                "Ponovno omogućeno pri pokretanju: {0}",
                "تمت إعادة التفعيل في بدء التشغيل: {0}",
                "已重新启用启动：{0}",
                "スタートアップで再度有効化しました: {0}",
                "시작 프로그램에 다시 활성화됨: {0}" } },
            { "boostStart", new[] {
                "Quick Boost started.",
                "Hızlı Güçlendirme başlatıldı.",
                "Schnell-Boost gestartet.",
                "Impulso rápido iniciado.",
                "Boost Rapide démarré.",
                "Boost rapido avviato.",
                "Rozpoczęto szybkie przyspieszenie.",
                "Boost rápido iniciado.",
                "Η γρήγορη ενίσχυση ξεκίνησε.",
                "Быстрое ускорение запущено.",
                "Brzo ubrzanje pokrenuto.",
                "بدأ التعزيز السريع.",
                "快速加速已开始。",
                "クイックブーストを開始しました。",
                "빠른 부스트를 시작했습니다." } },
            { "boostDone", new[] {
                "Quick Boost completed in {0:0.0} s.",
                "Hızlı Güçlendirme {0:0.0} sn içinde tamamlandı.",
                "Schnell-Boost in {0:0.0} s abgeschlossen.",
                "Impulso rápido completado en {0:0.0} s.",
                "Boost Rapide terminé en {0:0.0} s.",
                "Boost rapido completato in {0:0.0} s.",
                "Szybkie przyspieszenie zakończone w {0:0.0} s.",
                "Boost rápido concluído em {0:0.0} s.",
                "Η γρήγορη ενίσχυση ολοκληρώθηκε σε {0:0.0} δ.",
                "Быстрое ускорение завершено за {0:0.0} с.",
                "Brzo ubrzanje dovršeno za {0:0.0} s.",
                "اكتمل التعزيز السريع في {0:0.0} ثانية.",
                "快速加速已在 {0:0.0} 秒内完成。",
                "クイックブーストが {0:0.0} 秒で完了しました。",
                "빠른 부스트가 {0:0.0}초 만에 완료되었습니다." } },
            { "error", new[] {
                "Error: {0}",
                "Hata: {0}",
                "Fehler: {0}",
                "Error: {0}",
                "Erreur : {0}",
                "Errore: {0}",
                "Błąd: {0}",
                "Erro: {0}",
                "Σφάλμα: {0}",
                "Ошибка: {0}",
                "Pogreška: {0}",
                "خطأ: {0}",
                "错误：{0}",
                "エラー: {0}",
                "오류: {0}" } },
            { "hw", new[] {
                "Hardware Info",
                "Donanım Bilgileri",
                "Hardware-Infos",
                "Información del hardware",
                "Infos matérielles",
                "Informazioni hardware",
                "Informacje o sprzęcie",
                "Informações de hardware",
                "Πληροφορίες υλικού",
                "Сведения об оборудовании",
                "Informacije o hardveru",
                "معلومات الأجهزة",
                "硬件信息",
                "ハードウェア情報",
                "하드웨어 정보" } },
            { "hwSub", new[] {
                "Live system information (updates every second)",
                "Canlı sistem bilgileri (her saniye güncellenir)",
                "Live-Systeminformationen (Aktualisierung jede Sekunde)",
                "Información del sistema en vivo (se actualiza cada segundo)",
                "Informations système en direct (mise à jour chaque seconde)",
                "Informazioni di sistema in tempo reale (aggiornamento ogni secondo)",
                "Informacje o systemie na żywo (odświeżanie co sekundę)",
                "Informações do sistema em tempo real (atualiza a cada segundo)",
                "Πληροφορίες συστήματος σε πραγματικό χρόνο (ενημέρωση κάθε δευτερόλεπτο)",
                "Системная информация в реальном времени (обновление каждую секунду)",
                "Informacije o sustavu uživo (osvježava se svake sekunde)",
                "معلومات النظام المباشرة (تُحدَّث كل ثانية)",
                "实时系统信息（每秒更新）",
                "リアルタイムのシステム情報（1秒ごとに更新）",
                "실시간 시스템 정보(1초마다 갱신)" } },
            { "hwCpu", new[] {
                "Processor (CPU)",
                "İşlemci (CPU)",
                "Prozessor (CPU)",
                "Procesador (CPU)",
                "Processeur (CPU)",
                "Processore (CPU)",
                "Procesor (CPU)",
                "Processador (CPU)",
                "Επεξεργαστής (CPU)",
                "Процессор (CPU)",
                "Procesor (CPU)",
                "المعالج (CPU)",
                "处理器 (CPU)",
                "プロセッサ (CPU)",
                "프로세서(CPU)" } },
            { "hwGpu", new[] {
                "Graphics Card (GPU)",
                "Ekran Kartı (GPU)",
                "Grafikkarte (GPU)",
                "Tarjeta gráfica (GPU)",
                "Carte graphique (GPU)",
                "Scheda video (GPU)",
                "Karta graficzna (GPU)",
                "Placa de vídeo (GPU)",
                "Κάρτα γραφικών (GPU)",
                "Видеокарта (GPU)",
                "Grafička kartica (GPU)",
                "بطاقة الرسومات (GPU)",
                "显卡 (GPU)",
                "グラフィックスカード (GPU)",
                "그래픽 카드(GPU)" } },
            { "hwBoard", new[] {
                "Motherboard",
                "Anakart",
                "Mainboard",
                "Placa base",
                "Carte mère",
                "Scheda madre",
                "Płyta główna",
                "Placa-mãe",
                "Μητρική πλακέτα",
                "Материнская плата",
                "Matična ploča",
                "اللوحة الأم",
                "主板",
                "マザーボード",
                "메인보드" } },
            { "hwRam", new[] {
                "Memory (RAM)",
                "Bellek (RAM)",
                "Arbeitsspeicher (RAM)",
                "Memoria (RAM)",
                "Mémoire (RAM)",
                "Memoria (RAM)",
                "Pamięć (RAM)",
                "Memória (RAM)",
                "Μνήμη (RAM)",
                "Память (ОЗУ)",
                "Memorija (RAM)",
                "الذاكرة (RAM)",
                "内存 (RAM)",
                "メモリ (RAM)",
                "메모리(RAM)" } },
            { "hwUsage", new[] {
                "Current usage",
                "Anlık kullanım",
                "Aktuelle Auslastung",
                "Uso actual",
                "Utilisation actuelle",
                "Utilizzo attuale",
                "Bieżące użycie",
                "Uso atual",
                "Τρέχουσα χρήση",
                "Текущая загрузка",
                "Trenutna upotreba",
                "الاستخدام الحالي",
                "当前使用率",
                "現在の使用率",
                "현재 사용량" } },
            { "hwThreads", new[] {
                "{0} logical processors",
                "{0} mantıksal işlemci",
                "{0} logische Prozessoren",
                "{0} procesadores lógicos",
                "{0} processeurs logiques",
                "{0} processori logici",
                "Procesory logiczne: {0}",
                "{0} processadores lógicos",
                "{0} λογικοί επεξεργαστές",
                "Логических процессоров: {0}",
                "Logičkih procesora: {0}",
                "{0} معالجات منطقية",
                "{0} 个逻辑处理器",
                "論理プロセッサ {0} 個",
                "논리 프로세서 {0}개" } },
            { "hwRamTotal", new[] {
                "Total: {0:0.0} GB",
                "Toplam: {0:0.0} GB",
                "Gesamt: {0:0.0} GB",
                "Total: {0:0.0} GB",
                "Total : {0:0.0} Go",
                "Totale: {0:0.0} GB",
                "Razem: {0:0.0} GB",
                "Total: {0:0.0} GB",
                "Σύνολο: {0:0.0} GB",
                "Всего: {0:0.0} ГБ",
                "Ukupno: {0:0.0} GB",
                "الإجمالي: {0:0.0} GB",
                "总计：{0:0.0} GB",
                "合計: {0:0.0} GB",
                "전체: {0:0.0} GB" } },
            { "hwRamUsed", new[] {
                "In use: {0:0.0} GB   •   Available: {1:0.0} GB",
                "Kullanılan: {0:0.0} GB   •   Boş: {1:0.0} GB",
                "Belegt: {0:0.0} GB   •   Verfügbar: {1:0.0} GB",
                "En uso: {0:0.0} GB   •   Disponible: {1:0.0} GB",
                "Utilisée : {0:0.0} Go   •   Disponible : {1:0.0} Go",
                "In uso: {0:0.0} GB   •   Disponibile: {1:0.0} GB",
                "Używane: {0:0.0} GB   •   Dostępne: {1:0.0} GB",
                "Em uso: {0:0.0} GB   •   Disponível: {1:0.0} GB",
                "Σε χρήση: {0:0.0} GB   •   Διαθέσιμα: {1:0.0} GB",
                "Используется: {0:0.0} ГБ   •   Доступно: {1:0.0} ГБ",
                "U upotrebi: {0:0.0} GB   •   Dostupno: {1:0.0} GB",
                "قيد الاستخدام: {0:0.0} GB   •   المتاح: {1:0.0} GB",
                "已用：{0:0.0} GB   •   可用：{1:0.0} GB",
                "使用中: {0:0.0} GB   •   空き: {1:0.0} GB",
                "사용 중: {0:0.0} GB   •   사용 가능: {1:0.0} GB" } },
            { "hwUnknown", new[] {
                "Unknown",
                "Bilinmiyor",
                "Unbekannt",
                "Desconocido",
                "Inconnu",
                "Sconosciuto",
                "Nieznane",
                "Desconhecido",
                "Άγνωστο",
                "Неизвестно",
                "Nepoznato",
                "غير معروف",
                "未知",
                "不明",
                "알 수 없음" } },
            { "hwDetails", new[] {
                "Details",
                "Detaylar",
                "Details",
                "Detalles",
                "Détails",
                "Dettagli",
                "Szczegóły",
                "Detalhes",
                "Λεπτομέρειες",
                "Подробности",
                "Detalji",
                "التفاصيل",
                "详细信息",
                "詳細",
                "세부 정보" } },
            { "hwNA", new[] {
                "Usage data is not available on this system",
                "Bu sistemde kullanım verisi alınamıyor",
                "Auslastungsdaten auf diesem System nicht verfügbar",
                "Los datos de uso no están disponibles en este sistema",
                "Données d'utilisation indisponibles sur ce système",
                "I dati di utilizzo non sono disponibili su questo sistema",
                "Dane o użyciu są niedostępne w tym systemie",
                "Os dados de uso não estão disponíveis neste sistema",
                "Τα δεδομένα χρήσης δεν είναι διαθέσιμα σε αυτό το σύστημα",
                "Данные о загрузке недоступны в этой системе",
                "Podaci o upotrebi nisu dostupni na ovom sustavu",
                "بيانات الاستخدام غير متاحة على هذا النظام",
                "此系统无法获取使用率数据",
                "このシステムでは使用率データを取得できません",
                "이 시스템에서는 사용량 데이터를 가져올 수 없습니다" } },
            { "pdTitle", new[] {
                "Process Usage",
                "Süreç Kullanımı",
                "Prozessauslastung",
                "Uso por proceso",
                "Utilisation par processus",
                "Utilizzo dei processi",
                "Użycie przez procesy",
                "Uso por processo",
                "Χρήση διεργασιών",
                "Использование процессами",
                "Upotreba po procesima",
                "استخدام العمليات",
                "进程使用情况",
                "プロセス使用状況",
                "프로세스 사용량" } },
            { "pdSub", new[] {
                "All running processes, highest first (updates every second)",
                "Çalışan tüm süreçler, en yüksekten en aza (her saniye güncellenir)",
                "Alle laufenden Prozesse, höchste zuerst (Aktualisierung jede Sekunde)",
                "Todos los procesos en ejecución, de mayor a menor (se actualiza cada segundo)",
                "Tous les processus actifs, du plus élevé au plus bas (mise à jour chaque seconde)",
                "Tutti i processi in esecuzione, dal più alto al più basso (aggiornamento ogni secondo)",
                "Wszystkie uruchomione procesy, od największego (odświeżanie co sekundę)",
                "Todos os processos em execução, do maior para o menor (atualiza a cada segundo)",
                "Όλες οι διεργασίες που εκτελούνται, από τη μεγαλύτερη χρήση (ενημέρωση κάθε δευτερόλεπτο)",
                "Все запущенные процессы, от самых нагруженных (обновление каждую секунду)",
                "Svi pokrenuti procesi, od najvećeg (osvježava se svake sekunde)",
                "جميع العمليات قيد التشغيل، من الأعلى إلى الأقل (تُحدَّث كل ثانية)",
                "所有正在运行的进程，从高到低（每秒更新）",
                "実行中のすべてのプロセス（使用率の高い順・1秒ごとに更新）",
                "실행 중인 모든 프로세스, 높은 순(1초마다 갱신)" } },
            { "pdStats", new[] {
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0} %   •   GPU {1:0.0} %   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}",
                "CPU {0:0.0}%   •   GPU {1:0.0}%   •   RAM {2}" } },
            { "unitGb", new[] {
                "GB",
                "GB",
                "GB",
                "GB",
                "Go",
                "GB",
                "GB",
                "GB",
                "GB",
                "ГБ",
                "GB",
                "GB",
                "GB",
                "GB",
                "GB" } },
            { "unitMb", new[] {
                "MB",
                "MB",
                "MB",
                "MB",
                "Mo",
                "MB",
                "MB",
                "MB",
                "MB",
                "МБ",
                "MB",
                "MB",
                "MB",
                "MB",
                "MB" } },
            { "privacy", new[] {
                "Privacy Clean",
                "Gizlilik Temizliği",
                "Datenschutz-Bereinigung",
                "Limpieza de privacidad",
                "Nettoyage confidentialité",
                "Pulizia privacy",
                "Czyszczenie prywatności",
                "Limpeza de privacidade",
                "Εκκαθάριση απορρήτου",
                "Очистка конфиденциальности",
                "Čišćenje privatnosti",
                "تنظيف الخصوصية",
                "隐私清理",
                "プライバシークリーン",
                "개인정보 정리" } },
            { "privacyDone", new[] {
                "Privacy clean done: {0} recent items, {1} search history entries, {2} browser cache files removed ({3:0.0} MB freed).",
                "Gizlilik temizliği tamamlandı: {0} son öğe, {1} arama geçmişi kaydı, {2} tarayıcı önbellek dosyası silindi ({3:0.0} MB boşaltıldı).",
                "Datenschutz-Bereinigung abgeschlossen: {0} zuletzt verwendete Elemente, {1} Sucheinträge, {2} Browser-Cache-Dateien gelöscht ({3:0.0} MB freigegeben).",
                "Limpieza de privacidad completada: {0} elementos recientes, {1} entradas del historial de búsqueda, {2} archivos de caché del navegador eliminados ({3:0.0} MB liberados).",
                "Nettoyage de confidentialité terminé : {0} éléments récents, {1} entrées d'historique de recherche, {2} fichiers de cache navigateur supprimés ({3:0.0} Mo libérés).",
                "Pulizia privacy completata: {0} elementi recenti, {1} voci della cronologia di ricerca, {2} file della cache del browser rimossi ({3:0.0} MB liberati).",
                "Czyszczenie prywatności zakończone: usunięto {0} ostatnich elementów, {1} wpisów historii wyszukiwania, {2} plików pamięci podręcznej przeglądarki (zwolniono {3:0.0} MB).",
                "Limpeza de privacidade concluída: {0} itens recentes, {1} entradas do histórico de pesquisa, {2} arquivos de cache do navegador removidos ({3:0.0} MB liberados).",
                "Η εκκαθάριση απορρήτου ολοκληρώθηκε: διαγράφηκαν {0} πρόσφατα στοιχεία, {1} καταχωρήσεις ιστορικού αναζήτησης, {2} αρχεία προσωρινής μνήμης προγράμματος περιήγησης (ελευθερώθηκαν {3:0.0} MB).",
                "Очистка конфиденциальности завершена: удалено недавних элементов — {0}, записей истории поиска — {1}, файлов кэша браузера — {2} (освобождено {3:0.0} МБ).",
                "Čišćenje privatnosti dovršeno: uklonjeno {0} nedavnih stavki, {1} unosa povijesti pretraživanja, {2} datoteka predmemorije preglednika (oslobođeno {3:0.0} MB).",
                "اكتمل تنظيف الخصوصية: تمت إزالة {0} عنصر حديث و{1} من إدخالات سجل البحث و{2} من ملفات ذاكرة المتصفح المؤقتة (تم تحرير {3:0.0} MB).",
                "隐私清理完成：已删除 {0} 个最近项目、{1} 条搜索历史记录、{2} 个浏览器缓存文件（释放 {3:0.0} MB）。",
                "プライバシークリーンが完了しました: 最近の項目 {0} 件、検索履歴 {1} 件、ブラウザーキャッシュ {2} ファイルを削除しました（{3:0.0} MB 解放）。",
                "개인정보 정리 완료: 최근 항목 {0}개, 검색 기록 {1}개, 브라우저 캐시 파일 {2}개 삭제({3:0.0} MB 확보)." } },
            { "bloat", new[] {
                "Bloatware Manager",
                "Gereksiz Uygulamalar",
                "Überflüssige Apps",
                "Gestor de bloatware",
                "Applications inutiles",
                "Gestione bloatware",
                "Menedżer zbędnych aplikacji",
                "Gerenciador de bloatware",
                "Διαχείριση περιττών εφαρμογών",
                "Менеджер ненужных приложений",
                "Upravitelj nepotrebnih aplikacija",
                "مدير التطبيقات غير الضرورية",
                "预装软件管理器",
                "不要アプリ管理",
                "불필요한 앱 관리" } },
            { "bloatInfo", new[] {
                "{0} removable apps found",
                "{0} kaldırılabilir uygulama bulundu",
                "{0} entfernbare Apps gefunden",
                "{0} aplicaciones eliminables encontradas",
                "{0} applications supprimables trouvées",
                "{0} app rimovibili trovate",
                "Znalezione aplikacje do usunięcia: {0}",
                "{0} aplicativos removíveis encontrados",
                "Βρέθηκαν {0} εφαρμογές προς κατάργηση",
                "Найдено приложений для удаления: {0}",
                "Pronađeno aplikacija za uklanjanje: {0}",
                "تم العثور على {0} تطبيق قابل للإزالة",
                "找到 {0} 个可卸载的应用",
                "削除可能なアプリが {0} 件見つかりました",
                "제거 가능한 앱 {0}개 발견" } },
            { "bloatNone", new[] {
                "No removable apps found.",
                "Kaldırılabilir uygulama bulunamadı.",
                "Keine entfernbaren Apps gefunden.",
                "No se encontraron aplicaciones eliminables.",
                "Aucune application supprimable trouvée.",
                "Nessuna app rimovibile trovata.",
                "Nie znaleziono aplikacji do usunięcia.",
                "Nenhum aplicativo removível encontrado.",
                "Δεν βρέθηκαν εφαρμογές προς κατάργηση.",
                "Приложения для удаления не найдены.",
                "Nema aplikacija za uklanjanje.",
                "لم يتم العثور على تطبيقات قابلة للإزالة.",
                "未找到可卸载的应用。",
                "削除可能なアプリは見つかりませんでした。",
                "제거 가능한 앱이 없습니다." } },
            { "bloatRemove", new[] {
                "Remove",
                "Kaldır",
                "Entfernen",
                "Eliminar",
                "Supprimer",
                "Rimuovi",
                "Usuń",
                "Remover",
                "Κατάργηση",
                "Удалить",
                "Ukloni",
                "إزالة",
                "卸载",
                "削除",
                "제거" } },
            { "bloatRemoving", new[] {
                "Removing…",
                "Kaldırılıyor…",
                "Wird entfernt…",
                "Eliminando…",
                "Suppression…",
                "Rimozione…",
                "Usuwanie…",
                "Removendo…",
                "Κατάργηση…",
                "Удаление…",
                "Uklanjanje…",
                "جارٍ الإزالة…",
                "正在卸载…",
                "削除中…",
                "제거 중…" } },
            { "bloatRemoved", new[] {
                "Removed",
                "Kaldırıldı",
                "Entfernt",
                "Eliminada",
                "Supprimée",
                "Rimossa",
                "Usunięto",
                "Removido",
                "Καταργήθηκε",
                "Удалено",
                "Uklonjeno",
                "تمت الإزالة",
                "已卸载",
                "削除済み",
                "제거됨" } },
            { "bloatRemovedLog", new[] {
                "Removed app: {0}",
                "Uygulama kaldırıldı: {0}",
                "App entfernt: {0}",
                "Aplicación eliminada: {0}",
                "Application supprimée : {0}",
                "App rimossa: {0}",
                "Usunięto aplikację: {0}",
                "Aplicativo removido: {0}",
                "Καταργήθηκε η εφαρμογή: {0}",
                "Приложение удалено: {0}",
                "Aplikacija uklonjena: {0}",
                "تمت إزالة التطبيق: {0}",
                "已卸载应用：{0}",
                "アプリを削除しました: {0}",
                "앱을 제거했습니다: {0}" } },
            { "bloatFail", new[] {
                "Could not remove {0}: {1}",
                "{0} kaldırılamadı: {1}",
                "{0} konnte nicht entfernt werden: {1}",
                "No se pudo eliminar {0}: {1}",
                "Impossible de supprimer {0} : {1}",
                "Impossibile rimuovere {0}: {1}",
                "Nie można usunąć {0}: {1}",
                "Não foi possível remover {0}: {1}",
                "Δεν ήταν δυνατή η κατάργηση του {0}: {1}",
                "Не удалось удалить {0}: {1}",
                "Nije moguće ukloniti {0}: {1}",
                "تعذّرت إزالة {0}: {1}",
                "无法卸载 {0}：{1}",
                "{0} を削除できませんでした: {1}",
                "{0}을(를) 제거할 수 없습니다: {1}" } },
            { "alertTitle", new[] {
                "⚠ Critical Warning",
                "⚠ Kritik Uyarı",
                "⚠ Kritische Warnung",
                "⚠ Advertencia crítica",
                "⚠ Avertissement critique",
                "⚠ Avviso critico",
                "⚠ Ostrzeżenie krytyczne",
                "⚠ Aviso crítico",
                "⚠ Κρίσιμη προειδοποίηση",
                "⚠ Критическое предупреждение",
                "⚠ Kritično upozorenje",
                "⚠ تحذير حرج",
                "⚠ 严重警告",
                "⚠ 重大な警告",
                "⚠ 중요 경고" } },
            { "alertBody", new[] {
                "Caution: Removing system components may affect system stability. Please make sure you know what you are doing.",
                "Dikkat: Sistem bileşenlerini silmek sistem kararlılığını etkileyebilir, lütfen ne yaptığınızdan emin olun.",
                "Achtung: Das Löschen von Systemkomponenten kann die Systemstabilität beeinträchtigen. Bitte stellen Sie sicher, dass Sie wissen, was Sie tun.",
                "Precaución: eliminar componentes del sistema puede afectar a su estabilidad. Asegúrate de saber lo que haces.",
                "Attention : la suppression de composants système peut affecter la stabilité du système. Assurez-vous de savoir ce que vous faites.",
                "Attenzione: la rimozione di componenti di sistema può compromettere la stabilità del sistema. Assicurati di sapere cosa stai facendo.",
                "Uwaga: usuwanie składników systemu może wpłynąć na jego stabilność. Upewnij się, że wiesz, co robisz.",
                "Atenção: remover componentes do sistema pode afetar a estabilidade do sistema. Certifique-se de que sabe o que está fazendo.",
                "Προσοχή: η κατάργηση στοιχείων του συστήματος ενδέχεται να επηρεάσει τη σταθερότητά του. Βεβαιωθείτε ότι γνωρίζετε τι κάνετε.",
                "Внимание: удаление системных компонентов может повлиять на стабильность системы. Убедитесь, что вы понимаете, что делаете.",
                "Oprez: uklanjanje sistemskih komponenti može utjecati na stabilnost sustava. Provjerite znate li što radite.",
                "تنبيه: قد تؤثر إزالة مكوّنات النظام على استقراره. يرجى التأكد من أنك تعرف ما تفعله.",
                "注意：删除系统组件可能会影响系统稳定性。请确保您清楚自己在做什么。",
                "注意: システムコンポーネントを削除すると、システムの安定性に影響する可能性があります。操作内容をよく確認してください。",
                "주의: 시스템 구성 요소를 삭제하면 시스템 안정성에 영향을 줄 수 있습니다. 무엇을 하는지 확실히 확인하세요." } },
            { "alertRemoveHead", new[] {
                "You are about to remove: {0}",
                "Kaldırılacak uygulama: {0}",
                "Folgende App wird entfernt: {0}",
                "Vas a eliminar: {0}",
                "Vous allez supprimer : {0}",
                "Stai per rimuovere: {0}",
                "Za chwilę usuniesz: {0}",
                "Você está prestes a remover: {0}",
                "Πρόκειται να καταργήσετε: {0}",
                "Вы собираетесь удалить: {0}",
                "Uskoro ćete ukloniti: {0}",
                "أنت على وشك إزالة: {0}",
                "您即将卸载：{0}",
                "次のアプリを削除しようとしています: {0}",
                "제거하려는 앱: {0}" } },
            { "alertContinue", new[] {
                "Continue",
                "Devam Et",
                "Fortfahren",
                "Continuar",
                "Continuer",
                "Continua",
                "Kontynuuj",
                "Continuar",
                "Συνέχεια",
                "Продолжить",
                "Nastavi",
                "متابعة",
                "继续",
                "続行",
                "계속" } },
            { "alertCancel", new[] {
                "Cancel",
                "İptal",
                "Abbrechen",
                "Cancelar",
                "Annuler",
                "Annulla",
                "Anuluj",
                "Cancelar",
                "Άκυρο",
                "Отмена",
                "Odustani",
                "إلغاء",
                "取消",
                "キャンセル",
                "취소" } }
        };

        private static readonly string[] OpKeys = { "temp", "dns", "ram", "bin", "startup", "privacy", "hw", "bloat" };
        private static readonly string[] OpIcons = { "🧹", "🌐", "🚀", "🗑", "📋", "🛡", "🖥", "📦" };
        private int lang;                 // 0 = English (varsayılan)
        private bool darkMode = true;
        private bool busy;
        private Theme theme = Theme.Dark;
        private Func<string>[] ops = new Func<string>[0];
        private HardwareForm hwForm;
        private readonly Panel titleBar = new Panel();
        private readonly Label lblTitle = new Label();
        private readonly PictureBox logoBox = new PictureBox();
        private readonly Label lblSub = new Label();
        private readonly RoundButton themeBtn = new RoundButton();
        private readonly RoundSelect langSel = new RoundSelect();
        private readonly RoundButton minBtn = new RoundButton();
        private readonly RoundButton closeBtn = new RoundButton();
        private readonly RoundButton quickBtn = new RoundButton();
        private readonly RoundButton[] toolBtns = new RoundButton[8];
        private readonly SmoothBar bar = new SmoothBar();
        private readonly Label lblLog = new Label();
        private readonly RoundPanel logCard = new RoundPanel();
        private readonly RichTextBox logBox = new RichTextBox();
        private readonly RoundScrollBar logScroll = new RoundScrollBar();
        private readonly System.Windows.Forms.Timer logTimer = new System.Windows.Forms.Timer();
        public Form1()
        {
            Text = "Alaz PC Optimizer";
            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;   // Alaz logosu (görev çubuğu / Alt-Tab)
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(900, 640);
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            ops = new Func<string>[] { CleanTemp, FlushDns, OptimizeRam, EmptyRecycleBin, ReadStartup, PrivacyClean };

            lang = 0;          // ilk açılışta İngilizce + koyu mod
            LoadSettings();    // daha önce seçilenler varsa onları yükle

            BuildUi();

            langSel.SelectedIndex = lang;   // dinleyici eklenmeden önce, olay tetiklemez
            langSel.SelectedIndexChanged += delegate
            {
                lang = langSel.SelectedIndex;
                ApplyLanguage();
                SaveSettings();
            };

            Resize += delegate { UpdateRegion(); };
            Load += delegate { UpdateRegion(); };
            Application.AddMessageFilter(this);   // Activity Log için fare tekerleği
            FormClosed += delegate { Application.RemoveMessageFilter(this); logTimer.Dispose(); };

            ApplyTheme();
            ApplyLanguage();
            Log(T("ready"));
        }
        private void BuildUi()
        {
            SuspendLayout();
            titleBar.SetBounds(0, 0, 900, 70);
            titleBar.MouseDown += DragForm;
            logoBox.SetBounds(22, 9, 38, 52);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoBox.BackColor = Color.Transparent;
            logoBox.MouseDown += DragForm;

            lblTitle.Text = "Alaz PC Optimizer";
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(68, 8);
            lblTitle.MouseDown += DragForm;

            lblSub.Font = new Font("Segoe UI", 9F);
            lblSub.AutoSize = true;
            lblSub.Location = new Point(72, 44);
            lblSub.MouseDown += DragForm;

            themeBtn.SetBounds(396, 18, 192, 34);
            themeBtn.Radius = 17;
            themeBtn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            themeBtn.Click += delegate
            {
                darkMode = !darkMode;
                theme = darkMode ? Theme.Dark : Theme.Light;
                ApplyTheme();
                ApplyLanguage();
                SaveSettings();
            };

            langSel.Items = LangNames;
            langSel.SetBounds(600, 18, 184, 34);
            langSel.Radius = 17;

            minBtn.SetBounds(796, 18, 38, 34);
            minBtn.Radius = 12;
            minBtn.Text = "—";
            minBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            minBtn.Click += delegate { WindowState = FormWindowState.Minimized; };

            closeBtn.SetBounds(842, 18, 38, 34);
            closeBtn.Radius = 12;
            closeBtn.Text = "✕";
            closeBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            closeBtn.Click += delegate { Close(); };

            titleBar.Controls.Add(logoBox);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(lblSub);
            titleBar.Controls.Add(themeBtn);
            titleBar.Controls.Add(langSel);
            titleBar.Controls.Add(minBtn);
            titleBar.Controls.Add(closeBtn);
            quickBtn.SetBounds(30, 80, 840, 64);
            quickBtn.Radius = 24;
            quickBtn.UseAccent = true;
            quickBtn.SubText = "";   // ApplyLanguage doldurur ("All in one")
            quickBtn.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            quickBtn.Click += async delegate { await RunQuickBoost(); };
            int w = 204, gap = 8, bh = 84;
            for (int i = 0; i < toolBtns.Length; i++)
            {
                RoundButton b = new RoundButton();
                b.SetBounds(30 + (i % 4) * (w + gap), 158 + (i / 4) * (bh + gap), w, bh);
                b.Radius = 20;
                b.Icon = OpIcons[i];
                b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                int idx = i;
                b.Click += async delegate
                {
                    if (idx == 6)
                    {
                        ShowHardwareMonitor();   // Donanım bilgileri penceresi
                        return;
                    }
                    if (idx == 7)
                    {
                        await ShowBloatManager();   // Gereksiz uygulamalar penceresi
                        return;
                    }
                    if (busy) return;
                    await RunSingle(idx);
                    if (idx == 4) ShowStartupManager();   // Başlangıç programları penceresi
                };
                toolBtns[i] = b;
                Controls.Add(b);
            }
            bar.SetBounds(30, 348, 840, 28);
            bar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLog.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLog.AutoSize = true;
            lblLog.Location = new Point(32, 388);

            logCard.SetBounds(30, 418, 840, 202);
            logCard.Radius = 20;

            logBox.ReadOnly = true;
            logBox.BorderStyle = BorderStyle.None;
            logBox.Font = new Font("Consolas", 10F);
            logBox.ScrollBars = RichTextBoxScrollBars.None;   // özel kaydırma çubuğu kullanılıyor
            logBox.DetectUrls = false;
            logBox.TabStop = false;
            logBox.SetBounds(16, 14, 780, 174);

            logScroll.SetBounds(808, 14, 12, 174);
            logScroll.Scrolled += delegate { OnLogScrolled(); };

            logCard.Controls.Add(logBox);
            logCard.Controls.Add(logScroll);

            logTimer.Interval = 80;
            logTimer.Tick += delegate { SyncLogScroll(); };
            logTimer.Start();

            Controls.Add(titleBar);
            Controls.Add(quickBtn);
            Controls.Add(bar);
            Controls.Add(lblLog);
            Controls.Add(logCard);

            ResumeLayout(false);
        }
        private const int WM_MOUSEWHEEL = 0x020A;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL) return false;
            if (IsDisposed || !IsHandleCreated || !logBox.IsHandleCreated) return false;
            if (Form.ActiveForm != this) return false;   // alt pencere açıksa karışma
            if (!logCard.RectangleToScreen(logCard.ClientRectangle).Contains(Cursor.Position)) return false;

            int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
            int lines = -(int)Math.Round(delta / 120.0 * 3);
            if (lines == 0) lines = delta > 0 ? -1 : 1;
            SendMessage(logBox.Handle, EM_LINESCROLL, 0, lines);
            SyncLogScroll();
            return true;
        }
        private void OnLogScrolled()
        {
            if (!logBox.IsHandleCreated) return;

            if (logScroll.Value >= logScroll.MaxValue)
            {
                logBox.SelectionStart = logBox.TextLength;
                logBox.SelectionLength = 0;
                logBox.ScrollToCaret();
                return;
            }

            int first = SendMessage(logBox.Handle, EM_GETFIRSTVISIBLELINE, 0, 0).ToInt32();
            int delta = logScroll.Value - first;
            if (delta != 0) SendMessage(logBox.Handle, EM_LINESCROLL, 0, delta);
        }
        private void SyncLogScroll()
        {
            if (!logBox.IsHandleCreated || logScroll.IsDragging) return;
            int total = SendMessage(logBox.Handle, EM_GETLINECOUNT, 0, 0).ToInt32();
            int first = SendMessage(logBox.Handle, EM_GETFIRSTVISIBLELINE, 0, 0).ToInt32();
            int lineH = Math.Max(8, logBox.Font.Height);
            int visible = Math.Max(1, logBox.ClientSize.Height / lineH);
            logScroll.SetState(total, visible, first);
        }

        private void ShowStartupManager()
        {
            using (StartupForm f = new StartupForm(theme, delegate (string k, object[] a) { return T(k, a); },
                                                   delegate (string m) { Log(m); }))
            {
                f.ShowDialog(this);
            }
        }
        private async Task ShowBloatManager()
        {
            if (busy) return;

            bool go = AlertForm.Ask(this, theme, T("alertTitle"), T("alertBody"), T("alertContinue"), T("alertCancel"));
            if (!go) return;

            SetBusy(true);
            List<BloatApp> apps = new List<BloatApp>();
            try
            {
                Log(T("start", T("bloat")));
                apps = await Task.Run(() => BloatApp.LoadInstalled());
                if (apps.Count == 0) Log(T("bloatNone"));
            }
            catch (Exception ex)
            {
                Log(T("error", ex.Message));
            }
            finally
            {
                SetBusy(false);
            }

            using (BloatForm f = new BloatForm(theme, delegate (string k, object[] a) { return T(k, a); },
                                               delegate (string m) { Log(m); }, apps))
            {
                f.ShowDialog(this);
            }
        }

        private void ShowHardwareMonitor()
        {
            if (hwForm != null && !hwForm.IsDisposed)
            {
                hwForm.Activate();
                return;
            }

            hwForm = new HardwareForm(theme, delegate (string k, object[] a) { return T(k, a); });
            hwForm.FormClosed += delegate { hwForm = null; };
            hwForm.PlaceOver(this);
            hwForm.Show(this);   // modal değil: tema/dil değişince pencere anında güncellenir
        }

        private void DragForm(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void UpdateRegion()
        {
            if (Width < 10 || Height < 10) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), 26f))
            {
                Region = new Region(p);
            }
        }
        private static void StyleButton(RoundButton b, Theme t)
        {
            b.Normal = t.Btn;
            b.Hover = t.BtnHover;
            b.Pressed = t.BtnPressed;
            b.Border = t.Border;
            b.Fore = t.Text;
            b.UseAccent = false;
        }

        private void ApplyTheme()
        {
            Theme t = theme;

            BackColor = t.Bg;
            titleBar.BackColor = t.Bg;
            logoBox.Image = (t == Theme.Dark) ? AlazLogos.DarkTitle : AlazLogos.LightTitle;
            lblTitle.ForeColor = t.Text;
            lblSub.ForeColor = t.Sub;
            lblLog.ForeColor = t.Text;

            foreach (RoundButton b in toolBtns) StyleButton(b, t);
            StyleButton(themeBtn, t);
            StyleButton(minBtn, t);
            StyleButton(closeBtn, t);
            closeBtn.Hover = Color.FromArgb(232, 65, 80);
            closeBtn.Pressed = Color.FromArgb(190, 45, 60);

            quickBtn.UseAccent = true;
            quickBtn.Accent1 = t.Accent;
            quickBtn.Accent2 = t.Accent2;

            langSel.Normal = t.Btn;
            langSel.Hover = t.BtnHover;
            langSel.Border = t.Border;
            langSel.Fore = t.Text;
            langSel.PopupBg = t.Card;
            langSel.ItemHover = t.BtnHover;
            langSel.Accent1 = t.Accent;
            langSel.Accent2 = t.Accent2;

            bar.Track = t.Track;
            bar.Fill1 = t.Accent;
            bar.Fill2 = t.Accent2;
            bar.Fore = t.Text;

            logCard.BackColor = t.LogBg;
            logBox.BackColor = t.LogBg;
            logBox.ForeColor = t.LogFore;
            logBox.SelectAll();
            logBox.SelectionColor = t.LogFore;
            logBox.Select(logBox.TextLength, 0);

            logScroll.TrackColor = Color.FromArgb(40, t.LogFore);
            logScroll.ThumbColor = Color.FromArgb(150, t.LogFore);
            logScroll.HotColor = t.LogFore;

            if (hwForm != null && !hwForm.IsDisposed) hwForm.ApplyTheme(theme);

            Invalidate(true);
        }

        private void ApplyLanguage()
        {
            lblSub.Text = T("subtitle");
            quickBtn.Text = T("quick");
            quickBtn.SubText = T("quickSub");
            for (int i = 0; i < toolBtns.Length; i++)
            {
                toolBtns[i].Text = T(OpKeys[i]);
            }
            lblLog.Text = "📜 " + T("log");
            themeBtn.Text = darkMode ? T("themeLight") : T("themeDark");

            foreach (RoundButton b in toolBtns) b.Invalidate();
            quickBtn.Invalidate();
            themeBtn.Invalidate();

            if (hwForm != null && !hwForm.IsDisposed) hwForm.ApplyLanguage();
        }
        private const string SettingsKey = @"Software\AlazPCOptimizer";

        private void LoadSettings()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(SettingsKey))
                {
                    if (k == null) return;
                    string ln = k.GetValue("Language") as string;
                    if (!string.IsNullOrEmpty(ln))
                    {
                        int idx = Array.IndexOf(LangNames, ln);
                        if (idx >= 0) lang = idx;
                    }
                    object dm = k.GetValue("DarkMode");
                    if (dm != null)
                    {
                        darkMode = Convert.ToString(dm, CultureInfo.InvariantCulture) != "0";
                        theme = darkMode ? Theme.Dark : Theme.Light;
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(SettingsKey))
                {
                    k.SetValue("Language", LangNames[lang]);
                    k.SetValue("DarkMode", darkMode ? 1 : 0);
                }
            }
            catch { }
        }

        private string T(string key, params object[] args)
        {
            string[] arr;
            if (!Strings.TryGetValue(key, out arr)) return key;
            string s = lang < arr.Length ? arr[lang] : arr[0];
            return (args == null || args.Length == 0) ? s : string.Format(s, args);
        }
        private void Log(string msg)
        {
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = theme.LogFore;
            logBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + Environment.NewLine);
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        }

        private void SetBusy(bool value)
        {
            busy = value;
            quickBtn.Enabled = !value;
            foreach (RoundButton b in toolBtns) b.Enabled = !value;
        }
        private async Task RunStep(Func<string> work, int from, int to)
        {
            bar.SetTarget(from + (int)((to - from) * 0.8));
            string result = await Task.Run(work);
            await bar.AnimateToAsync(to);

            foreach (string line in result.Split('\n'))
            {
                if (line.Length > 0) Log(line);
            }
        }

        private async Task RunSingle(int idx)
        {
            if (busy) return;
            SetBusy(true);
            try
            {
                bar.Reset();                       // her tıklamada 0'dan başla
                Log(T("start", T(OpKeys[idx])));
                await RunStep(ops[idx], 0, 100);   // 0 -> 100
            }
            catch (Exception ex)
            {
                Log(T("error", ex.Message));
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task RunQuickBoost()
        {
            if (busy) return;
            SetBusy(true);
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                bar.Reset();
                Log(T("boostStart"));
                int count = QuickCount;
                for (int i = 0; i < count; i++)
                {
                    int from = i * 100 / count;
                    int to = (i + 1) * 100 / count;
                    Log(T("start", T(OpKeys[i])));
                    await RunStep(ops[i], from, to);
                }
                await bar.AnimateToAsync(100);
                sw.Stop();
                Log(T("boostDone", sw.Elapsed.TotalSeconds));
            }
            catch (Exception ex)
            {
                Log(T("error", ex.Message));
            }
            finally
            {
                SetBusy(false);
            }
        }
        private string CleanTemp()
        {
            int files = 0;
            long bytes = 0;
            try
            {
                CleanDir(new DirectoryInfo(Path.GetTempPath()), ref files, ref bytes);
            }
            catch (Exception ex)
            {
                return T("error", ex.Message);
            }
            return T("tempDone", files, bytes / 1048576.0);
        }

        private static void CleanDir(DirectoryInfo dir, ref int files, ref long bytes)
        {
            try
            {
                foreach (FileInfo f in dir.GetFiles())
                {
                    try
                    {
                        long len = f.Length;
                        f.Delete();
                        files++;
                        bytes += len;
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                foreach (DirectoryInfo d in dir.GetDirectories())
                {
                    if ((d.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                    CleanDir(d, ref files, ref bytes);
                    try { d.Delete(false); } catch { }
                }
            }
            catch { }
        }

        private string FlushDns()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("ipconfig", "/flushdns");
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                using (Process p = Process.Start(psi))
                {
                    if (p == null) return T("dnsFail", "process");
                    p.StandardOutput.ReadToEnd();
                    p.StandardError.ReadToEnd();
                    p.WaitForExit(10000);
                    if (p.HasExited && p.ExitCode == 0) return T("dnsDone");
                    return T("dnsFail", "exit code " + (p.HasExited ? p.ExitCode.ToString() : "timeout"));
                }
            }
            catch (Exception ex)
            {
                return T("dnsFail", ex.Message);
            }
        }

        private string OptimizeRam()
        {
            long before = 0, after = 0;
            try
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    before = p.WorkingSet64;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    try { SetProcessWorkingSetSize(p.Handle, (IntPtr)(-1), (IntPtr)(-1)); } catch { }

                    p.Refresh();
                    after = p.WorkingSet64;
                }
            }
            catch (Exception ex)
            {
                return T("error", ex.Message);
            }
            double freed = Math.Max(0L, before - after) / 1048576.0;
            return T("ramDone", freed);
        }

        private string EmptyRecycleBin()
        {
            try
            {
                int hr = SHEmptyRecycleBin(IntPtr.Zero, IntPtr.Zero,
                    SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

                if (hr == 0) return T("binDone");
                if (hr == unchecked((int)0x8000FFFF)) return T("binEmpty"); // E_UNEXPECTED: zaten boş
                return T("binFail", hr);
            }
            catch (Exception ex)
            {
                return T("error", ex.Message);
            }
        }

        private string ReadStartup()
        {
            List<StartupEntry> items;
            try { items = StartupEntry.LoadAll(); }
            catch (Exception ex) { return T("error", ex.Message); }

            if (items.Count == 0) return T("startupNone");

            List<string> lines = new List<string>();
            lines.Add(T("startupHead", items.Count));
            foreach (StartupEntry en in items)
            {
                lines.Add("  " + (en.Enabled ? "✔" : "✖") + " [" + en.Tag + "] " + en.Name);
            }
            return string.Join("\n", lines.ToArray());
        }
        private string PrivacyClean()
        {
            int recent = 0;
            int searches = 0;
            int cache = 0;
            long bytes = 0;

            try
            {
                string recentDir = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                if (!string.IsNullOrEmpty(recentDir) && Directory.Exists(recentDir))
                {
                    CleanFilesOnly(new DirectoryInfo(recentDir), ref recent, ref bytes);
                }
                searches += ClearRegistryHistory(@"Software\Microsoft\Windows\CurrentVersion\Explorer\WordWheelQuery", "MRUListEx");
                searches += ClearRegistryHistory(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", "MRUList");
                foreach (string dir in GetBrowserCacheDirs())
                {
                    CleanFilesOnly(new DirectoryInfo(dir), ref cache, ref bytes);
                }
            }
            catch (Exception ex)
            {
                return T("error", ex.Message);
            }

            return T("privacyDone", recent, searches, cache, bytes / 1048576.0);
        }
        private static void CleanFilesOnly(DirectoryInfo dir, ref int files, ref long bytes)
        {
            try
            {
                foreach (FileInfo f in dir.GetFiles())
                {
                    try
                    {
                        long len = f.Length;
                        f.Delete();
                        files++;
                        bytes += len;
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                foreach (DirectoryInfo d in dir.GetDirectories())
                {
                    if ((d.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                    CleanFilesOnly(d, ref files, ref bytes);
                }
            }
            catch { }
        }
        private static int ClearRegistryHistory(string subKey, string orderValue)
        {
            int count = 0;
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(subKey, true))
                {
                    if (rk == null) return 0;

                    foreach (string name in rk.GetValueNames())
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        bool isOrder = string.Equals(name, orderValue, StringComparison.OrdinalIgnoreCase);
                        try
                        {
                            rk.DeleteValue(name, false);
                            if (!isOrder) count++;
                        }
                        catch { }
                    }
                }
            }
            catch { }
            return count;
        }
        private static List<string> GetBrowserCacheDirs()
        {
            List<string> dirs = new List<string>();
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(local)) return dirs;

            string[] roots =
            {
                Path.Combine(local, @"Google\Chrome\User Data"),
                Path.Combine(local, @"Microsoft\Edge\User Data"),
                Path.Combine(local, @"BraveSoftware\Brave-Browser\User Data"),
                Path.Combine(local, @"Vivaldi\User Data")
            };
            string[] subs = { "Cache", "Code Cache", "GPUCache", @"Service Worker\CacheStorage" };

            foreach (string root in roots)
            {
                try
                {
                    if (!Directory.Exists(root)) continue;
                    foreach (string prof in Directory.GetDirectories(root))
                    {
                        string pn = Path.GetFileName(prof);
                        if (pn != "Default" && !pn.StartsWith("Profile ", StringComparison.Ordinal)) continue;
                        foreach (string s in subs)
                        {
                            string d = Path.Combine(prof, s);
                            if (Directory.Exists(d)) dirs.Add(d);
                        }
                    }
                }
                catch { }
            }

            try
            {
                string ff = Path.Combine(local, @"Mozilla\Firefox\Profiles");
                if (Directory.Exists(ff))
                {
                    foreach (string prof in Directory.GetDirectories(ff))
                    {
                        string d = Path.Combine(prof, "cache2");
                        if (Directory.Exists(d)) dirs.Add(d);
                    }
                }
            }
            catch { }

            return dirs;
        }
    }
    internal sealed class UsageBar : Control
    {
        public Color Track = Color.Gray;
        public Color Fill1 = Color.RoyalBlue;
        public Color Fill2 = Color.DeepSkyBlue;
        public Color Fore = Color.White;

        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private float shown;
        private float target;

        public UsageBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            timer.Interval = 20;
            timer.Tick += OnTick;
            timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            float diff = target - shown;
            if (Math.Abs(diff) < 0.001f) return;
            if (Math.Abs(diff) < 0.5f) shown = target;
            else shown += diff * 0.18f;
            Invalidate();
        }

        public void SetValue(int v)
        {
            target = Math.Max(0, Math.Min(100, v));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) timer.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            if (Width < 6 || Height < 6) return;

            using (GraphicsPath tp = Gfx.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f), Height / 2f))
            using (SolidBrush tb = new SolidBrush(Track))
            {
                g.FillPath(tb, tp);
            }

            float w = (Width - 2f) * shown / 100f;
            if (w >= 2f)
            {
                using (GraphicsPath fp = Gfx.RoundRect(new RectangleF(1f, 1f, w, Height - 3f), (Height - 3f) / 2f))
                using (LinearGradientBrush fb = new LinearGradientBrush(new RectangleF(1f, 1f, Width - 2f, Height - 3f), Fill1, Fill2, LinearGradientMode.Horizontal))
                {
                    g.FillPath(fb, fp);
                }
            }

            string txt = ((int)Math.Round(shown)).ToString(CultureInfo.InvariantCulture) + "%";
            TextRenderer.DrawText(g, txt, Font, ClientRectangle, shown > 50f ? Color.White : Fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
    internal sealed class HardwareForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct FileTimeRaw
        {
            public uint Low;
            public uint High;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemStatus
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetSystemTimes(out FileTimeRaw idle, out FileTimeRaw kernel, out FileTimeRaw user);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemStatus buffer);

        private Theme t;
        private readonly Func<string, object[], string> tr;
        private readonly float k;                       // DPI ölçeği
        private readonly string cpuName;
        private readonly string boardName;
        private readonly string[] gpuNames;
        private readonly int cpuCount;
        private ulong prevIdle;
        private ulong prevTotal;
        private bool havePrev;
        private int cpuPct;
        private double ramTotalGb;
        private double ramUsedGb;
        private double ramFreeGb;
        private int ramPct;
        private readonly Label lblTitle = new Label();
        private readonly Label lblInfo = new Label();
        private readonly RoundButton closeX = new RoundButton();

        private readonly RoundPanel cpuCard = new RoundPanel();
        private readonly RoundPanel gpuCard = new RoundPanel();
        private readonly RoundPanel boardCard = new RoundPanel();
        private readonly RoundPanel ramCard = new RoundPanel();

        private readonly IconTile cpuTile = new IconTile();
        private readonly IconTile gpuTile = new IconTile();
        private readonly IconTile boardTile = new IconTile();
        private readonly IconTile ramTile = new IconTile();

        private readonly Label lblCpuCap = new Label();
        private readonly Label lblCpuName = new Label();
        private readonly Label lblCpuThreads = new Label();
        private readonly Label lblCpuUsage = new Label();
        private readonly UsageBar cpuBar = new UsageBar();

        private readonly Label lblGpuCap = new Label();
        private readonly Label lblGpuName = new Label();
        private readonly Label lblGpuUsage = new Label();
        private readonly UsageBar gpuBar = new UsageBar();

        private readonly RoundButton cpuDetBtn = new RoundButton();
        private readonly RoundButton gpuDetBtn = new RoundButton();
        private readonly RoundButton ramDetBtn = new RoundButton();

        private readonly GpuMeter gpu = new GpuMeter();
        private ProcessForm procForm;
        private int gpuPct;

        private readonly Label lblBoardCap = new Label();
        private readonly Label lblBoardName = new Label();

        private readonly Label lblRamCap = new Label();
        private readonly Label lblRamTotal = new Label();
        private readonly Label lblRamUsed = new Label();
        private readonly Label lblRamUsage = new Label();
        private readonly UsageBar ramBar = new UsageBar();

        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private readonly ToolTip tip = new ToolTip();

        private int S(int v) { return (int)Math.Round(v * k); }

        public HardwareForm(Theme theme, Func<string, object[], string> translate)
        {
            t = theme;
            tr = translate;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                k = g.DpiX / 96f;
            }

            cpuName = GetCpuName();
            cpuCount = Environment.ProcessorCount;
            gpuNames = GetGpuNames();
            boardName = GetBoardName();

            ulong idle, total;
            if (ReadCpuTimes(out idle, out total))
            {
                prevIdle = idle;
                prevTotal = total;
                havePrev = true;
            }
            SampleRam();

            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;   // Alaz logosu (görev çubuğu / Alt-Tab)
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            BackColor = t.Bg;
            KeyPreview = true;

            BuildUi();
            ApplyTheme(t);
            ApplyLanguage();

            timer.Interval = 500;   // ilk ölçüm hızlı gelsin, sonra 1 sn
            timer.Tick += OnTimer;
            timer.Start();

            KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            Resize += delegate { ApplyRegion(); };
            Load += delegate { ApplyRegion(); };
            FormClosed += delegate { timer.Stop(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                tip.Dispose();
                gpu.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SetupDetailButton(RoundButton b, Control parent, int metric)
        {
            b.SetBounds(S(498), S(10), S(100), S(26));
            b.Radius = S(13);
            b.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            int m = metric;
            b.Visible = false;   // Detaylar butonu kaldırıldı (kontrol yalnızca içeride tutuluyor)
        }

        private void ShowDetails(int metric)
        {
            if (procForm != null && !procForm.IsDisposed)
            {
                procForm.SetMetric(metric);
                procForm.Activate();
                return;
            }

            procForm = new ProcessForm(t, tr, gpu, metric);
            procForm.FormClosed += delegate { procForm = null; };
            procForm.PlaceNextTo(this);
            procForm.Show(this);
        }
        public void PlaceOver(Form parent)
        {
            StartPosition = FormStartPosition.Manual;
            int x = parent.Left + (parent.Width - Width) / 2;
            int y = parent.Top + S(76);
            Rectangle wa = Screen.FromControl(parent).WorkingArea;
            y = Math.Max(wa.Top, Math.Min(y, wa.Bottom - Height));
            Location = new Point(x, y);
        }

        private void ApplyRegion()
        {
            if (Width < 10 || Height < 10) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), S(26)))
            {
                Region = new Region(p);
            }
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        private void SetupLabel(Label l, Control parent, int x, int y, int w, int h, float size, bool bold)
        {
            l.AutoSize = false;
            l.SetBounds(S(x), S(y), S(w), S(h));
            l.Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
            parent.Controls.Add(l);
        }

        private void SetupCard(RoundPanel card, IconTile tile, string letter, Color fill, int y, int h)
        {
            card.SetBounds(S(24), S(y), S(612), S(h));
            card.Radius = S(18);
            tile.SetBounds(S(14), S(14), S(44), S(44));
            tile.Radius = S(14);
            tile.Fill = fill;
            tile.Letter = letter;
            card.Controls.Add(tile);
            Controls.Add(card);
        }

        private void BuildUi()
        {
            SuspendLayout();

            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(S(28), S(14));
            lblTitle.MouseDown += Drag;

            lblInfo.Font = new Font("Segoe UI", 9.5F);
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(S(31), S(54));
            lblInfo.MouseDown += Drag;

            closeX.SetBounds(S(598), S(18), S(38), S(34));
            closeX.Radius = S(12);
            closeX.Text = "✕";
            closeX.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            closeX.Click += delegate { Close(); };

            Controls.Add(lblTitle);
            Controls.Add(lblInfo);
            Controls.Add(closeX);

            int y = 96;
            SetupCard(cpuCard, cpuTile, "⚙", Color.FromArgb(99, 102, 241), y, 136);
            SetupLabel(lblCpuCap, cpuCard, 70, 12, 420, 18, 8.5F, true);
            SetupLabel(lblCpuName, cpuCard, 70, 32, 420, 24, 10.5F, true);
            SetupDetailButton(cpuDetBtn, cpuCard, 0);
            lblCpuName.AutoEllipsis = true;
            SetupLabel(lblCpuThreads, cpuCard, 70, 58, 524, 18, 8.5F, false);
            SetupLabel(lblCpuUsage, cpuCard, 70, 82, 524, 16, 8.5F, true);
            cpuBar.SetBounds(S(70), S(100), S(524), S(24));
            cpuCard.Controls.Add(cpuBar);
            tip.SetToolTip(lblCpuName, cpuName);
            y += 136 + 10;
            int gpuLines = Math.Max(1, gpuNames.Length);
            int gpuBarY = 56 + 22 * gpuLines;
            int gpuH = gpuBarY + 24 + 12;
            SetupCard(gpuCard, gpuTile, "🎮", Color.FromArgb(14, 165, 233), y, gpuH);
            SetupLabel(lblGpuCap, gpuCard, 70, 12, 420, 18, 8.5F, true);
            SetupLabel(lblGpuName, gpuCard, 70, 32, 420, 22 * gpuLines, 10.5F, true);
            SetupLabel(lblGpuUsage, gpuCard, 70, gpuBarY - 18, 524, 16, 8.5F, true);
            gpuBar.SetBounds(S(70), S(gpuBarY), S(524), S(24));
            gpuCard.Controls.Add(gpuBar);
            SetupDetailButton(gpuDetBtn, gpuCard, 1);
            y += gpuH + 10;
            SetupCard(boardCard, boardTile, "🔌", Color.FromArgb(16, 185, 129), y, 68);
            SetupLabel(lblBoardCap, boardCard, 70, 12, 524, 18, 8.5F, true);
            SetupLabel(lblBoardName, boardCard, 70, 32, 524, 24, 10.5F, true);
            lblBoardName.AutoEllipsis = true;
            tip.SetToolTip(lblBoardName, boardName);
            y += 68 + 10;
            SetupCard(ramCard, ramTile, "💾", Color.FromArgb(245, 158, 11), y, 136);
            SetupLabel(lblRamCap, ramCard, 70, 12, 420, 18, 8.5F, true);
            SetupLabel(lblRamTotal, ramCard, 70, 32, 420, 24, 10.5F, true);
            SetupDetailButton(ramDetBtn, ramCard, 2);
            SetupLabel(lblRamUsed, ramCard, 70, 58, 524, 18, 8.5F, false);
            SetupLabel(lblRamUsage, ramCard, 70, 82, 524, 16, 8.5F, true);
            ramBar.SetBounds(S(70), S(100), S(524), S(24));
            ramCard.Controls.Add(ramBar);
            y += 136;

            ClientSize = new Size(S(660), S(y + 18));

            MouseDown += Drag;

            ResumeLayout(false);
        }
        public void ApplyTheme(Theme th)
        {
            t = th;

            BackColor = t.Bg;
            lblTitle.ForeColor = t.Text;
            lblInfo.ForeColor = t.Sub;

            closeX.Normal = t.Btn;
            closeX.Hover = Color.FromArgb(232, 65, 80);
            closeX.Pressed = Color.FromArgb(190, 45, 60);
            closeX.Border = t.Border;
            closeX.Fore = t.Text;
            closeX.Invalidate();

            RoundPanel[] cards = { cpuCard, gpuCard, boardCard, ramCard };
            foreach (RoundPanel c in cards) c.BackColor = t.Card;

            Label[] subs = { lblCpuCap, lblCpuThreads, lblCpuUsage, lblGpuCap, lblGpuUsage, lblBoardCap, lblRamCap, lblRamUsed, lblRamUsage };
            foreach (Label l in subs) l.ForeColor = t.Sub;

            Label[] mains = { lblCpuName, lblGpuName, lblBoardName, lblRamTotal };
            foreach (Label l in mains) l.ForeColor = t.Text;

            UsageBar[] bars = { cpuBar, gpuBar, ramBar };
            foreach (UsageBar b in bars)
            {
                b.Track = t.Track;
                b.Fill1 = t.Accent;
                b.Fill2 = t.Accent2;
                b.Fore = t.Text;
                b.Invalidate();
            }

            RoundButton[] dets = { cpuDetBtn, gpuDetBtn, ramDetBtn };
            foreach (RoundButton d in dets)
            {
                d.Normal = t.Btn;
                d.Hover = t.BtnHover;
                d.Pressed = t.BtnPressed;
                d.Border = t.Border;
                d.Fore = t.Text;
                d.Invalidate();
            }

            if (procForm != null && !procForm.IsDisposed) procForm.ApplyTheme(t);

            Invalidate(true);
        }

        public void ApplyLanguage()
        {
            lblTitle.Text = "🖥 " + tr("hw", null);
            lblInfo.Text = tr("hwSub", null);
            lblCpuCap.Text = tr("hwCpu", null);
            lblGpuCap.Text = tr("hwGpu", null);
            lblBoardCap.Text = tr("hwBoard", null);
            lblRamCap.Text = tr("hwRam", null);
            lblCpuUsage.Text = tr("hwUsage", null);
            lblRamUsage.Text = tr("hwUsage", null);

            string det = "🔍 " + tr("hwDetails", null);
            cpuDetBtn.Text = det;
            gpuDetBtn.Text = det;
            ramDetBtn.Text = det;
            cpuDetBtn.Invalidate();
            gpuDetBtn.Invalidate();
            ramDetBtn.Invalidate();

            string unk = tr("hwUnknown", null);
            lblCpuName.Text = cpuName.Length > 0 ? cpuName : unk;
            lblGpuName.Text = gpuNames.Length > 0 ? string.Join(Environment.NewLine, gpuNames) : unk;
            lblBoardName.Text = boardName.Length > 0 ? boardName : unk;

            RefreshValues();

            if (procForm != null && !procForm.IsDisposed) procForm.ApplyLanguage();
        }

        private void RefreshValues()
        {
            lblCpuThreads.Text = tr("hwThreads", new object[] { cpuCount });
            lblRamTotal.Text = tr("hwRamTotal", new object[] { ramTotalGb });
            lblRamUsed.Text = tr("hwRamUsed", new object[] { ramUsedGb, ramFreeGb });
            cpuBar.SetValue(cpuPct);
            ramBar.SetValue(ramPct);

            gpuBar.Visible = gpu.Available;
            lblGpuUsage.Text = gpu.Available ? tr("hwUsage", null) : tr("hwNA", null);
            gpuBar.SetValue(gpuPct);
        }
        private void OnTimer(object sender, EventArgs e)
        {
            if (timer.Interval != 1000) timer.Interval = 1000;
            SampleCpu();
            SampleRam();
            gpu.Sample();
            gpuPct = (int)Math.Round(gpu.Total);
            RefreshValues();
        }

        private static bool ReadCpuTimes(out ulong idle, out ulong total)
        {
            FileTimeRaw i, kt, u;
            idle = 0UL;
            total = 0UL;
            if (!GetSystemTimes(out i, out kt, out u)) return false;
            idle = ((ulong)i.High << 32) | i.Low;
            ulong kernel = ((ulong)kt.High << 32) | kt.Low;   // çekirdek süresi boşta süresini de içerir
            ulong user = ((ulong)u.High << 32) | u.Low;
            total = kernel + user;
            return true;
        }

        private void SampleCpu()
        {
            ulong idle, total;
            if (!ReadCpuTimes(out idle, out total)) return;

            if (havePrev)
            {
                ulong dTotal = total - prevTotal;
                ulong dIdle = idle - prevIdle;
                if (dTotal > 0UL)
                {
                    if (dIdle > dTotal) dIdle = dTotal;
                    cpuPct = (int)Math.Round(100.0 * (double)(dTotal - dIdle) / (double)dTotal);
                }
            }
            prevIdle = idle;
            prevTotal = total;
            havePrev = true;
        }

        private void SampleRam()
        {
            MemStatus ms = new MemStatus();
            ms.dwLength = (uint)Marshal.SizeOf(typeof(MemStatus));
            if (!GlobalMemoryStatusEx(ref ms)) return;

            double gb = 1073741824.0;
            ramTotalGb = ms.ullTotalPhys / gb;
            ramFreeGb = ms.ullAvailPhys / gb;
            ramUsedGb = Math.Max(0.0, ramTotalGb - ramFreeGb);
            ramPct = (int)ms.dwMemoryLoad;
        }
        private static RegistryKey OpenHklm()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        }

        private static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\0", " ");
            return System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
        }

        private static string RegText(string path, string name)
        {
            try
            {
                using (RegistryKey bk = OpenHklm())
                using (RegistryKey rk = bk.OpenSubKey(path))
                {
                    if (rk == null) return "";
                    object v = rk.GetValue(name);
                    return v == null ? "" : Clean(Convert.ToString(v, CultureInfo.InvariantCulture));
                }
            }
            catch { return ""; }
        }

        private static string Usable(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            string l = s.ToLowerInvariant();
            if (l.Contains("o.e.m") || l.Contains("default string")) return "";
            return s;
        }

        private static string GetCpuName()
        {
            return RegText(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString");
        }

        private static string GetBoardName()
        {
            const string bios = @"HARDWARE\DESCRIPTION\System\BIOS";
            string mf = Usable(RegText(bios, "BaseBoardManufacturer"));
            string pr = Usable(RegText(bios, "BaseBoardProduct"));
            if (mf.Length == 0 && pr.Length == 0)
            {
                mf = Usable(RegText(bios, "SystemManufacturer"));
                pr = Usable(RegText(bios, "SystemProductName"));
            }
            if (mf.Length > 0 && pr.Length > 0 && pr.StartsWith(mf, StringComparison.OrdinalIgnoreCase)) return pr;
            return (mf + " " + pr).Trim();
        }

        private static string[] GetGpuNames()
        {
            List<string> phys = new List<string>();
            List<string> all = new List<string>();
            try
            {
                using (RegistryKey bk = OpenHklm())
                using (RegistryKey ck = bk.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"))
                {
                    if (ck != null)
                    {
                        foreach (string sub in ck.GetSubKeyNames())
                        {
                            int num;
                            if (!int.TryParse(sub, NumberStyles.None, CultureInfo.InvariantCulture, out num)) continue;
                            try
                            {
                                using (RegistryKey sk = ck.OpenSubKey(sub))
                                {
                                    if (sk == null) continue;
                                    string desc = Clean(Convert.ToString(sk.GetValue("DriverDesc"), CultureInfo.InvariantCulture));
                                    if (desc.Length == 0) continue;
                                    string id = Convert.ToString(sk.GetValue("MatchingDeviceId"), CultureInfo.InvariantCulture);
                                    if (id == null) id = "";
                                    if (!all.Contains(desc)) all.Add(desc);
                                    if (id.StartsWith("pci\\", StringComparison.OrdinalIgnoreCase) && !phys.Contains(desc)) phys.Add(desc);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
            return (phys.Count > 0 ? phys : all).ToArray();
        }
    }
    internal sealed class GpuMeter : IDisposable
    {
        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        private static extern uint PdhOpenQueryW(string dataSource, IntPtr userData, out IntPtr query);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        private static extern uint PdhAddEnglishCounterW(IntPtr query, string counterPath, IntPtr userData, out IntPtr counter);

        [DllImport("pdh.dll")]
        private static extern uint PdhCollectQueryData(IntPtr query);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        private static extern uint PdhGetFormattedCounterArrayW(IntPtr counter, uint format, ref uint bufferSize, out uint itemCount, IntPtr itemBuffer);

        [DllImport("pdh.dll")]
        private static extern uint PdhCloseQuery(IntPtr query);

        [StructLayout(LayoutKind.Sequential)]
        private struct PdhValue
        {
            public uint Status;
            public double Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PdhItem
        {
            public IntPtr Name;
            public PdhValue Value;
        }

        private const uint PdhFmtDouble = 0x00000200;
        private const uint PdhMoreData = 0x800007D2;

        private IntPtr query = IntPtr.Zero;
        private IntPtr counter = IntPtr.Zero;
        private bool disposed;

        public bool Available;                                   // sayaç bu sistemde var mı
        public double Total;                                     // genel GPU kullanımı (0-100)
        public Dictionary<int, double> PerPid = new Dictionary<int, double>();   // süreç başına GPU %

        public GpuMeter()
        {
            try
            {
                if (PdhOpenQueryW(null, IntPtr.Zero, out query) != 0)
                {
                    query = IntPtr.Zero;
                    return;
                }
                if (PdhAddEnglishCounterW(query, @"\GPU Engine(*)\Utilization Percentage", IntPtr.Zero, out counter) != 0)
                {
                    PdhCloseQuery(query);
                    query = IntPtr.Zero;
                    counter = IntPtr.Zero;
                    return;
                }
                Available = true;
                PdhCollectQueryData(query);   // ilk (temel) ölçüm
            }
            catch
            {
                Available = false;
            }
        }

        public void Sample()
        {
            if (!Available || disposed) return;
            try
            {
                if (PdhCollectQueryData(query) != 0) return;

                uint size = 0;
                uint count = 0;
                uint rc = PdhGetFormattedCounterArrayW(counter, PdhFmtDouble, ref size, out count, IntPtr.Zero);
                if (rc != PdhMoreData || size == 0) return;

                IntPtr buf = Marshal.AllocHGlobal((int)size);
                try
                {
                    rc = PdhGetFormattedCounterArrayW(counter, PdhFmtDouble, ref size, out count, buf);
                    if (rc != 0) return;

                    int itemSize = Marshal.SizeOf(typeof(PdhItem));
                    Dictionary<string, double> engines = new Dictionary<string, double>();
                    Dictionary<int, Dictionary<string, double>> perPid = new Dictionary<int, Dictionary<string, double>>();

                    for (int i = 0; i < (int)count; i++)
                    {
                        PdhItem it = Marshal.PtrToStructure<PdhItem>(new IntPtr(buf.ToInt64() + (long)i * itemSize));
                        if (it.Value.Status != 0 && it.Value.Status != 1) continue;

                        string name = Marshal.PtrToStringUni(it.Name);
                        if (string.IsNullOrEmpty(name) || !name.StartsWith("pid_", StringComparison.Ordinal)) continue;

                        int us = name.IndexOf('_', 4);
                        int luid = name.IndexOf("luid_", StringComparison.Ordinal);
                        int et = name.IndexOf("_engtype_", StringComparison.Ordinal);
                        if (us < 0 || luid < 0 || et < 0) continue;

                        int pid;
                        if (!int.TryParse(name.Substring(4, us - 4), NumberStyles.None, CultureInfo.InvariantCulture, out pid)) continue;

                        string engineKey = name.Substring(luid);
                        string engType = name.Substring(et + 9);
                        double v = it.Value.Value;
                        if (v < 0.0) v = 0.0;

                        double cur;
                        engines.TryGetValue(engineKey, out cur);
                        engines[engineKey] = cur + v;

                        Dictionary<string, double> types;
                        if (!perPid.TryGetValue(pid, out types))
                        {
                            types = new Dictionary<string, double>();
                            perPid[pid] = types;
                        }
                        double tc;
                        types.TryGetValue(engType, out tc);
                        types[engType] = tc + v;
                    }

                    double max = 0.0;
                    foreach (KeyValuePair<string, double> ev in engines)
                    {
                        if (ev.Value > max) max = ev.Value;
                    }
                    Total = Math.Min(100.0, max);

                    Dictionary<int, double> result = new Dictionary<int, double>();
                    foreach (KeyValuePair<int, Dictionary<string, double>> pv in perPid)
                    {
                        double best = 0.0;
                        foreach (KeyValuePair<string, double> tv in pv.Value)
                        {
                            if (tv.Value > best) best = tv.Value;
                        }
                        result[pv.Key] = Math.Min(100.0, best);
                    }
                    PerPid = result;
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
            catch { }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try { if (query != IntPtr.Zero) PdhCloseQuery(query); } catch { }
            query = IntPtr.Zero;
        }
    }
    internal sealed class ProcRow
    {
        public string Name = "";
        public int Count;
        public double Cpu;      // toplam CPU kapasitesine göre %
        public double Gpu;      // %
        public long Ram;        // bayt (working set)
    }
    internal sealed class ProcPdh : IDisposable
    {
        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        private static extern uint PdhOpenQueryW(string dataSource, IntPtr userData, out IntPtr query);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        private static extern uint PdhAddEnglishCounterW(IntPtr query, string counterPath, IntPtr userData, out IntPtr counter);

        [DllImport("pdh.dll")]
        private static extern uint PdhCollectQueryData(IntPtr query);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        private static extern uint PdhGetFormattedCounterArrayW(IntPtr counter, uint format, ref uint bufferSize, out uint itemCount, IntPtr itemBuffer);

        [DllImport("pdh.dll")]
        private static extern uint PdhCloseQuery(IntPtr query);

        [StructLayout(LayoutKind.Sequential)]
        private struct PdhValue
        {
            public uint Status;
            public double Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PdhItem
        {
            public IntPtr Name;
            public PdhValue Value;
        }

        public sealed class Item
        {
            public double Cpu;   // tek çekirdeğe göre % (çok çekirdekte 100'ü aşabilir)
            public double Mem;   // özel çalışma kümesi (bayt)
            public int Pid;
        }

        private const uint PdhFmtDouble = 0x00000200;
        private const uint PdhFmtNoCap100 = 0x00008000;
        private const uint PdhMoreData = 0x800007D2;

        private IntPtr query = IntPtr.Zero;
        private IntPtr cCpu = IntPtr.Zero, cMem = IntPtr.Zero, cId = IntPtr.Zero;
        public bool Ok;

        public ProcPdh()
        {
            try
            {
                if (PdhOpenQueryW(null, IntPtr.Zero, out query) != 0) { query = IntPtr.Zero; return; }
                if (PdhAddEnglishCounterW(query, @"\Process(*)\% Processor Time", IntPtr.Zero, out cCpu) != 0 ||
                    PdhAddEnglishCounterW(query, @"\Process(*)\Working Set - Private", IntPtr.Zero, out cMem) != 0 ||
                    PdhAddEnglishCounterW(query, @"\Process(*)\ID Process", IntPtr.Zero, out cId) != 0)
                {
                    PdhCloseQuery(query);
                    query = IntPtr.Zero;
                    return;
                }
                PdhCollectQueryData(query);   // temel ölçüm
                Ok = true;
            }
            catch { Ok = false; }
        }

        ~ProcPdh() { Dispose(); }

        public void Dispose()
        {
            try { if (query != IntPtr.Zero) PdhCloseQuery(query); } catch { }
            query = IntPtr.Zero;
            Ok = false;
        }

        private static Dictionary<string, double> ReadArray(IntPtr counter)
        {
            Dictionary<string, double> d = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            uint size = 0, count = 0;
            uint rc = PdhGetFormattedCounterArrayW(counter, PdhFmtDouble | PdhFmtNoCap100, ref size, out count, IntPtr.Zero);
            if (rc != PdhMoreData || size == 0) return d;
            IntPtr buf = Marshal.AllocHGlobal((int)size);
            try
            {
                rc = PdhGetFormattedCounterArrayW(counter, PdhFmtDouble | PdhFmtNoCap100, ref size, out count, buf);
                if (rc != 0) return d;
                int itemSize = Marshal.SizeOf(typeof(PdhItem));
                for (int i = 0; i < (int)count; i++)
                {
                    PdhItem it = Marshal.PtrToStructure<PdhItem>(new IntPtr(buf.ToInt64() + (long)i * itemSize));
                    if (it.Value.Status != 0 && it.Value.Status != 1) continue;
                    string name = Marshal.PtrToStringUni(it.Name);
                    if (string.IsNullOrEmpty(name)) continue;
                    d[name] = it.Value.Value;
                }
            }
            finally { Marshal.FreeHGlobal(buf); }
            return d;
        }
        public Dictionary<string, Item> Read()
        {
            if (!Ok) return null;
            try
            {
                if (PdhCollectQueryData(query) != 0) return null;
                Dictionary<string, double> cpu = ReadArray(cCpu);
                Dictionary<string, double> mem = ReadArray(cMem);
                Dictionary<string, double> ids = ReadArray(cId);
                if (ids.Count == 0) return null;

                Dictionary<string, Item> res = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, double> kv in ids)
                {
                    Item it = new Item();
                    it.Pid = (int)kv.Value;
                    double v;
                    if (cpu.TryGetValue(kv.Key, out v)) it.Cpu = Math.Max(0.0, v);
                    if (mem.TryGetValue(kv.Key, out v)) it.Mem = Math.Max(0.0, v);
                    res[kv.Key] = it;
                }
                return res;
            }
            catch { return null; }
        }
    }

    internal sealed class ProcSampler
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MemStatus
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemStatus buffer);

        private readonly ProcPdh pdh = new ProcPdh();
        private readonly Dictionary<int, TimeSpan> prevCpu = new Dictionary<int, TimeSpan>();
        private readonly Stopwatch clock = new Stopwatch();

        public static double TotalPhysBytes()
        {
            MemStatus ms = new MemStatus();
            ms.dwLength = (uint)Marshal.SizeOf(typeof(MemStatus));
            if (!GlobalMemoryStatusEx(ref ms)) return 0.0;
            return (double)ms.ullTotalPhys;
        }

        public List<ProcRow> Sample(Dictionary<int, double> gpuByPid)
        {
            double elapsed = clock.IsRunning ? clock.Elapsed.TotalSeconds : 0.0;
            clock.Reset();
            clock.Start();
            int cores = Math.Max(1, Environment.ProcessorCount);

            Dictionary<string, ProcRow> groups = new Dictionary<string, ProcRow>(StringComparer.OrdinalIgnoreCase);
            Dictionary<int, TimeSpan> now = new Dictionary<int, TimeSpan>();
            Dictionary<string, ProcPdh.Item> pitems = pdh.Read();
            if (pitems != null)
            {
                foreach (KeyValuePair<string, ProcPdh.Item> kv in pitems)
                {
                    string inst = kv.Key;
                    if (inst.Equals("_Total", StringComparison.OrdinalIgnoreCase) || inst.Equals("Idle", StringComparison.OrdinalIgnoreCase)) continue;
                    int hash = inst.LastIndexOf('#');
                    string pname = hash > 0 ? inst.Substring(0, hash) : inst;
                    double pcpu = Math.Min(100.0, kv.Value.Cpu / cores);
                    double pgpu = 0.0;
                    if (gpuByPid != null) gpuByPid.TryGetValue(kv.Value.Pid, out pgpu);

                    ProcRow prow;
                    if (!groups.TryGetValue(pname, out prow))
                    {
                        prow = new ProcRow();
                        prow.Name = pname;
                        groups[pname] = prow;
                    }
                    prow.Count++;
                    prow.Cpu += pcpu;
                    prow.Gpu += pgpu;
                    prow.Ram += (long)kv.Value.Mem;
                }
                List<ProcRow> plist = new List<ProcRow>(groups.Values);
                foreach (ProcRow r in plist)
                {
                    r.Cpu = Math.Min(100.0, r.Cpu);
                    r.Gpu = Math.Min(100.0, r.Gpu);
                }
                return plist;
            }

            Process[] all;
            try { all = Process.GetProcesses(); }
            catch { return new List<ProcRow>(); }

            foreach (Process p in all)
            {
                try
                {
                    int pid = p.Id;
                    if (pid == 0) continue;   // Sistem Boşta İşlemi

                    string name = p.ProcessName;

                    long ram = 0;
                    try { ram = p.WorkingSet64; } catch { }

                    double cpu = 0.0;
                    try
                    {
                        TimeSpan total = p.TotalProcessorTime;
                        now[pid] = total;
                        TimeSpan prev;
                        if (elapsed > 0.0 && prevCpu.TryGetValue(pid, out prev))
                        {
                            double d = (total - prev).TotalSeconds;
                            if (d > 0.0) cpu = Math.Min(100.0, d / (elapsed * cores) * 100.0);
                        }
                    }
                    catch { }   // erişim reddedilen sistem süreçleri

                    double gpu = 0.0;
                    if (gpuByPid != null) gpuByPid.TryGetValue(pid, out gpu);

                    ProcRow row;
                    if (!groups.TryGetValue(name, out row))
                    {
                        row = new ProcRow();
                        row.Name = name;
                        groups[name] = row;
                    }
                    row.Count++;
                    row.Cpu += cpu;
                    row.Gpu += gpu;
                    row.Ram += ram;
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }

            prevCpu.Clear();
            foreach (KeyValuePair<int, TimeSpan> kv in now) prevCpu[kv.Key] = kv.Value;

            List<ProcRow> list = new List<ProcRow>(groups.Values);
            foreach (ProcRow r in list)
            {
                r.Cpu = Math.Min(100.0, r.Cpu);
                r.Gpu = Math.Min(100.0, r.Gpu);
            }
            return list;
        }
    }
    internal sealed class ProcessForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int Rows = 30;                  // en fazla kart sayısı
        private int vis = 8;                           // ekrana göre görünen satır sayısı

        private Theme t;
        private readonly Func<string, object[], string> tr;
        private readonly GpuMeter gpu;
        private readonly ProcSampler sampler = new ProcSampler();
        private readonly float k;                       // DPI ölçeği

        private int metric;                             // 0 = CPU, 1 = GPU, 2 = RAM
        private readonly double totalRam;
        private List<ProcRow> last = new List<ProcRow>();

        private readonly Label lblTitle = new Label();
        private readonly Label lblInfo = new Label();
        private readonly RoundButton closeX = new RoundButton();
        private readonly RoundButton[] chips = new RoundButton[3];
        private readonly RoundScrollBar scroll = new RoundScrollBar();
        private int top;                                // listenin ilk görünen satırı
        private readonly RoundPanel[] cards = new RoundPanel[Rows];
        private readonly IconTile[] tiles = new IconTile[Rows];
        private readonly Label[] names = new Label[Rows];
        private readonly Label[] stats = new Label[Rows];
        private readonly UsageBar[] bars = new UsageBar[Rows];
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private static readonly Color[] Palette =
        {
            Color.FromArgb(99, 102, 241), Color.FromArgb(14, 165, 233), Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11), Color.FromArgb(244, 63, 94), Color.FromArgb(168, 85, 247)
        };

        private int S(int v) { return (int)Math.Round(v * k); }

        public ProcessForm(Theme theme, Func<string, object[], string> translate, GpuMeter meter, int startMetric)
        {
            t = theme;
            tr = translate;
            gpu = meter;
            metric = Math.Max(0, Math.Min(2, startMetric));

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                k = g.DpiX / 96f;
            }

            totalRam = ProcSampler.TotalPhysBytes();

            Rectangle wa0 = Screen.PrimaryScreen.WorkingArea;
            vis = Math.Max(6, Math.Min(Rows, (int)((wa0.Height - S(138 + 16 + 30)) / (58.0 * k))));

            FormBorderStyle = FormBorderStyle.None;
            Icon = AlazLogos.AppIcon;   // Alaz logosu (görev çubuğu / Alt-Tab)
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            BackColor = t.Bg;
            KeyPreview = true;

            BuildUi();
            ApplyTheme(t);

            last = sampler.Sample(gpu.Available ? gpu.PerPid : null);   // temel ölçüm
            UpdateChips();
            ApplyLanguage();

            timer.Interval = 1000;   // ilk güncelleme hızlı, sonra 2 sn
            timer.Tick += OnTimer;
            timer.Start();

            KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            Resize += delegate { ApplyRegion(); };
            Load += delegate { ApplyRegion(); };
            FormClosed += delegate { timer.Stop(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) timer.Dispose();
            base.Dispose(disposing);
        }
        public void PlaceNextTo(Form hw)
        {
            StartPosition = FormStartPosition.Manual;
            Rectangle wa = Screen.FromControl(hw).WorkingArea;
            int x = hw.Right + S(10);
            if (x + Width > wa.Right) x = Math.Max(wa.Left, hw.Left + S(40));
            int y = Math.Max(wa.Top, Math.Min(hw.Top, wa.Bottom - Height));
            Location = new Point(x, y);
        }

        public void SetMetric(int m)
        {
            metric = Math.Max(0, Math.Min(2, m));
            top = 0;
            UpdateChips();
            RefreshRows();
        }

        private void ApplyRegion()
        {
            if (Width < 10 || Height < 10) return;
            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, Width, Height), S(26)))
            {
                Region = new Region(p);
            }
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        private void BuildUi()
        {
            SuspendLayout();

            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(S(28), S(14));
            lblTitle.MouseDown += Drag;

            lblInfo.Font = new Font("Segoe UI", 9.5F);
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(S(31), S(54));
            lblInfo.MouseDown += Drag;

            closeX.SetBounds(S(598), S(18), S(38), S(34));
            closeX.Radius = S(12);
            closeX.Text = "✕";
            closeX.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            closeX.Click += delegate { Close(); };

            Controls.Add(lblTitle);
            Controls.Add(lblInfo);
            Controls.Add(closeX);
            string[] chipText = { "⚙  CPU", "🎮  GPU", "💾  RAM" };
            for (int i = 0; i < chips.Length; i++)
            {
                RoundButton c = new RoundButton();
                c.SetBounds(S(24) + i * S(160), S(92), S(150), S(34));
                c.Radius = S(17);
                c.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                c.Text = chipText[i];
                int idx = i;
                c.Click += delegate { SetMetric(idx); };
                chips[i] = c;
                Controls.Add(c);
            }
            for (int i = 0; i < Rows; i++)
            {
                RoundPanel card = new RoundPanel();
                card.SetBounds(S(24), S(138 + i * 58), S(612), S(50));
                card.Radius = S(16);

                IconTile tile = new IconTile();
                tile.SetBounds(S(12), S(6), S(38), S(38));
                tile.Radius = S(12);

                Label lblName = new Label();
                lblName.AutoSize = false;
                lblName.SetBounds(S(62), S(5), S(330), S(20));
                lblName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                lblName.AutoEllipsis = true;

                Label lblStats = new Label();
                lblStats.AutoSize = false;
                lblStats.SetBounds(S(62), S(26), S(330), S(18));
                lblStats.Font = new Font("Segoe UI", 8.5F);
                lblStats.AutoEllipsis = true;

                UsageBar bar = new UsageBar();
                bar.SetBounds(S(404), S(13), S(194), S(24));

                card.Controls.Add(tile);
                card.Controls.Add(lblName);
                card.Controls.Add(lblStats);
                card.Controls.Add(bar);
                card.Visible = false;
                Controls.Add(card);

                cards[i] = card;
                tiles[i] = tile;
                names[i] = lblName;
                stats[i] = lblStats;
                bars[i] = bar;
            }

            scroll.SetBounds(S(642), S(138), S(10), S(vis * 58 - 8));
            scroll.Scrolled += delegate { top = scroll.Value; RefreshRows(); };
            Controls.Add(scroll);

            ClientSize = new Size(S(660), S(138 + vis * 58 + 16));

            MouseDown += Drag;

            ResumeLayout(false);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int step = e.Delta > 0 ? -3 : 3;
            int nt = Math.Max(0, top + step);
            if (nt == top) return;
            top = nt;
            RefreshRows();
        }

        private void UpdateChips()
        {
            for (int i = 0; i < chips.Length; i++)
            {
                chips[i].UseAccent = (i == metric);
                chips[i].Invalidate();
            }
        }
        public void ApplyTheme(Theme th)
        {
            t = th;

            BackColor = t.Bg;
            lblTitle.ForeColor = t.Text;
            lblInfo.ForeColor = t.Sub;

            closeX.Normal = t.Btn;
            closeX.Hover = Color.FromArgb(232, 65, 80);
            closeX.Pressed = Color.FromArgb(190, 45, 60);
            closeX.Border = t.Border;
            closeX.Fore = t.Text;
            closeX.Invalidate();

            foreach (RoundButton c in chips)
            {
                c.Normal = t.Btn;
                c.Hover = t.BtnHover;
                c.Pressed = t.BtnPressed;
                c.Border = t.Border;
                c.Fore = t.Text;
                c.Accent1 = t.Accent;
                c.Accent2 = t.Accent2;
                c.Invalidate();
            }

            for (int i = 0; i < Rows; i++)
            {
                cards[i].BackColor = t.Card;
                names[i].ForeColor = t.Text;
                stats[i].ForeColor = t.Sub;
                bars[i].Track = t.Track;
                bars[i].Fill1 = t.Accent;
                bars[i].Fill2 = t.Accent2;
                bars[i].Fore = t.Text;
                bars[i].Invalidate();
                tiles[i].Invalidate();
            }

            scroll.TrackColor = Color.FromArgb(50, t.Sub);
            scroll.ThumbColor = Color.FromArgb(150, t.Sub);
            scroll.HotColor = t.Accent;
            scroll.Invalidate();

            Invalidate(true);
        }

        public void ApplyLanguage()
        {
            lblTitle.Text = "🔍 " + tr("pdTitle", null);
            RefreshRows();
        }
        private void OnTimer(object sender, EventArgs e)
        {
            if (timer.Interval != 1000) timer.Interval = 1000;   // saniyede bir canlı güncelleme
            last = sampler.Sample(gpu.Available ? gpu.PerPid : null);
            RefreshRows();
        }

        private static double SortKey(ProcRow r, int m)
        {
            return m == 0 ? r.Cpu : (m == 1 ? r.Gpu : (double)r.Ram);
        }

        private static string DisplayName(string n)
        {
            if (n.IndexOf(' ') >= 0 || n == "System" || n == "Registry" || n == "Idle") return n;
            return n + ".exe";
        }

        private string FormatRam(long bytes)
        {
            double mb = bytes / 1048576.0;
            if (mb >= 1024.0) return (mb / 1024.0).ToString("0.0") + " " + tr("unitGb", null);
            return mb.ToString("0") + " " + tr("unitMb", null);
        }

        private void RefreshRows()
        {
            lblInfo.Text = (metric == 1 && !gpu.Available) ? tr("hwNA", null) : tr("pdSub", null);

            int m = metric;
            List<ProcRow> sorted = new List<ProcRow>(last);
            sorted.Sort(delegate (ProcRow a, ProcRow b) { return SortKey(b, m).CompareTo(SortKey(a, m)); });

            int maxTop = Math.Max(0, sorted.Count - vis);
            if (top > maxTop) top = maxTop;
            if (!scroll.IsDragging) scroll.SetState(sorted.Count, vis, top);

            for (int i = 0; i < Rows; i++)
            {
                if (i >= vis || top + i >= sorted.Count)
                {
                    cards[i].Visible = false;
                    continue;
                }

                ProcRow r = sorted[top + i];
                cards[i].Visible = true;

                string nm = DisplayName(r.Name);
                names[i].Text = r.Count > 1 ? nm + "  ×" + r.Count.ToString(CultureInfo.InvariantCulture) : nm;
                stats[i].Text = tr("pdStats", new object[] { r.Cpu, r.Gpu, FormatRam(r.Ram) });

                double pct;
                if (m == 0) pct = r.Cpu;
                else if (m == 1) pct = r.Gpu;
                else pct = totalRam > 0.0 ? r.Ram * 100.0 / totalRam : 0.0;
                bars[i].SetValue((int)Math.Round(pct));

                int h = 7;
                foreach (char ch in r.Name) h = unchecked(h * 31 + ch);
                h &= 0x7fffffff;
                tiles[i].Fill = Palette[h % Palette.Length];
                string first = r.Name.Trim();
                tiles[i].Letter = first.Length > 0 ? first.Substring(0, 1).ToUpperInvariant() : "?";
                tiles[i].Invalidate();
            }
        }
    }
    internal static class AlazLogos
    {
        private const string DarkB64 = "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";
        private const string LightB64 = "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";

        private static Image dark, light, darkTitle, lightTitle;
        private static Icon appIcon;

        public static Image Dark { get { if (dark == null) dark = Decode(DarkB64); return dark; } }
        public static Image Light { get { if (light == null) light = Decode(LightB64); return light; } }
        public static Image DarkTitle { get { if (darkTitle == null) darkTitle = Scaled(Dark, 104); return darkTitle; } }
        public static Image LightTitle { get { if (lightTitle == null) lightTitle = Scaled(Light, 104); return lightTitle; } }

        public static Icon AppIcon
        {
            get
            {
                if (appIcon == null)
                {
                    try
                    {
                        using (Bitmap bmp = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            using (GraphicsPath p = Gfx.RoundRect(new RectangleF(0, 0, 128, 128), 28))
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(18, 20, 28)))
                            {
                                g.FillPath(b, p);
                            }
                            Image l = Dark;
                            int h = 100;
                            int w = (int)Math.Round(h * (double)l.Width / l.Height);
                            g.DrawImage(l, new Rectangle((128 - w) / 2, (128 - h) / 2, w, h));
                            appIcon = Icon.FromHandle(bmp.GetHicon());
                        }
                    }
                    catch { appIcon = null; }
                }
                return appIcon;
            }
        }

        private static Image Decode(string data)
        {
            int comma = data.IndexOf(',');
            if (data.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0) data = data.Substring(comma + 1);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
            using (Image img = Image.FromStream(ms))
            {
                return new Bitmap(img);   // akışa bağımlı olmayan kopya
            }
        }

        private static Image Scaled(Image src, int height)
        {
            int w = Math.Max(1, (int)Math.Round(height * (double)src.Width / src.Height));
            Bitmap bmp = new Bitmap(w, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(src, new Rectangle(0, 0, w, height));
            }
            return bmp;
        }
    }
}