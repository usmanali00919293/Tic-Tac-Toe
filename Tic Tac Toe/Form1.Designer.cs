
namespace Tic_Tac_Toe
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblWifiTitle;
        private System.Windows.Forms.Label lblHostTitle;
        private System.Windows.Forms.Label lblJoinTitle;

        private System.Windows.Forms.ListBox lstWifi;
        private System.Windows.Forms.TextBox txtIP;

        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.Button btnRestart;

        private System.Windows.Forms.Button btnTicTacToe;
        private System.Windows.Forms.Button btnLudo;

        private System.Windows.Forms.Button[] boardButtons;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            lblTitle = new System.Windows.Forms.Label();
            lblSubtitle = new System.Windows.Forms.Label();
            lblIP = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            lblWifiTitle = new System.Windows.Forms.Label();
            lblHostTitle = new System.Windows.Forms.Label();
            lblJoinTitle = new System.Windows.Forms.Label();

            lstWifi = new System.Windows.Forms.ListBox();
            txtIP = new System.Windows.Forms.TextBox();

            btnScan = new System.Windows.Forms.Button();
            btnHost = new System.Windows.Forms.Button();
            btnJoin = new System.Windows.Forms.Button();
            btnRestart = new System.Windows.Forms.Button();

            btnTicTacToe = new System.Windows.Forms.Button();
            btnLudo = new System.Windows.Forms.Button();

            boardButtons = new System.Windows.Forms.Button[9];

            SuspendLayout();

            // FORM
            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.ClientSize =
                new System.Drawing.Size(1000, 680);

            this.MinimumSize =
                new System.Drawing.Size(900, 650);

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Text = "Multiplayer Gaming Application";

            this.BackColor =
                System.Drawing.Color.FromArgb(240, 245, 250);

            this.Font =
                new System.Drawing.Font("Segoe UI", 10F);

            // TITLE
            lblTitle.Text = "MULTIPLAYER GAMING";
            lblTitle.Location =
                new System.Drawing.Point(25, 18);

            lblTitle.Size =
                new System.Drawing.Size(600, 42);

            lblTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI", 23F,
                    System.Drawing.FontStyle.Bold);

            lblTitle.ForeColor =
                System.Drawing.Color.FromArgb(28, 53, 87);

            // SUBTITLE
            lblSubtitle.Text =
                "Play together over Wi-Fi";

            lblSubtitle.Location =
                new System.Drawing.Point(29, 62);

            lblSubtitle.Size =
                new System.Drawing.Size(650, 28);

            lblSubtitle.ForeColor =
                System.Drawing.Color.DimGray;

            // GAME SELECTION
            btnTicTacToe.Text = "TIC-TAC-TOE";

            btnTicTacToe.Location =
                new System.Drawing.Point(390, 100);

            btnTicTacToe.Size =
                new System.Drawing.Size(175, 38);

            btnTicTacToe.BackColor =
                System.Drawing.Color.FromArgb(35, 100, 180);

            btnTicTacToe.ForeColor =
                System.Drawing.Color.White;

            btnTicTacToe.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            btnLudo.Text = "LUDO GAME";

            btnLudo.Location =
                new System.Drawing.Point(580, 100);

            btnLudo.Size =
                new System.Drawing.Size(175, 38);

            btnLudo.BackColor =
                System.Drawing.Color.FromArgb(30, 145, 100);

            btnLudo.ForeColor =
                System.Drawing.Color.White;

            btnLudo.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            // WIFI
            lblWifiTitle.Text = "AVAILABLE WI-FI NETWORKS";

            lblWifiTitle.Location =
                new System.Drawing.Point(30, 115);

            lblWifiTitle.Size =
                new System.Drawing.Size(280, 28);

            lblWifiTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI", 11F,
                    System.Drawing.FontStyle.Bold);

            lstWifi.Location =
                new System.Drawing.Point(30, 150);

            lstWifi.Size =
                new System.Drawing.Size(270, 140);

            lstWifi.IntegralHeight = false;
            lstWifi.BackColor =
                System.Drawing.Color.White;

            btnScan.Text = "Scan Wi-Fi";

            btnScan.Location =
                new System.Drawing.Point(30, 300);

            btnScan.Size =
                new System.Drawing.Size(270, 38);

            // HOST
            lblHostTitle.Text = "HOST A GAME";

            lblHostTitle.Location =
                new System.Drawing.Point(30, 365);

            lblHostTitle.Size =
                new System.Drawing.Size(270, 28);

            lblHostTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI", 11F,
                    System.Drawing.FontStyle.Bold);

            btnHost.Text = "Create Game (X)";

            btnHost.Location =
                new System.Drawing.Point(30, 400);

            btnHost.Size =
                new System.Drawing.Size(270, 42);

            btnHost.BackColor =
                System.Drawing.Color.FromArgb(35, 100, 180);

            btnHost.ForeColor =
                System.Drawing.Color.White;

            btnHost.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            lblIP.Text = "Your IP: Not hosting";

            lblIP.Location =
                new System.Drawing.Point(30, 450);

            lblIP.Size =
                new System.Drawing.Size(300, 30);

            lblIP.Font =
                new System.Drawing.Font(
                    "Segoe UI", 10F,
                    System.Drawing.FontStyle.Bold);

            // JOIN
            lblJoinTitle.Text = "JOIN A GAME";

            lblJoinTitle.Location =
                new System.Drawing.Point(30, 495);

            lblJoinTitle.Size =
                new System.Drawing.Size(270, 28);

            lblJoinTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI", 11F,
                    System.Drawing.FontStyle.Bold);

            txtIP.Location =
                new System.Drawing.Point(30, 530);

            txtIP.Size =
                new System.Drawing.Size(270, 30);

            //txtIP.PlaceholderText = "Enter Host IP Address";

            btnJoin.Text = "Join Game (O)";

            btnJoin.Location =
                new System.Drawing.Point(30, 570);

            btnJoin.Size =
                new System.Drawing.Size(270, 42);

            btnJoin.BackColor =
                System.Drawing.Color.FromArgb(30, 145, 100);

            btnJoin.ForeColor =
                System.Drawing.Color.White;

            btnJoin.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            // TIC TAC TOE BOARD
            int boardX = 390;
            int boardY = 150;
            int cellSize = 115;
            int gap = 7;

            for (int i = 0; i < 9; i++)
            {
                boardButtons[i] =
                    new System.Windows.Forms.Button();

                boardButtons[i].Location =
                    new System.Drawing.Point(
                        boardX + ((i % 3) * (cellSize + gap)),
                        boardY + ((i / 3) * (cellSize + gap))
                    );

                boardButtons[i].Size =
                    new System.Drawing.Size(
                        cellSize, cellSize);

                boardButtons[i].Font =
                    new System.Drawing.Font(
                        "Segoe UI", 36F,
                        System.Drawing.FontStyle.Bold);

                boardButtons[i].BackColor =
                    System.Drawing.Color.White;

                boardButtons[i].FlatStyle =
                    System.Windows.Forms.FlatStyle.Flat;

                boardButtons[i].FlatAppearance.BorderColor =
                    System.Drawing.Color.FromArgb(190, 205, 220);

                boardButtons[i].FlatAppearance.BorderSize = 2;

                boardButtons[i].UseVisualStyleBackColor = false;

                this.Controls.Add(boardButtons[i]);
            }

            // STATUS
            lblStatus.Text =
                "Host a game or join another PC.";

            lblStatus.Location =
                new System.Drawing.Point(390, 525);

            lblStatus.Size =
                new System.Drawing.Size(450, 55);

            lblStatus.Font =
                new System.Drawing.Font(
                    "Segoe UI", 12F,
                    System.Drawing.FontStyle.Bold);

            lblStatus.ForeColor =
                System.Drawing.Color.FromArgb(28, 53, 87);

            // RESTART
            btnRestart.Text = "Restart Game";

            btnRestart.Location =
                new System.Drawing.Point(390, 585);

            btnRestart.Size =
                new System.Drawing.Size(365, 42);

            btnRestart.BackColor =
                System.Drawing.Color.FromArgb(28, 53, 87);

            btnRestart.ForeColor =
                System.Drawing.Color.White;

            btnRestart.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            // ADD CONTROLS
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);

            this.Controls.Add(btnTicTacToe);
            this.Controls.Add(btnLudo);

            this.Controls.Add(lblWifiTitle);
            this.Controls.Add(lstWifi);
            this.Controls.Add(btnScan);

            this.Controls.Add(lblHostTitle);
            this.Controls.Add(btnHost);
            this.Controls.Add(lblIP);

            this.Controls.Add(lblJoinTitle);
            this.Controls.Add(txtIP);
            this.Controls.Add(btnJoin);

            this.Controls.Add(lblStatus);
            this.Controls.Add(btnRestart);

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
