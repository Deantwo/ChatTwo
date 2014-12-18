using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatTwo
{
    public partial class FormMain : Form
    {
        UdpCommunication _client;

        Dictionary<int, FormChat> _chats = new Dictionary<int, FormChat>();

        public FormMain()
        {
            InitializeComponent();

#if DEBUG
            this.Text += " (DEBUG)";
#endif

            _client = new UdpCommunication();
            _client.MessageReceived += ChatTwo_Client_Protocol.MessageReceivedHandler;
            ChatTwo_Client_Protocol.MessageTransmission += _client.SendMessage;

#if DEBUG
            ChatTwo_Client_Protocol.ServerAddress = new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 9020);
#else
            // My server IP and port. Need to make this changable.
            ChatTwo_Client_Protocol.ServerAddress = new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 87, 52, 32, 46 }), 9020);
#endif

            StartUdpClient(0);
            //StartUdpClient(9020);

            notifyIcon1.BalloonTipTitle = this.Name;
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private bool StartUdpClient(int port)
        {
            bool worked = _client.Start(port);
            if (worked)
                MessageBox.Show(this, "UDP server started on port " + _client.Port + ".", "UdpCommunication");
            else
                MessageBox.Show(this, "UDP server failed on port " + port + ".", "UdpCommunication");
            return worked;
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLogin loggingin = new FormLogin())
            {
                loggingin.ShowDialog(this);
                if (loggingin.DialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    // !?!?!?! Logged in?
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        bool _closing = false;
        private void CloseForm()
        {
            _client.Stop();
            _closing = true;
            this.Close();
        }

        #region Minimize to Tray
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_closing)
            {
                e.Cancel = true;
                this.Hide();
                TrayBalloonTip("Minimized to tray", ToolTipIcon.None);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreForm();
        }

        private void RestoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreForm();
        }

        private void RestoreForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void TrayBalloonTip(string message, ToolTipIcon toolTipIcon, int time = 500)
        {
            notifyIcon1.BalloonTipIcon = toolTipIcon;
            notifyIcon1.BalloonTipText = message;
            notifyIcon1.ShowBalloonTip(time);
        }
        #endregion
    }
}
