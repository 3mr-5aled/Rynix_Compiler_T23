using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rynix_Compiler
{
    public partial class Form1 : Form
    {
        private TextBoxScrollWatcher _scrollWatcher;

        public Form1()
        {
            InitializeComponent();
            SetupEditorLineNumbers();
        }

        private void SetupEditorLineNumbers()
        {
            // Enable double buffering for line number panel to avoid flicker
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
                // Ignore if reflection fails in restricted environment
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

            // Subtle vertical separator line on the right edge
            using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
            {
                e.Graphics.DrawLine(pen, panelLineNumbers.Width - 1, 0, panelLineNumbers.Width - 1, panelLineNumbers.Height);
            }

            if (textBox1.Font == null) return;

            int firstChar = textBox1.GetCharIndexFromPosition(new Point(1, 1));
            int firstLine = textBox1.GetLineFromCharIndex(firstChar);

            int totalLines = textBox1.Lines.Length;
            if (totalLines == 0) totalLines = 1;

            int fontHeight = TextRenderer.MeasureText("1", textBox1.Font).Height;

            using (var brush = new SolidBrush(Color.FromArgb(130, 130, 130)))
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

                // If user text ends with a newline, show that trailing empty line's number
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
                lblStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblStatus.Text = string.Format("Completed with {0} error(s) and {1} tokens.", errorCount, tokenCount);
                lblStatus.ForeColor = Color.DarkRed;
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
            for (int i = 0; i < Errors.Error_List.Count; i++)
            {
                listBoxErrors.Items.Add(string.Format("[Error {0}] {1}", i + 1, Errors.Error_List[i]));
            }
            listBoxErrors.EndUpdate();
        }

        private void listBoxErrors_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBoxErrors.SelectedIndex;
            if (index >= 0 && index < Errors.Error_List.Count)
            {
                string errLexeme = Errors.Error_List[index];
                if (!string.IsNullOrEmpty(errLexeme))
                {
                    int pos = textBox1.Text.IndexOf(errLexeme, StringComparison.Ordinal);
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
            Errors.Error_List.Clear();
            lblStatus.Text = "Cleared.";
            lblStatus.ForeColor = Color.DimGray;
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
