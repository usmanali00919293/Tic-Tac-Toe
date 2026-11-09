using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe
{
    // =========================================================================
    // REALISTIC AUDIO GENERATOR (TIC-TAC-TOE & APP INTERACTIONS)
    // =========================================================================
    public static class TicTacToeAudio
    {
        // Realistic 'X' and 'O' Placement Sound (Clean wooden/mechanical pop click)
        public static void PlayMove(char symbol)
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.08; // 80ms crisp placement
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];
                    Random rnd = new Random();

                    // 'X' aur 'O' ke liye thora sa alag tone taaki pehchan ho sakay
                    double baseFreq = (symbol == 'X') ? 580.0 : 440.0;

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        double decay = Math.Exp(-55.0 * t);
                        double clickNoise = (rnd.NextDouble() * 2.0 - 1.0) * Math.Exp(-140.0 * t) * 0.35;
                        double tone = Math.Sin(2.0 * Math.PI * baseFreq * t) * 0.65;

                        double sample = (tone + clickNoise) * decay;
                        pcm[i] = (short)(sample * short.MaxValue * 0.85);
                    }

                    PlayRawPcmWav(pcm, sampleRate);
                }
                catch { }
            });
        }

        // Realistic Victory/Win Jingle (Gentle 3-tone harmonic chime)
        public static void PlayWin()
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.45;
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];

                    double[] notes = { 523.25, 659.25, 783.99 }; // C5, E5, G5

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        double sample = 0;

                        for (int n = 0; n < notes.Length; n++)
                        {
                            double noteStart = n * 0.12;
                            if (t >= noteStart)
                            {
                                double noteT = t - noteStart;
                                double decay = Math.Exp(-12.0 * noteT);
                                sample += Math.Sin(2.0 * Math.PI * notes[n] * noteT) * decay * 0.32;
                            }
                        }

                        pcm[i] = (short)(sample * short.MaxValue);
                    }

                    PlayRawPcmWav(pcm, sampleRate);
                }
                catch { }
            });
        }

        // Realistic Draw Sound (Soft mellow descending tone)
        public static void PlayDraw()
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.35;
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        double freq = 340.0 - (t * 120.0); // Pitch slides down
                        double decay = Math.Exp(-8.0 * t);
                        double sample = Math.Sin(2.0 * Math.PI * freq * t) * decay * 0.45;
                        pcm[i] = (short)(sample * short.MaxValue);
                    }

                    PlayRawPcmWav(pcm, sampleRate);
                }
                catch { }
            });
        }

        // Button Click / Selection Sound
        public static void PlayButtonClick()
        {
            Task.Run(() =>
            {
                try
                {
                    int sampleRate = 22050;
                    double duration = 0.04;
                    int numSamples = (int)(sampleRate * duration);
                    short[] pcm = new short[numSamples];

                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        double sample = Math.Sin(2.0 * Math.PI * 800.0 * t) * Math.Exp(-80.0 * t) * 0.40;
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

    public partial class Form1 : Form
    {
        // Tracks active game sub-windows
        public static int ActiveGameCount = 0;

        private TcpListener server;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;

        private char[] board = new char[9];
        private char currentPlayer = 'X';
        private char mySymbol = 'X';
        private bool gameOver = false;
        private bool connected = false;

        private Button btnChessProgrammatic;
        private Label lblCredits;

        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < 9; i++)
            {
                boardButtons[i].Tag = i;
                boardButtons[i].Click += BoardButton_Click;
            }

            btnHost.Click += BtnHost_Click;
            btnJoin.Click += BtnJoin_Click;
            btnScan.Click += BtnScan_Click;
            btnRestart.Click += BtnRestart_Click;

            btnLudo.Click += BtnLudo_Click;
            btnTicTacToe.Click += BtnTicTacToe_Click;

            AddChessButtonProgrammatically();
            AddProjectCredits();

            UpdateStatus("Host a game or join another PC.");
            UpdateBoard();
        }

        private void AddChessButtonProgrammatically()
        {
            btnChessProgrammatic = new Button
            {
                Text = "CHESS GAME",
                Size = btnLudo != null ? btnLudo.Size : new Size(130, 35),
                Location = btnLudo != null ? new Point(btnLudo.Location.X + btnLudo.Width + 12, btnLudo.Location.Y) : new Point(220, 20),
                BackColor = Color.FromArgb(120, 65, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnChessProgrammatic.FlatAppearance.BorderSize = 0;
            btnChessProgrammatic.Click += BtnChess_Click;

            this.Controls.Add(btnChessProgrammatic);
            btnChessProgrammatic.BringToFront();
        }

        private void AddProjectCredits()
        {
            lblCredits = new Label
            {
                Text = "Developed by Usman Ashraf & Sir Muhammad Abdullah | Multiplayer Gaming Suite",
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 50, 70),
                BackColor = Color.FromArgb(225, 232, 240)
            };
            this.Controls.Add(lblCredits);
        }

        private void BtnTicTacToe_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            UpdateStatus("Tic-Tac-Toe selected. Host or join a game.");
        }

        private void BtnLudo_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            ActiveGameCount++;
            LudoForm ludo = new LudoForm();
            ludo.FormClosed += (s, args) => { ActiveGameCount = Math.Max(0, ActiveGameCount - 1); };
            ludo.Show(this);
        }

        private void BtnChess_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            ActiveGameCount++;
            ChessForm chess = new ChessForm();
            chess.FormClosed += (s, args) => { ActiveGameCount = Math.Max(0, ActiveGameCount - 1); };
            chess.Show(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (ActiveGameCount > 0)
            {
                e.Cancel = true;
                MessageBox.Show(
                    "You cannot close the application while a game is running.",
                    "Active Game Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            connected = false;
            try { server?.Stop(); } catch { }
            try { client?.Close(); } catch { }

            base.OnFormClosing(e);
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            try
            {
                lstWifi.Items.Clear();
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "wlan show networks mode=bssid",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    foreach (string line in output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("SSID ", StringComparison.OrdinalIgnoreCase) && trimmed.Contains(":"))
                        {
                            string name = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
                            if (!string.IsNullOrWhiteSpace(name)) lstWifi.Items.Add(name);
                        }
                    }
                }

                if (lstWifi.Items.Count == 0) lstWifi.Items.Add("No networks found. Check Wi-Fi.");
                UpdateStatus("Wi-Fi scan completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wi-Fi scan failed: " + ex.Message);
            }
        }

        private async void BtnHost_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            try
            {
                if (connected || server != null) return;
                btnHost.Enabled = false;
                btnJoin.Enabled = false;

                mySymbol = 'X';
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();

                lblIP.Text = "Your IP: " + GetLocalIP();
                UpdateStatus("Hosting on port 5000. Waiting for player O...");

                client = await server.AcceptTcpClientAsync();
                server.Stop();
                server = null;

                SetupConnection(client);
                connected = true;
                ResetGame(false);
                UpdateStatus("Player connected! You are X.");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                UpdateStatus("Host error: " + ex.Message);
                try { server?.Stop(); } catch { }
                server = null;
                client = null;
                btnHost.Enabled = true;
                btnJoin.Enabled = true;
            }
        }

        private async void BtnJoin_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            try
            {
                if (connected) return;
                string ip = txtIP.Text.Trim();

                if (!IPAddress.TryParse(ip, out IPAddress address) || address.AddressFamily != AddressFamily.InterNetwork)
                {
                    MessageBox.Show("Enter the host PC's valid IPv4 address.");
                    return;
                }

                btnHost.Enabled = false;
                btnJoin.Enabled = false;
                mySymbol = 'O';

                client = new TcpClient();
                UpdateStatus("Connecting to host...");
                await client.ConnectAsync(address, 5000);

                SetupConnection(client);
                connected = true;
                ResetGame(false);
                UpdateStatus("Connected! You are O.");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                UpdateStatus("Connection failed: " + ex.Message);
                MessageBox.Show("Could not connect. Check the host IP, Wi-Fi and firewall.\n\n" + ex.Message);
                try { client?.Close(); } catch { }
                client = null;
                btnHost.Enabled = true;
                btnJoin.Enabled = true;
            }
        }

        private void SetupConnection(TcpClient tcp)
        {
            NetworkStream stream = tcp.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (connected && reader != null)
                {
                    string message = await reader.ReadLineAsync();
                    if (message == null || IsDisposed || !IsHandleCreated) break;
                    BeginInvoke(new Action(() => { ProcessMessage(message); }));
                }
            }
            catch { }
            finally
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    try
                    {
                        BeginInvoke(new Action(() => {
                            connected = false;
                            UpdateStatus("Connection closed. Start a new game.");
                            btnHost.Enabled = true;
                            btnJoin.Enabled = true;
                        }));
                    }
                    catch { }
                }
            }
        }

        private void ProcessMessage(string message)
        {
            if (message.StartsWith("MOVE:"))
            {
                string[] parts = message.Split(':');
                if (parts.Length == 3 && int.TryParse(parts[1], out int index) && index >= 0 && index < 9)
                {
                    char symbol = parts[2][0];
                    if (!gameOver && currentPlayer != mySymbol && symbol != mySymbol && board[index] == '\0')
                    {
                        MakeMove(index, symbol);
                    }
                }
            }
            else if (message == "RESET")
            {
                ResetGame(false);
            }
        }

        private async void BoardButton_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                MessageBox.Show("First host or join a game.");
                return;
            }

            Button button = (Button)sender;
            int index = (int)button.Tag;

            if (gameOver || currentPlayer != mySymbol || board[index] != '\0') return;

            MakeMove(index, mySymbol);
            try
            {
                if (writer != null) await writer.WriteLineAsync($"MOVE:{index}:{mySymbol}");
            }
            catch
            {
                UpdateStatus("Could not send move. Check connection.");
            }
        }

        private void MakeMove(int index, char symbol)
        {
            if (board[index] != '\0' || gameOver) return;
            board[index] = symbol;

            // Mark place sound play karein
            TicTacToeAudio.PlayMove(symbol);

            UpdateBoard();

            if (HasWinner(symbol))
            {
                gameOver = true;
                UpdateStatus(symbol + " wins!");
                TicTacToeAudio.PlayWin();
                MessageBox.Show(symbol == mySymbol ? "Congratulations! You won!" : "The other player won!");
                return;
            }

            bool draw = true;
            foreach (char cell in board)
            {
                if (cell == '\0') { draw = false; break; }
            }

            if (draw)
            {
                gameOver = true;
                UpdateStatus("Game ended in a draw!");
                TicTacToeAudio.PlayDraw();
                MessageBox.Show("It's a draw!");
                return;
            }

            currentPlayer = symbol == 'X' ? 'O' : 'X';
            UpdateStatus(currentPlayer == mySymbol ? "Your turn (" + mySymbol + ")" : "Waiting for " + currentPlayer + "'s turn...");
        }

        private bool HasWinner(char p)
        {
            int[,] wins = {
                { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },
                { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },
                { 0, 4, 8 }, { 2, 4, 6 }
            };

            for (int i = 0; i < wins.GetLength(0); i++)
            {
                if (board[wins[i, 0]] == p && board[wins[i, 1]] == p && board[wins[i, 2]] == p) return true;
            }
            return false;
        }

        private void UpdateBoard()
        {
            for (int i = 0; i < 9; i++)
            {
                boardButtons[i].Text = board[i] == '\0' ? "" : board[i].ToString();
                boardButtons[i].ForeColor = board[i] == 'X' ? Color.DodgerBlue : Color.Crimson;
            }
        }

        private void UpdateStatus(string text) => lblStatus.Text = text;

        private string GetLocalIP()
        {
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    return ip.ToString();
            }
            return "IP not found";
        }

        private async void BtnRestart_Click(object sender, EventArgs e)
        {
            TicTacToeAudio.PlayButtonClick();
            ResetGame(false);
            if (connected && writer != null)
            {
                try { await writer.WriteLineAsync("RESET"); }
                catch { UpdateStatus("Could not send restart."); }
            }
        }

        private void ResetGame(bool unused)
        {
            board = new char[9];
            currentPlayer = 'X';
            gameOver = false;
            UpdateBoard();
            UpdateStatus(connected ? (mySymbol == 'X' ? "Your turn (X)" : "Waiting for X...") : "Host a game or join another PC.");
        }
    }
}