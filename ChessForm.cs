using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe
{
    // =========================================================================
    // REALISTIC CHESS WOODEN AUDIO SYNTHESIZER (IN-MEMORY WAV PCM)
    // =========================================================================
    public static class ChessAudio
    {
        // Real Wooden Board Move Sound (Soft dull wood knock/clack)
        public static void PlayChessMove()
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.12; // 120ms sharp impact
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];
                    Random rnd = new Random();

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        // Wood acoustic resonance (decaying low-mid body + burst transient)
                        double decay = Math.Exp(-38.0 * t);
                        double woodBody = Math.Sin(2 * Math.PI * 260 * t) * 0.6 +
                                          Math.Sin(2 * Math.PI * 440 * t) * 0.3;
                        double transient = (rnd.NextDouble() * 2.0 - 1.0) * Math.Exp(-120.0 * t) * 0.35;

                        double sample = (woodBody + transient) * decay;
                        pcm[i] = (short)(sample * short.MaxValue * 0.85);
                    }

                    PlayRawPcmWav(pcm, sampleRate);
                }
                catch { }
            });
        }

        // Real Wooden Piece Strike/Capture Sound (Crisper, heavier wood-on-wood snap)
        public static void PlayChessCapture()
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.16;
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];
                    Random rnd = new Random();

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        // Two resonant wooden piece bodies colliding
                        double decay = Math.Exp(-32.0 * t);
                        double strike = Math.Sin(2 * Math.PI * 340 * t) * 0.5 +
                                        Math.Sin(2 * Math.PI * 680 * t) * 0.35;
                        double sharpClick = (rnd.NextDouble() * 2.0 - 1.0) * Math.Exp(-160.0 * t) * 0.5;

                        double sample = (strike + sharpClick) * decay;
                        pcm[i] = (short)(sample * short.MaxValue * 0.95);
                    }

                    PlayRawPcmWav(pcm, sampleRate);
                }
                catch { }
            });
        }

        // Subtle lift / piece selection sound
        public static void PlayPieceLift()
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.05;
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        double sample = Math.Sin(2 * Math.PI * 520 * t) * Math.Exp(-55.0 * t) * 0.35;
                        pcm[i] = (short)(sample * short.MaxValue);
                    }

                    PlayRawPcmWav(pcm, sampleRate);
                }
                catch { }
            });
        }

        private static void PlayRawPcmWav(short[] pcmData, int sampleRate)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + pcmData.Length * 2);
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((short)1); // PCM
                bw.Write((short)1); // Mono
                bw.Write(sampleRate);
                bw.Write(sampleRate * 2);
                bw.Write((short)2);
                bw.Write((short)16);
                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(pcmData.Length * 2);

                for (int i = 0; i < pcmData.Length; i++)
                {
                    bw.Write(pcmData[i]);
                }

                bw.Flush();
                ms.Position = 0;

                using (SoundPlayer sp = new SoundPlayer(ms))
                {
                    sp.PlaySync();
                }
            }
        }
    }

    public class ChessForm : Form
    {
        private Button[,] squares = new Button[8, 8];
        private string[,] board = new string[8, 8];
        private bool whiteTurn = true;
        private Point? selectedCoord = null;

        // UI Header and Panels
        private Label lblStatus;
        private Panel chessBoardContainer;

        // Requirement 5: Integrated Chat Controls
        private Panel chatPanel;
        private ListBox lstChatMessages;
        private TextBox txtChatInput;
        private Button btnSendChat;

        public ChessForm()
        {
            Text = "Pro Realistic 3D Chess & Live Player Chat";
            Size = new Size(980, 680);
            MinimumSize = new Size(980, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(45, 30, 20); // Deep Walnut Wood outer frame
            Font = new Font("Segoe UI", 9.5f);

            InitializeChessLayout();
            InitializeChatInterface();
            SetupPieces();
        }

        // Requirement 4: Realistic Wooden 3D Chessboard Presentation
        private void InitializeChessLayout()
        {
            lblStatus = new Label
            {
                Text = "WHITE'S TURN (Move any piece)",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 230, 210),
                Location = new Point(30, 15),
                Size = new Size(540, 32),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblStatus);

            // 3D Beveled Outer Wooden Border
            chessBoardContainer = new Panel
            {
                Location = new Point(30, 55),
                Size = new Size(544, 544),
                BackColor = Color.FromArgb(92, 58, 33), // Mahogany Wood Border
                Padding = new Padding(12)
            };
            chessBoardContainer.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                using (Pen lightEdge = new Pen(Color.FromArgb(160, 110, 70), 3))
                    g.DrawRectangle(lightEdge, 1, 1, chessBoardContainer.Width - 3, chessBoardContainer.Height - 3);
                using (Pen shadowEdge = new Pen(Color.FromArgb(20, 12, 8), 4))
                    g.DrawRectangle(shadowEdge, 3, 3, chessBoardContainer.Width - 7, chessBoardContainer.Height - 7);
            };

            int sz = 65;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(sz, sz),
                        Location = new Point(12 + c * sz, 12 + r * sz),
                        FlatStyle = FlatStyle.Flat,
                        UseVisualStyleBackColor = false,
                        Tag = new Point(r, c)
                    };
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Click += Square_Click;
                    btn.Paint += (sender, pe) => RenderCellRealistic3D((Button)sender, pe);

                    squares[r, c] = btn;
                    chessBoardContainer.Controls.Add(btn);
                }
            }
            Controls.Add(chessBoardContainer);
        }

        // Requirement 5: Integrated Chat Feature Interface
        private void InitializeChatInterface()
        {
            chatPanel = new Panel
            {
                Location = new Point(595, 55),
                Size = new Size(345, 544),
                BackColor = Color.FromArgb(32, 22, 16),
                Padding = new Padding(10)
            };

            Label lblChatHeader = new Label
            {
                Text = "IN-GAME CHAT",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 215, 185),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };
            chatPanel.Controls.Add(lblChatHeader);

            lstChatMessages = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 430,
                BackColor = Color.FromArgb(22, 15, 10),
                ForeColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false
            };
            lstChatMessages.Items.Add("[System] Match loaded. Live chat active!");
            chatPanel.Controls.Add(lstChatMessages);

            Panel inputRow = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36
            };

            txtChatInput = new TextBox
            {
                Location = new Point(0, 5),
                Size = new Size(245, 26),
                BackColor = Color.FromArgb(45, 35, 28),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };
            txtChatInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SendChatMessage();
                }
            };
            inputRow.Controls.Add(txtChatInput);

            btnSendChat = new Button
            {
                Text = "SEND",
                Location = new Point(252, 4),
                Size = new Size(72, 28),
                BackColor = Color.FromArgb(170, 115, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnSendChat.FlatAppearance.BorderSize = 0;
            btnSendChat.Click += (s, e) => SendChatMessage();
            inputRow.Controls.Add(btnSendChat);

            chatPanel.Controls.Add(inputRow);
            Controls.Add(chatPanel);
        }

        private void SendChatMessage()
        {
            string message = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            string senderName = whiteTurn ? "White" : "Black";
            string formatted = $"[{senderName}]: {message}";
            lstChatMessages.Items.Add(formatted);
            lstChatMessages.TopIndex = lstChatMessages.Items.Count - 1;
            txtChatInput.Clear();
            ChessAudio.PlayPieceLift();
        }

        // Realistic Wood Grain & Staunton Piece Renderer
        private void RenderCellRealistic3D(Button btn, PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Point pt = (Point)btn.Tag;
            int r = pt.X, c = pt.Y;

            bool isLight = (r + c) % 2 == 0;
            Rectangle rect = new Rectangle(0, 0, btn.Width, btn.Height);

            Color cTop = isLight ? Color.FromArgb(242, 222, 185) : Color.FromArgb(145, 96, 60);
            Color cBottom = isLight ? Color.FromArgb(215, 188, 145) : Color.FromArgb(108, 68, 38);

            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, cTop, cBottom, 45f))
            {
                g.FillRectangle(lgb, rect);
            }

            using (Pen innerLight = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
                g.DrawLine(innerLight, 0, 0, btn.Width, 0);
            using (Pen innerDark = new Pen(Color.FromArgb(60, 0, 0, 0), 1))
                g.DrawLine(innerDark, 0, btn.Height - 1, btn.Width, btn.Height - 1);

            if (selectedCoord != null && selectedCoord.Value.X == r && selectedCoord.Value.Y == c)
            {
                using (SolidBrush sel = new SolidBrush(Color.FromArgb(90, 80, 180, 50)))
                    g.FillRectangle(sel, rect);
                using (Pen selBorder = new Pen(Color.FromArgb(160, 255, 100), 2.5f))
                    g.DrawRectangle(selBorder, 1, 1, btn.Width - 3, btn.Height - 3);
            }

            string piece = board[r, c];
            if (!string.IsNullOrEmpty(piece))
            {
                bool isWhite = piece.StartsWith("W");
                string pieceType = piece.Substring(2);
                DrawRealistic3DPiece(g, rect, pieceType, isWhite);
            }
        }

        private void DrawRealistic3DPiece(Graphics g, Rectangle r, string pieceType, bool isWhite)
        {
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            {
                g.FillEllipse(shadow, r.X + 14, r.Y + r.Height - 14, r.Width - 28, 8);
            }

            char glyph = ' ';
            switch (pieceType)
            {
                case "K": glyph = isWhite ? '♔' : '♚'; break;
                case "Q": glyph = isWhite ? '♕' : '♛'; break;
                case "R": glyph = isWhite ? '♖' : '♜'; break;
                case "B": glyph = isWhite ? '♗' : '♝'; break;
                case "N": glyph = isWhite ? '♘' : '♞'; break;
                case "P": glyph = isWhite ? '♙' : '♟'; break;
            }

            using (Font pieceFont = new Font("Segoe UI Symbol", 34, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                Rectangle shadowOffset = new Rectangle(r.X + 1, r.Y + 3, r.Width, r.Height);
                using (SolidBrush darkEmboss = new SolidBrush(Color.FromArgb(130, 10, 10, 10)))
                {
                    g.DrawString(glyph.ToString(), pieceFont, darkEmboss, shadowOffset, sf);
                }

                Color pieceColor = isWhite ? Color.FromArgb(255, 250, 240) : Color.FromArgb(32, 26, 24);
                using (SolidBrush frontBrush = new SolidBrush(pieceColor))
                {
                    g.DrawString(glyph.ToString(), pieceFont, frontBrush, r, sf);
                }

                using (Pen specularRim = new Pen(isWhite ? Color.FromArgb(100, 255, 255, 255) : Color.FromArgb(120, 100, 80), 1.2f))
                {
                    g.DrawString(glyph.ToString(), pieceFont, new SolidBrush(Color.FromArgb(30, 255, 255, 255)), new PointF(r.X - 1, r.Y - 1));
                }
            }
        }

        private void SetupPieces()
        {
            string[] backRow = { "R", "N", "B", "Q", "K", "B", "N", "R" };
            for (int c = 0; c < 8; c++)
            {
                board[0, c] = "B_" + backRow[c];
                board[1, c] = "B_P";
            }
            for (int r = 2; r < 6; r++)
                for (int c = 0; c < 8; c++) board[r, c] = "";

            for (int c = 0; c < 8; c++)
            {
                board[6, c] = "W_P";
                board[7, c] = "W_" + backRow[c];
            }
            RefreshSquares();
        }

        private void RefreshSquares()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    squares[r, c].Invalidate();
        }

        private void Square_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Point coord = (Point)btn.Tag;
            int r = coord.X, c = coord.Y;

            if (selectedCoord == null)
            {
                string piece = board[r, c];
                if (!string.IsNullOrEmpty(piece))
                {
                    if ((whiteTurn && piece.StartsWith("W")) || (!whiteTurn && piece.StartsWith("B")))
                    {
                        selectedCoord = coord;
                        ChessAudio.PlayPieceLift(); // Wood touch/lift sound
                    }
                }
            }
            else
            {
                Point src = selectedCoord.Value;
                if (src.X == r && src.Y == c)
                {
                    selectedCoord = null;
                    RefreshSquares();
                    return;
                }

                string moving = board[src.X, src.Y];
                string target = board[r, c];

                // Collision & Move rules
                if (string.IsNullOrEmpty(target) || !target.StartsWith(moving.Substring(0, 1)))
                {
                    bool isCapture = !string.IsNullOrEmpty(target);
                    board[r, c] = moving;
                    board[src.X, src.Y] = "";

                    // Authentic piece collision / placement sound
                    if (isCapture)
                        ChessAudio.PlayChessCapture();
                    else
                        ChessAudio.PlayChessMove();

                    whiteTurn = !whiteTurn;
                    lblStatus.Text = whiteTurn ? "WHITE'S TURN" : "BLACK'S TURN";
                }
                selectedCoord = null;
            }
            RefreshSquares();
        }
    }
}