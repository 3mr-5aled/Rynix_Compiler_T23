using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rynix_Compiler
{
    public partial class Form1 : Form
    {
        private TextBoxScrollWatcher _scrollWatcher;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        public Form1()
        {
            InitializeComponent();
            SetupTitleBarButtons();
            SetupActionButtons();
            SetupEditorLineNumbers();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x20000; // WS_MINIMIZEBOX
                cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
                return cp;
            }
        }

        private void SetupTitleBarButtons()
        {
            btnMinimize.MouseEnter += (s, e) => { btnMinimize.BackColor = Color.FromArgb(39, 48, 67); btnMinimize.Invalidate(); };
            btnMinimize.MouseLeave += (s, e) => { btnMinimize.BackColor = Color.Transparent; btnMinimize.Invalidate(); };

            btnMaximize.MouseEnter += (s, e) => { btnMaximize.BackColor = Color.FromArgb(39, 48, 67); btnMaximize.Invalidate(); };
            btnMaximize.MouseLeave += (s, e) => { btnMaximize.BackColor = Color.Transparent; btnMaximize.Invalidate(); };

            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(220, 38, 38); btnClose.ForeColor = Color.White; btnClose.Invalidate(); };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.FromArgb(148, 163, 184); btnClose.Invalidate(); };
        }

        private void SetupActionButtons()
        {
            button1.MouseEnter += (s, e) => button1.BackColor = Color.FromArgb(29, 78, 216);
            button1.MouseLeave += (s, e) => button1.BackColor = Color.FromArgb(37, 99, 235);

            button2.MouseEnter += (s, e) => button2.BackColor = Color.FromArgb(226, 232, 240);
            button2.MouseLeave += (s, e) => button2.BackColor = Color.FromArgb(241, 245, 249);
        }

        private void btnMinimize_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(btnMinimize.ForeColor, 1.4f))
            {
                int midY = btnMinimize.Height / 2 + 1;
                int midX = btnMinimize.Width / 2;
                e.Graphics.DrawLine(pen, midX - 5, midY, midX + 5, midY);
            }
        }

        private void btnMaximize_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(btnMaximize.ForeColor, 1.2f))
            {
                int midX = btnMaximize.Width / 2;
                int midY = btnMaximize.Height / 2;
                if (this.WindowState == FormWindowState.Maximized)
                {
                    e.Graphics.DrawRectangle(pen, midX - 2, midY - 6, 8, 8);
                    Color fill = btnMaximize.BackColor == Color.Transparent ? panelTitleBar.BackColor : btnMaximize.BackColor;
                    using (var brush = new SolidBrush(fill))
                    {
                        e.Graphics.FillRectangle(brush, midX - 6, midY - 2, 8, 8);
                    }
                    e.Graphics.DrawRectangle(pen, midX - 6, midY - 2, 8, 8);
                }
                else
                {
                    e.Graphics.DrawRectangle(pen, midX - 5, midY - 5, 10, 10);
                }
            }
        }

        private void btnClose_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var pen = new Pen(btnClose.ForeColor, 1.5f))
            {
                int midX = btnClose.Width / 2;
                int midY = btnClose.Height / 2;
                e.Graphics.DrawLine(pen, midX - 4.5f, midY - 4.5f, midX + 4.5f, midY + 4.5f);
                e.Graphics.DrawLine(pen, midX + 4.5f, midY - 4.5f, midX - 4.5f, midY + 4.5f);
            }
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 2)
                {
                    ToggleMaximize();
                }
                else
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            ToggleMaximize();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ToggleMaximize()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
                this.WindowState = FormWindowState.Maximized;
            }
            if (btnMaximize != null)
            {
                btnMaximize.Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (btnMaximize != null)
            {
                btnMaximize.Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && this.WindowState != FormWindowState.Maximized)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    Point cursor = this.PointToClient(Cursor.Position);
                    const int resizeBorder = 6;

                    bool left = cursor.X <= resizeBorder;
                    bool right = cursor.X >= this.ClientSize.Width - resizeBorder;
                    bool top = cursor.Y <= resizeBorder;
                    bool bottom = cursor.Y >= this.ClientSize.Height - resizeBorder;

                    if (top && left) m.Result = (IntPtr)HTTOPLEFT;
                    else if (top && right) m.Result = (IntPtr)HTTOPRIGHT;
                    else if (bottom && left) m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (bottom && right) m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (left) m.Result = (IntPtr)HTLEFT;
                    else if (right) m.Result = (IntPtr)HTRIGHT;
                    else if (top) m.Result = (IntPtr)HTTOP;
                    else if (bottom) m.Result = (IntPtr)HTBOTTOM;
                }
                return;
            }

            base.WndProc(ref m);
        }

        private void SetupEditorLineNumbers()
        {
            try
            {
                typeof(Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, panelLineNumbers, new object[] { true });
            }
            catch
            {
            }

            textBox1.TextChanged += (s, e) =>
            {
                AdjustGutterWidth();
                panelLineNumbers.Invalidate();
            };

            textBox1.Resize += (s, e) => panelLineNumbers.Invalidate();
            textBox1.FontChanged += (s, e) =>
            {
                AdjustGutterWidth();
                panelLineNumbers.Invalidate();
            };

            textBox1.KeyUp += (s, e) => panelLineNumbers.Invalidate();

            if (textBox1.IsHandleCreated)
            {
                AttachWatcher();
            }
            else
            {
                textBox1.HandleCreated += (s, e) => AttachWatcher();
            }
        }

        private void AttachWatcher()
        {
            if (_scrollWatcher == null && textBox1.IsHandleCreated)
            {
                _scrollWatcher = new TextBoxScrollWatcher(textBox1, () => panelLineNumbers.Invalidate());
            }
        }

        private void AdjustGutterWidth()
        {
            int lineCount = Math.Max(1, textBox1.Lines.Length + 1);
            int digits = Math.Max(2, lineCount.ToString().Length);
            int neededWidth = TextRenderer.MeasureText(new string('9', digits), textBox1.Font).Width + 14;
            if (panelLineNumbers.Width != neededWidth)
            {
                panelLineNumbers.Width = neededWidth;
            }
        }

        private void panelLineNumbers_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(panelLineNumbers.BackColor);

            using (var pen = new Pen(Color.FromArgb(203, 213, 225)))
            {
                e.Graphics.DrawLine(pen, panelLineNumbers.Width - 1, 0, panelLineNumbers.Width - 1, panelLineNumbers.Height);
            }

            if (textBox1.Font == null) return;

            int firstChar = textBox1.GetCharIndexFromPosition(new Point(1, 1));
            int firstLine = textBox1.GetLineFromCharIndex(firstChar);

            int totalLines = textBox1.Lines.Length;
            if (totalLines == 0) totalLines = 1;

            int fontHeight = TextRenderer.MeasureText("1", textBox1.Font).Height;

            using (var brush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near })
            {
                int line = firstLine;
                while (line < totalLines)
                {
                    int charIndex = textBox1.GetFirstCharIndexFromLine(line);
                    if (charIndex < 0 && line > 0) break;

                    Point pos = textBox1.GetPositionFromCharIndex(charIndex >= 0 ? charIndex : 0);
                    if (pos.Y > panelLineNumbers.Height)
                        break;

                    string lineStr = (line + 1).ToString();
                    Rectangle rect = new Rectangle(0, pos.Y, panelLineNumbers.Width - 6, fontHeight);
                    e.Graphics.DrawString(lineStr, textBox1.Font, brush, rect, sf);

                    line++;
                }

                if (textBox1.Text.EndsWith("\n") || textBox1.Text.EndsWith("\r"))
                {
                    int lastChar = textBox1.TextLength > 0 ? textBox1.TextLength - 1 : 0;
                    Point lastPos = textBox1.GetPositionFromCharIndex(lastChar);
                    int y = lastPos.Y + fontHeight;
                    if (y >= 0 && y < panelLineNumbers.Height)
                    {
                        string lineStr = (totalLines + 1).ToString();
                        Rectangle rect = new Rectangle(0, y, panelLineNumbers.Width - 6, fontHeight);
                        e.Graphics.DrawString(lineStr, textBox1.Font, brush, rect, sf);
                    }
                }
            }
        }

        private void panelLineNumbers_MouseDown(object sender, MouseEventArgs e)
        {
            int charIndex = textBox1.GetCharIndexFromPosition(new Point(1, e.Y));
            int line = textBox1.GetLineFromCharIndex(charIndex);
            int lineStart = textBox1.GetFirstCharIndexFromLine(line);
            if (lineStart >= 0)
            {
                textBox1.Focus();
                textBox1.Select(lineStart, 0);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5 || (e.Control && e.KeyCode == Keys.Enter))
            {
                CompileCode();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CompileCode();
        }

        private void CompileCode()
        {
            listBoxErrors.Items.Clear();
            dataGridView1.Rows.Clear();

            string Code = textBox1.Text;
            Rynix_Compiler.Start_Compiling(Code);
            PrintTokens();
            PrintErrors();

            int tokenCount = Rynix_Compiler.Rynix_Scanner.Tokens.Count;
            int errorCount = Errors.Error_List.Count;

            if (errorCount == 0)
            {
                lblStatus.Text = string.Format("Compiled successfully: {0} tokens.", tokenCount);
                lblStatus.ForeColor = Color.FromArgb(5, 150, 105);
            }
            else
            {
                lblStatus.Text = string.Format("Completed with {0} error(s) and {1} tokens.", errorCount, tokenCount);
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        void PrintTokens()
        {
            dataGridView1.Rows.Clear();
            for (int i = 0; i < Rynix_Compiler.Rynix_Scanner.Tokens.Count; i++)
            {
                dataGridView1.Rows.Add(
                    (i + 1).ToString(),
                    Rynix_Compiler.Rynix_Scanner.Tokens.ElementAt(i).lex,
                    Rynix_Compiler.Rynix_Scanner.Tokens.ElementAt(i).token_type
                );
            }
        }

        void PrintErrors()
        {
            listBoxErrors.BeginUpdate();
            listBoxErrors.Items.Clear();
            if (Errors.DetailedErrors.Count > 0)
            {
                for (int i = 0; i < Errors.DetailedErrors.Count; i++)
                {
                    var err = Errors.DetailedErrors[i];
                    listBoxErrors.Items.Add(string.Format("[Line {0}] [Error {1}] {2}", err.Line, i + 1, err.Lexeme));
                }
            }
            else
            {
                for (int i = 0; i < Errors.Error_List.Count; i++)
                {
                    listBoxErrors.Items.Add(string.Format("[Error {0}] {1}", i + 1, Errors.Error_List[i]));
                }
            }
            listBoxErrors.EndUpdate();
        }

        private void listBoxErrors_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBoxErrors.SelectedIndex;
            if (index >= 0 && index < Errors.DetailedErrors.Count)
            {
                var err = Errors.DetailedErrors[index];
                if (err.StartIndex >= 0 && err.StartIndex + err.Length <= textBox1.TextLength)
                {
                    textBox1.Focus();
                    textBox1.Select(err.StartIndex, err.Length);
                    textBox1.ScrollToCaret();
                    return;
                }

                int lineIndex = Math.Max(0, err.Line - 1);
                int lineStart = textBox1.GetFirstCharIndexFromLine(lineIndex);
                if (lineStart >= 0)
                {
                    int posOnLine = textBox1.Text.IndexOf(err.Lexeme, lineStart, StringComparison.Ordinal);
                    if (posOnLine >= 0)
                    {
                        textBox1.Focus();
                        textBox1.Select(posOnLine, err.Length);
                        textBox1.ScrollToCaret();
                        return;
                    }
                }
            }

            if (index >= 0 && index < Errors.Error_List.Count)
            {
                string errLexeme = Errors.Error_List[index];
                if (!string.IsNullOrEmpty(errLexeme))
                {
                    int pos = textBox1.Text.LastIndexOf(errLexeme, StringComparison.Ordinal);
                    if (pos >= 0)
                    {
                        textBox1.Focus();
                        textBox1.Select(pos, errLexeme.Length);
                        textBox1.ScrollToCaret();
                    }
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            listBoxErrors.Items.Clear();
            Rynix_Compiler.TokenStream.Clear();
            Rynix_Compiler.Rynix_Scanner.Tokens.Clear();
            Errors.Clear();
            lblStatus.Text = "Cleared.";
            lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AdjustGutterWidth();
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_scrollWatcher != null)
            {
                _scrollWatcher.ReleaseHandle();
                _scrollWatcher = null;
            }
            base.OnFormClosed(e);
        }

        private class TextBoxScrollWatcher : NativeWindow
        {
            private readonly Action _onAction;
            private const int WM_VSCROLL = 0x115;
            private const int WM_HSCROLL = 0x114;
            private const int WM_MOUSEWHEEL = 0x20A;
            private const int WM_PAINT = 0x00F;
            private const int WM_KEYUP = 0x101;

            public TextBoxScrollWatcher(TextBox tb, Action onAction)
            {
                _onAction = onAction;
                AssignHandle(tb.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL || m.Msg == WM_PAINT || m.Msg == WM_KEYUP)
                {
                    _onAction?.Invoke();
                }
            }
        }
    }
}
