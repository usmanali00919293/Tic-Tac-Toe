using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe
{
    // =========================================================================
    // REALISTIC AUDIO GENERATOR (AUTHENTIC WOODEN SURFACE TUMBLE & BOUNCES)
    // =========================================================================
    public static class GameAudio
    {
        // Real Wooden Board Dice Tumbling, Shaking & Final Wooden Bounce Sound
        public static void PlayAuthenticDiceRoll()
        {
            Task.Run(() =>
            {
                try
                {
                    const int sampleRate = 44100;
                    const double durationSeconds = 0.72;
                    int totalSamples = (int)(sampleRate * durationSeconds);
                    short[] pcm = new short[totalSamples];
                    Random rng = new Random();

                    // Uneven bounce timing model: fast chaotic rolling into slower settling bounces
                    double[] bounceTimes = { 0.03, 0.08, 0.14, 0.21, 0.29, 0.38, 0.48, 0.59, 0.68 };
                    double[] bounceWeights = { 0.85, 0.70, 0.60, 0.52, 0.45, 0.38, 0.30, 0.22, 0.14 };

                    for (int b = 0; b < bounceTimes.Length; b++)
                    {
                        int startSample = (int)(bounceTimes[b] * sampleRate);
                        double impactWeight = bounceWeights[b];
                        int bounceLen = (int)(0.045 * sampleRate);

                        // Resonant frequencies of solid wooden dice impacting a wooden surface
                        double fBody1 = 280.0 + rng.Next(-30, 40); // Deep hollow wood resonance
                        double fBody2 = 540.0 + rng.Next(-40, 50); // Mid wooden box tone
                        double fClick = 1450.0 + rng.Next(-150, 200); // Sharp transient corner tap

                        for (int i = 0; i < bounceLen; i++)
                        {
                            int idx = startSample + i;
                            if (idx >= totalSamples) break;

                            double t = (double)i / sampleRate;

                            // Wood impact decay envelopes
                            double decayFast = Math.Exp(-110.0 * t);
                            double decayBody = Math.Exp(-55.0 * t);

                            // Initial wood transient click
                            double transient = (rng.NextDouble() * 2.0 - 1.0) * decayFast * 0.40;

                            // Hollow wooden body resonance
                            double body = (Math.Sin(2.0 * Math.PI * fBody1 * t) * 0.50 +
                                           Math.Sin(2.0 * Math.PI * fBody2 * t) * 0.30 +
                                           Math.Sin(2.0 * Math.PI * fClick * t) * 0.20) * decayBody;

                            double sample = (transient + body) * impactWeight;

                            // Soft saturation mixing
                            int currentVal = pcm[idx];
                            int mixed = currentVal + (int)(sample * 24000.0);
                            if (mixed > 32767) mixed = 32767;
                            if (mixed < -32768) mixed = -32768;
                            pcm[idx] = (short)mixed;
                        }
                    }

                    // Low rolling wood scrape/table rumble in the background
                    double filteredNoise = 0.0;
                    for (int i = 0; i < totalSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        double fade = Math.Max(0.0, 1.0 - t / durationSeconds);
                        double noise = rng.NextDouble() * 2.0 - 1.0;
                        filteredNoise = filteredNoise * 0.88 + noise * 0.12;

                        int scrape = (int)(filteredNoise * fade * 1800.0);
                        int combined = pcm[i] + scrape;
                        if (combined > 32767) combined = 32767;
                        if (combined < -32768) combined = -32768;
                        pcm[i] = (short)combined;
                    }

                    // Write to standard PCM in-memory WAV container
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                        bw.Write(36 + totalSamples * 2);
                        bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                        bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                        bw.Write(16);
                        bw.Write((short)1); // PCM format
                        bw.Write((short)1); // Mono
                        bw.Write(sampleRate);
                        bw.Write(sampleRate * 2); // Byte rate
                        bw.Write((short)2); // Block align
                        bw.Write((short)16); // 16-bit
                        bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                        bw.Write(totalSamples * 2);

                        for (int i = 0; i < totalSamples; i++)
                        {
                            bw.Write(pcm[i]);
                        }

                        bw.Flush();
                        ms.Position = 0;

                        using (SoundPlayer sp = new SoundPlayer(ms))
                        {
                            sp.PlaySync();
                        }
                    }
                }
                catch
                {
                    // Fallback
                    try { SystemSounds.Asterisk.Play(); } catch { }
                }
            });
        }

        public static void PlayStep()
        {
            Task.Run(() => { try { Console.Beep(540, 45); } catch { } });
        }

        public static void PlayCapture()
        {
            Task.Run(() =>
            {
                try
                {
                    Console.Beep(920, 70);
                    Console.Beep(460, 110);
                }
                catch { }
            });
        }

        public static void PlayWin()
        {
            Task.Run(() =>
            {
                int[] melody = { 523, 659, 784, 1046 };
                foreach (int note in melody)
                {
                    try { Console.Beep(note, 120); } catch { }
                }
            });
        }

        public static void PlayClick()
        {
            Task.Run(() => { try { Console.Beep(750, 25); } catch { } });
        }
    }

    // Double-buffered board prevents flickering while Windows repaints the game.
    public class SmoothBoardPanel : Panel
    {
        public SmoothBoardPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }

    public class LudoForm : Form
    {
        private const int PORT = 5000;
        private const int CELL = 36;

        // Player Rotation: 0: GREEN, 1: YELLOW, 2: BLUE, 3: RED
        private string[] players = { "GREEN", "YELLOW", "BLUE", "RED" };
        private Color[] playerColors =
        {
            Color.ForestGreen,
            Color.Gold,
            Color.DodgerBlue,
            Color.Crimson
        };

        private int[] startOffsets = { 0, 13, 26, 39 };

        // -1 = Yard, 0-50 = Track, 51-55 = Home lane, 56 = Finished
        private int[,] positions = new int[4, 4];
        private int currentPlayer = 0;
        private int diceValue = 0;
        private int displayDiceValue = 0;
        private bool isRollingAnimation = false;
        private bool gameOver = false;
        private Random random = new Random();

        // Animation Timer for Dice Rotation
        private System.Windows.Forms.Timer rollTimer;
        private int rollAnimTicks = 0;

        // Network
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread receiveThread, hostThread;
        private bool isHost = false;
        private volatile bool connected = false;
        private volatile bool closing = false;
        private readonly object sendLock = new object();

        // Controls
        private SmoothBoardPanel boardPanel;
        private Button btnHost, btnJoin, btnRoll, btnNewGame;
        private TextBox txtIP;
        private Label lblStatus, lblTurn, lblDice, lblMessage;
        private CheckBox chkOfflineLocal;

        // LAN chat controls
        private ListBox lstChat;
        private TextBox txtChat;
        private Button btnSendChat;

        // Each chat line keeps its own player colour for coloured display.
        private class ChatEntry
        {
            public string Text;
            public Color Colour;

            public ChatEntry(string text, Color colour)
            {
                Text = text;
                Colour = colour;
            }
        }

        // Standard 52-Cell Continuous Clockwise Track
        private Point[] track =
        {
            new Point(6, 1), new Point(6, 2), new Point(6, 3), new Point(6, 4), new Point(6, 5),
            new Point(5, 6), new Point(4, 6), new Point(3, 6), new Point(2, 6), new Point(1, 6), new Point(0, 6),
            new Point(0, 7),
            new Point(0, 8), new Point(1, 8), new Point(2, 8), new Point(3, 8), new Point(4, 8), new Point(5, 8),
            new Point(6, 9), new Point(6, 10), new Point(6, 11), new Point(6, 12), new Point(6, 13), new Point(6, 14),
            new Point(7, 14),
            new Point(8, 14), new Point(8, 13), new Point(8, 12), new Point(8, 11), new Point(8, 10), new Point(8, 9),
            new Point(9, 8), new Point(10, 8), new Point(11, 8), new Point(12, 8), new Point(13, 8), new Point(14, 8),
            new Point(14, 7),
            new Point(14, 6), new Point(13, 6), new Point(12, 6), new Point(11, 6), new Point(10, 6), new Point(9, 6),
            new Point(8, 5), new Point(8, 4), new Point(8, 3), new Point(8, 2), new Point(8, 1), new Point(8, 0),
            new Point(7, 0)
        };

        public LudoForm()
        {
            Text = "4-PLAYER LUDO (LAN & OFFLINE PASS-AND-PLAY)";
            // Extra width is required for the chat panel on the right.
            Size = new Size(1300, 760);
            MinimumSize = new Size(1300, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(242, 246, 250);
            Font = new Font("Segoe UI", 9);

            BuildInterface();
            SetupDiceAnimationTimer();
            ResetPositions();
            UpdateScreen();

            FormClosing += LudoForm_FormClosing;
        }

        private void SetupDiceAnimationTimer()
        {
            rollTimer = new System.Windows.Forms.Timer();
            rollTimer.Interval = 42;
            rollTimer.Tick += (s, e) =>
            {
                rollAnimTicks++;
                int tempFace = random.Next(1, 7);
                lblDice.Text = tempFace.ToString();
                lblDice.ForeColor = (rollAnimTicks % 2 == 0) ? Color.Black : playerColors[currentPlayer];

                if (rollAnimTicks >= 15)
                {
                    rollTimer.Stop();
                    isRollingAnimation = false;
                    FinalizeDiceRoll();
                }
            };
        }

        private void BuildInterface()
        {
            Label title = new Label
            {
                Text = "4-PLAYER LUDO",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 53, 82),
                Location = new Point(20, 10),
                Size = new Size(540, 45),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(title);

            boardPanel = new SmoothBoardPanel
            {
                Location = new Point(20, 65),
                Size = new Size(540, 540),
                BackColor = Color.White
            };
            boardPanel.Paint += BoardPanel_Paint;
            boardPanel.MouseClick += BoardPanel_MouseClick;
            Controls.Add(boardPanel);

            int x = 600;

            chkOfflineLocal = new CheckBox
            {
                Text = "4-Player Local (Offline Pass-and-Play)",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 60, 120),
                Location = new Point(x, 65),
                Size = new Size(340, 28),
                Checked = true
            };
            chkOfflineLocal.CheckedChanged += (s, e) =>
            {
                GameAudio.PlayClick();
                lblMessage.Text = chkOfflineLocal.Checked
                    ? "4-Player Offline Active! Green, Yellow, Blue & Red take turns."
                    : "Multiplayer Network mode selected.";
                UpdateScreen();
            };
            Controls.Add(chkOfflineLocal);

            Label networkTitle = new Label
            {
                Text = "LAN MULTIPLAYER (OPTIONAL)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(x, 105),
                Size = new Size(300, 22),
                ForeColor = Color.Gray
            };
            Controls.Add(networkTitle);

            btnHost = CreateButton("HOST GAME", x, 135, 130, 36, Color.FromArgb(30, 53, 82));
            btnHost.Click += BtnHost_Click;
            Controls.Add(btnHost);

            txtIP = new TextBox { Text = "192.168.100.13", Location = new Point(x, 180), Size = new Size(130, 28) };
            Controls.Add(txtIP);

            btnJoin = CreateButton("JOIN GAME", x + 140, 178, 120, 32, Color.ForestGreen);
            btnJoin.Click += BtnJoin_Click;
            Controls.Add(btnJoin);

            lblStatus = new Label
            {
                Text = "Mode: 4-Player Local Active",
                ForeColor = Color.FromArgb(0, 120, 60),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(x, 218),
                Size = new Size(300, 35)
            };
            Controls.Add(lblStatus);

            lblTurn = new Label
            {
                Text = "GREEN'S TURN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(x, 260),
                Size = new Size(280, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTurn);

            lblDice = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 42, FontStyle.Bold),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x + 90, 310),
                Size = new Size(100, 95),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblDice);

            btnRoll = CreateButton("ROLL DICE", x + 45, 425, 190, 48, Color.FromArgb(35, 100, 180));
            btnRoll.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnRoll.Click += BtnRoll_Click;
            Controls.Add(btnRoll);

            lblMessage = new Label
            {
                Text = "Game ready. GREEN rolls first!",
                Location = new Point(x + 5, 490),
                Size = new Size(270, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5f)
            };
            Controls.Add(lblMessage);

            btnNewGame = CreateButton("NEW GAME", x + 45, 555, 190, 38, Color.FromArgb(30, 53, 82));
            btnNewGame.Click += BtnNewGame_Click;
            Controls.Add(btnNewGame);

            Label info = new Label
            {
                Text = "GREEN → YELLOW → BLUE → RED\nSimultaneous 4-Player Rotation",
                Location = new Point(x, 615),
                Size = new Size(280, 45),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(70, 80, 95),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            Controls.Add(info);

            // CHAT PANEL (works in LAN mode; offline mode keeps it local)
            Label chatTitle = new Label
            {
                Text = "GAME CHAT",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 53, 82),
                Location = new Point(930, 65),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(chatTitle);

            lstChat = new ListBox
            {
                Location = new Point(930, 105),
                Size = new Size(330, 475),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                HorizontalScrollbar = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 38,
                IntegralHeight = false
            };
            lstChat.DrawItem += LstChat_DrawItem;
            Controls.Add(lstChat);

            txtChat = new TextBox
            {
                Location = new Point(930, 590),
                Size = new Size(235, 30),
                Font = new Font("Segoe UI", 10),
                MaxLength = 250
            };
            txtChat.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SendChatMessage();
                }
            };
            Controls.Add(txtChat);

            btnSendChat = CreateButton("SEND", 1175, 588, 85, 32, Color.ForestGreen);
            btnSendChat.Click += (s, e) => SendChatMessage();
            Controls.Add(btnSendChat);

            Label chatHint = new Label
            {
                Text = "Host and joined player can chat here.",
                Location = new Point(930, 630),
                Size = new Size(330, 25),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(chatHint);
        }

        private void SendChatMessage()
        {
            if (txtChat == null) return;

            string message = txtChat.Text.Trim();
            if (message.Length == 0) return;

            // In LAN mode, each computer controls two colours:
            // Host = GREEN / BLUE; Joined player = YELLOW / RED.
            string senderName;
            Color senderColour;

            if (chkOfflineLocal.Checked)
            {
                senderName = "LOCAL";
                senderColour = Color.DarkSlateGray;
            }
            else if (isHost)
            {
                senderName = "GREEN / BLUE (HOST)";
                senderColour = Color.ForestGreen;
            }
            else
            {
                senderName = "YELLOW / RED (JOINED)";
                senderColour = Color.DarkGoldenrod;
            }

            string chatLine = senderName + ": " + message;

            // Host displays its own message immediately. A joined player's
            // message is displayed when the host broadcasts it back, avoiding
            // duplicate lines in the joined player's chat window.
            if (chkOfflineLocal.Checked || isHost)
                AddChatMessage(chatLine, senderColour);

            // Send the chat to the other LAN computer. The sender label is
            // included so both computers show the same player colour.
            if (!chkOfflineLocal.Checked && connected)
            {
                string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(chatLine));
                SendLine("CHAT|" + encoded);
            }
            else if (!chkOfflineLocal.Checked)
            {
                AddChatMessage("System: Not connected to a LAN player.", Color.Gray);
            }

            txtChat.Clear();
            txtChat.Focus();
        }

        private void AddChatMessage(string message, Color? messageColour = null)
        {
            if (lstChat == null || lstChat.IsDisposed) return;

            if (lstChat.InvokeRequired)
            {
                SafeUI(() => AddChatMessage(message, messageColour));
                return;
            }

            Color colour = messageColour ?? GetChatColour(message);
            string time = DateTime.Now.ToString("HH:mm");
            lstChat.Items.Add(new ChatEntry("[" + time + "] " + message, colour));

            if (lstChat.Items.Count > 200)
                lstChat.Items.RemoveAt(0);

            if (lstChat.Items.Count > 0)
                lstChat.TopIndex = lstChat.Items.Count - 1;
        }

        private Color GetChatColour(string message)
        {
            string upper = (message ?? string.Empty).ToUpperInvariant();

            if (upper.StartsWith("GREEN / BLUE") || upper.StartsWith("GREEN:") || upper.StartsWith("BLUE:"))
                return Color.ForestGreen;
            if (upper.StartsWith("YELLOW / RED") || upper.StartsWith("YELLOW:") || upper.StartsWith("RED:"))
                return Color.DarkGoldenrod;
            if (upper.StartsWith("LOCAL:"))
                return Color.DarkSlateGray;
            if (upper.StartsWith("SYSTEM:"))
                return Color.Gray;

            return Color.Black;
        }

        private void LstChat_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstChat.Items.Count) return;

            e.DrawBackground();
            ChatEntry entry = lstChat.Items[e.Index] as ChatEntry;
            if (entry != null)
            {
                using (SolidBrush brush = new SolidBrush(entry.Colour))
                {
                    Rectangle textBounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2,
                        e.Bounds.Width - 8, e.Bounds.Height - 4);
                    e.Graphics.DrawString(entry.Text, e.Font, brush, textBounds);
                }
            }
            e.DrawFocusRectangle();
        }

        private void ReceiveChatMessage(string encodedMessage)
        {
            try
            {
                string message = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessage));
                AddChatMessage(message, GetChatColour(message));
            }
            catch
            {
                AddChatMessage("System: Could not read incoming chat message.", Color.Gray);
            }
        }

        private Button CreateButton(string text, int x, int y, int width, int height, Color color)
        {
            Button b = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void BoardPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            // Bases
            DrawBase(g, 0, 0, Color.ForestGreen); // Green Base (Top-Left)
            DrawBase(g, 9, 0, Color.Gold);        // Yellow Base (Top-Right)
            DrawBase(g, 9, 9, Color.DodgerBlue);  // Blue Base (Bottom-Right)
            DrawBase(g, 0, 9, Color.Crimson);     // Red Base (Bottom-Left)

            // Main Track Cells
            for (int i = 0; i < track.Length; i++)
            {
                DrawCell(g, track[i].X, track[i].Y, Color.White);
            }

            // Home Entrance Lanes
            for (int i = 1; i <= 5; i++)
            {
                DrawCell(g, 7, i, Color.LightGreen);      // Green Lane
                DrawCell(g, i, 7, Color.LightYellow);     // Yellow Lane
                DrawCell(g, 7, 14 - i, Color.LightBlue);  // Blue Lane
                DrawCell(g, 14 - i, 7, Color.LightPink);  // Red Lane
            }

            // Correct Starting Squares
            DrawCell(g, 6, 1, Color.ForestGreen); // Green Start
            DrawCell(g, 1, 8, Color.Gold);        // Yellow Start
            DrawCell(g, 8, 13, Color.DodgerBlue); // Blue Start
            DrawCell(g, 13, 6, Color.Crimson);    // Red Start

            // Center Triangles
            using (SolidBrush bGreen = new SolidBrush(Color.ForestGreen))
            using (SolidBrush bYellow = new SolidBrush(Color.Gold))
            using (SolidBrush bBlue = new SolidBrush(Color.DodgerBlue))
            using (SolidBrush bRed = new SolidBrush(Color.Crimson))
            {
                Point center = new Point(7 * CELL + CELL / 2, 7 * CELL + CELL / 2);
                g.FillPolygon(bGreen, new Point[] { new Point(6 * CELL, 6 * CELL), new Point(6 * CELL, 9 * CELL), center });
                g.FillPolygon(bYellow, new Point[] { new Point(6 * CELL, 6 * CELL), new Point(9 * CELL, 6 * CELL), center });
                g.FillPolygon(bBlue, new Point[] { new Point(9 * CELL, 6 * CELL), new Point(9 * CELL, 9 * CELL), center });
                g.FillPolygon(bRed, new Point[] { new Point(6 * CELL, 9 * CELL), new Point(9 * CELL, 9 * CELL), center });
            }

            // Safe Stars
            DrawStar(g, 8, 2);
            DrawStar(g, 2, 6);
            DrawStar(g, 6, 12);
            DrawStar(g, 12, 8);

            // Draw Tokens
            for (int p = 0; p < 4; p++)
            {
                for (int t = 0; t < 4; t++)
                {
                    Point center = GetTokenCenter(p, t);
                    int radius = 12;

                    using (SolidBrush b = new SolidBrush(playerColors[p]))
                        g.FillEllipse(b, center.X - radius, center.Y - radius, radius * 2, radius * 2);

                    using (Pen pen = new Pen(Color.White, 2))
                        g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);

                    using (Font f = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                    using (SolidBrush b = new SolidBrush(p == 1 ? Color.Black : Color.White))
                    using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString((t + 1).ToString(), f, b, new Rectangle(center.X - radius, center.Y - radius, radius * 2, radius * 2), sf);
                    }
                }
            }

            using (Pen border = new Pen(Color.FromArgb(180, 190, 205), 2))
                g.DrawRectangle(border, 0, 0, 538, 538);
        }

        private void DrawStar(Graphics g, int row, int col)
        {
            Point center = CellCenter(row, col);
            int outerRadius = 11, innerRadius = 5;
            PointF[] points = new PointF[10];

            for (int i = 0; i < 10; i++)
            {
                double angle = (i * 36 - 90) * Math.PI / 180;
                int radius = (i % 2 == 0) ? outerRadius : innerRadius;
                points[i] = new PointF(center.X + (float)(radius * Math.Cos(angle)), center.Y + (float)(radius * Math.Sin(angle)));
            }
            using (SolidBrush brush = new SolidBrush(Color.Gray))
            using (Pen pen = new Pen(Color.White, 1.5f))
            {
                g.FillPolygon(brush, points);
                g.DrawPolygon(pen, points);
            }
        }

        private void DrawBase(Graphics g, int col, int row, Color color)
        {
            Rectangle outer = new Rectangle(col * CELL, row * CELL, 6 * CELL, 6 * CELL);
            using (SolidBrush b = new SolidBrush(color)) g.FillRectangle(b, outer);
            using (Pen p = new Pen(Color.White, 2)) g.DrawRectangle(p, outer);

            Rectangle inner = new Rectangle(outer.X + CELL, outer.Y + CELL, outer.Width - 2 * CELL, outer.Height - 2 * CELL);
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillRectangle(b, inner);
            using (Pen p = new Pen(Color.LightGray)) g.DrawRectangle(p, inner);
        }

        private void DrawCell(Graphics g, int row, int col, Color color)
        {
            Rectangle r = new Rectangle(col * CELL, row * CELL, CELL, CELL);
            using (SolidBrush b = new SolidBrush(color)) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.LightGray)) g.DrawRectangle(p, r);
        }

        private Point CellCenter(int row, int col) => new Point(col * CELL + CELL / 2, row * CELL + CELL / 2);

        private Point GetTokenCenter(int player, int token)
        {
            int pos = positions[player, token];
            if (pos == -1) // Yard
            {
                int[,] basePositions =
                {
                    { 2, 2 },   // Green
                    { 11, 2 },  // Yellow
                    { 11, 11 }, // Blue
                    { 2, 11 }   // Red
                };
                int[,] offsets = { { 0, 0 }, { 2, 0 }, { 0, 2 }, { 2, 2 } };
                return CellCenter(basePositions[player, 1] + offsets[token, 1], basePositions[player, 0] + offsets[token, 0]);
            }
            if (pos == 56) // Home Finish
            {
                int[,] finish = { { 7, 6 }, { 6, 7 }, { 7, 8 }, { 8, 7 } };
                return CellCenter(finish[player, 0], finish[player, 1]);
            }
            if (pos >= 51) // Home Lane
            {
                Point home = GetHomeCell(player, pos - 51);
                return CellCenter(home.X, home.Y);
            }
            // Active Track
            int global = (startOffsets[player] + pos) % 52;
            return CellCenter(track[global].X, track[global].Y);
        }

        private Point GetHomeCell(int player, int step)
        {
            switch (player)
            {
                case 0: return new Point(7, 1 + step);       // Green
                case 1: return new Point(1 + step, 7);       // Yellow
                case 2: return new Point(7, 13 - step);      // Blue
                case 3: return new Point(13 - step, 7);      // Red
                default: return new Point(7, 7);
            }
        }

        private void BoardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver || diceValue == 0 || isRollingAnimation) return;

            bool localAllowed = chkOfflineLocal.Checked || (isHost ? currentPlayer == 0 || currentPlayer == 2 : currentPlayer == 1 || currentPlayer == 3);
            if (!localAllowed) return;

            for (int t = 0; t < 4; t++)
            {
                Point center = GetTokenCenter(currentPlayer, t);
                int dx = e.X - center.X;
                int dy = e.Y - center.Y;

                if (dx * dx + dy * dy <= 400 && CanMove(currentPlayer, t))
                {
                    GameAudio.PlayStep();
                    if (chkOfflineLocal.Checked) MoveToken(currentPlayer, t);
                    else SendCommand("MOVE," + currentPlayer + "," + t);
                    return;
                }
            }
        }

        private void BtnRoll_Click(object sender, EventArgs e)
        {
            if (gameOver || isRollingAnimation || diceValue != 0) return;

            GameAudio.PlayClick();
            if (chkOfflineLocal.Checked)
            {
                StartDiceAnimation();
            }
            else
            {
                SendCommand("ROLL");
            }
        }

        private void StartDiceAnimation()
        {
            isRollingAnimation = true;
            btnRoll.Enabled = false;
            rollAnimTicks = 0;
            GameAudio.PlayAuthenticDiceRoll();
            rollTimer.Start();
        }

        private void FinalizeDiceRoll()
        {
            diceValue = random.Next(1, 7);
            displayDiceValue = diceValue;
            lblDice.Text = diceValue.ToString();
            lblDice.ForeColor = Color.Black;

            bool canMove = false;
            for (int t = 0; t < 4; t++)
            {
                if (CanMove(currentPlayer, t))
                {
                    canMove = true;
                    break;
                }
            }

            if (!canMove)
            {
                if (diceValue == 6)
                {
                    lblMessage.Text = players[currentPlayer] + " rolled 6. Roll again!";
                    diceValue = 0;
                }
                else
                {
                    lblMessage.Text = players[currentPlayer] + " rolled " + diceValue + ". No moves possible. Turn passed.";
                    diceValue = 0;
                    NextPlayer();
                }
            }
            else
            {
                lblMessage.Text = players[currentPlayer] + " rolled " + diceValue + ". Select a token.";
            }

            UpdateScreen();
            if (!chkOfflineLocal.Checked) SendState();
        }

        private bool CanMove(int player, int token)
        {
            int pos = positions[player, token];
            if (pos == 56) return false;
            if (pos == -1) return diceValue == 6;
            return pos + diceValue <= 56;
        }

        private void MoveToken(int player, int token)
        {
            if (positions[player, token] == -1)
                positions[player, token] = 0;
            else
                positions[player, token] += diceValue;

            CaptureOpponent(player, token);

            bool won = true;
            for (int t = 0; t < 4; t++)
            {
                if (positions[player, t] != 56) { won = false; break; }
            }

            if (won)
            {
                gameOver = true;
                diceValue = 0;
                displayDiceValue = 0;
                GameAudio.PlayWin();
                lblMessage.Text = players[player] + " WINS THE GAME!";
                UpdateScreen();
                if (!chkOfflineLocal.Checked) SendState();
                MessageBox.Show(players[player] + " WINS THE MATCH!");
                return;
            }

            bool extraTurn = diceValue == 6;
            diceValue = 0;

            if (extraTurn)
            {
                lblMessage.Text = players[player] + " rolled 6! Extra roll awarded.";
            }
            else
            {
                NextPlayer();
            }

            UpdateScreen();
            if (!chkOfflineLocal.Checked) SendState();
        }

        private void CaptureOpponent(int player, int token)
        {
            int pos = positions[player, token];
            if (pos < 0 || pos >= 51) return;

            int global = (startOffsets[player] + pos) % 52;
            int[] safeSquares = { 0, 13, 26, 39, 8, 21, 34, 47 };
            for (int i = 0; i < safeSquares.Length; i++)
            {
                if (global == safeSquares[i]) return;
            }

            for (int p = 0; p < 4; p++)
            {
                if (p == player) continue;
                for (int t = 0; t < 4; t++)
                {
                    int oppPos = positions[p, t];
                    if (oppPos < 0 || oppPos >= 51) continue;

                    int oppGlobal = (startOffsets[p] + oppPos) % 52;
                    if (global == oppGlobal)
                    {
                        positions[p, t] = -1;
                        GameAudio.PlayCapture();
                        lblMessage.Text = players[player] + " captured " + players[p] + "'s piece!";
                    }
                }
            }
        }

        private void NextPlayer()
        {
            currentPlayer = (currentPlayer + 1) % 4;
            displayDiceValue = 0;
            lblMessage.Text = players[currentPlayer] + "'s turn. Roll the dice.";
        }

        private void UpdateScreen()
        {
            if (lblTurn == null) return;

            if (gameOver)
            {
                lblTurn.Text = "GAME OVER";
                lblTurn.ForeColor = Color.DarkSlateGray;
            }
            else
            {
                lblTurn.Text = players[currentPlayer] + "'S TURN";
                lblTurn.ForeColor = playerColors[currentPlayer];
            }

            if (!isRollingAnimation)
            {
                lblDice.Text = displayDiceValue == 0 ? "-" : displayDiceValue.ToString();
            }

            btnRoll.Enabled = !gameOver && diceValue == 0 && !isRollingAnimation;
            boardPanel.Invalidate();
        }

        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            GameAudio.PlayClick();
            rollTimer.Stop();
            isRollingAnimation = false;
            btnRoll.Enabled = true;
            ResetPositions();
            currentPlayer = 0;
            diceValue = 0;
            displayDiceValue = 0;
            gameOver = false;
            lblMessage.Text = "New Game started. GREEN rolls first.";
            UpdateScreen();
            if (!chkOfflineLocal.Checked) SendState();
        }

        private void ResetPositions()
        {
            for (int p = 0; p < 4; p++)
                for (int t = 0; t < 4; t++)
                    positions[p, t] = -1;
        }

        // =====================================
        // LAN MULTIPLAYER LOGIC
        // =====================================
        private void BtnHost_Click(object sender, EventArgs e)
        {
            GameAudio.PlayClick();
            try
            {
                listener = new TcpListener(IPAddress.Any, PORT);
                listener.Start();
                isHost = true;
                connected = false;
                btnHost.Enabled = false;
                btnJoin.Enabled = false;
                chkOfflineLocal.Checked = false;
                lblStatus.Text = "Waiting for LAN players...";
                lblStatus.ForeColor = Color.DarkOrange;
                hostThread = new Thread(WaitForClient) { IsBackground = true };
                hostThread.Start();
                UpdateScreen();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void WaitForClient()
        {
            try
            {
                client = listener.AcceptTcpClient();
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
                writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
                connected = true;

                SafeUI(() => {
                    lblStatus.Text = "LAN Player Connected";
                    lblStatus.ForeColor = Color.Green;
                    AddChatMessage("System: Player connected.");
                    UpdateScreen();
                    SendState();
                });
                ReceiveLoop();
            }
            catch { if (!closing) SafeUI(() => lblStatus.Text = "Connection Failed"); }
        }

        private void BtnJoin_Click(object sender, EventArgs e)
        {
            GameAudio.PlayClick();
            try
            {
                if (!IPAddress.TryParse(txtIP.Text.Trim(), out IPAddress ip)) { MessageBox.Show("Enter valid IP."); return; }
                client = new TcpClient();
                client.Connect(ip, PORT);
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
                writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
                isHost = false;
                connected = true;
                btnHost.Enabled = false;
                btnJoin.Enabled = false;
                chkOfflineLocal.Checked = false;
                lblStatus.Text = "Connected to Host";
                lblStatus.ForeColor = Color.Green;
                AddChatMessage("System: Connected to host.");
                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();
                UpdateScreen();
            }
            catch (Exception ex) { MessageBox.Show("Connection failed:\n" + ex.Message); }
        }

        private void ReceiveLoop()
        {
            try
            {
                string line;
                while (!closing && (line = reader.ReadLine()) != null)
                {
                    string message = line;
                    SafeUI(() =>
                    {
                        if (isHost)
                            ProcessClientCommand(message);
                        else if (message.StartsWith("CHAT|"))
                            ReceiveChatMessage(message.Substring(5));
                        else
                            ApplyState(message);
                    });
                }
            }
            catch { }
            if (!closing)
            {
                connected = false;
                SafeUI(() => {
                    lblStatus.Text = "Disconnected";
                    lblStatus.ForeColor = Color.Red;
                    UpdateScreen();
                });
            }
        }

        private void SendLine(string message)
        {
            if (!connected || writer == null) return;
            try { lock (sendLock) { writer.WriteLine(message); writer.Flush(); } } catch { connected = false; }
        }

        private void SendCommand(string command)
        {
            if (isHost) ProcessHostCommand(command);
            else SendLine(command);
        }

        private void ProcessClientCommand(string command)
        {
            if (!isHost) return;

            if (command.StartsWith("CHAT|"))
            {
                string encoded = command.Substring(5);
                ReceiveChatMessage(encoded);
                // Echo/broadcast to the connected client so both sides see it.
                SendLine("CHAT|" + encoded);
                return;
            }

            if (gameOver) return;
            if (command == "ROLL") { StartDiceAnimation(); return; }
            if (command.StartsWith("MOVE,"))
            {
                string[] parts = command.Split(',');
                if (parts.Length == 3 && int.TryParse(parts[1], out int p) && int.TryParse(parts[2], out int t))
                {
                    if (p == currentPlayer && diceValue != 0 && CanMove(p, t)) MoveToken(p, t);
                }
            }
        }

        private void ProcessHostCommand(string command)
        {
            if (!isHost || gameOver) return;
            if (command == "ROLL") { StartDiceAnimation(); return; }
            if (command.StartsWith("MOVE,"))
            {
                string[] parts = command.Split(',');
                if (parts.Length == 3 && int.TryParse(parts[1], out int p) && int.TryParse(parts[2], out int t))
                {
                    if (p == currentPlayer && diceValue != 0 && CanMove(p, t)) MoveToken(p, t);
                }
            }
        }

        private void SendState()
        {
            if (!isHost || !connected) return;
            StringBuilder data = new StringBuilder();
            data.Append("STATE|").Append(currentPlayer).Append("|").Append(diceValue).Append("|").Append(displayDiceValue).Append("|").Append(gameOver ? "1" : "0").Append("|");
            for (int p = 0; p < 4; p++)
            {
                for (int t = 0; t < 4; t++)
                {
                    data.Append(positions[p, t]);
                    if (p != 3 || t != 3) data.Append(",");
                }
            }
            data.Append("|").Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(lblMessage.Text)));
            SendLine(data.ToString());
        }

        private void ApplyState(string message)
        {
            try
            {
                string[] parts = message.Split('|');
                if (parts.Length != 7 || parts[0] != "STATE") return;
                currentPlayer = int.Parse(parts[1]);
                diceValue = int.Parse(parts[2]);
                displayDiceValue = int.Parse(parts[3]);
                gameOver = parts[4] == "1";
                string[] values = parts[5].Split(',');
                int index = 0;
                for (int p = 0; p < 4; p++)
                {
                    for (int t = 0; t < 4; t++) positions[p, t] = int.Parse(values[index++]);
                }
                lblMessage.Text = Encoding.UTF8.GetString(Convert.FromBase64String(parts[6]));
                UpdateScreen();
            }
            catch { }
        }

        private void SafeUI(Action action)
        {
            if (closing || IsDisposed || !IsHandleCreated) return;
            try { if (InvokeRequired) BeginInvoke(action); else action(); } catch { }
        }

        private void LudoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            closing = true;
            connected = false;
            try { rollTimer?.Stop(); } catch { }
            try { listener?.Stop(); } catch { }
            try { client?.Close(); } catch { }
            try { stream?.Close(); } catch { }
        }
    }
}