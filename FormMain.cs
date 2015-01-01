﻿using System;
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
            _client.PacketReceived += ChatTwo_Client_Protocol.MessageReceivedHandler;
            ChatTwo_Client_Protocol.MessageTransmission += _client.SendPacket;

#if DEBUG
            // Localhost as server addressed used for easier testing.
            ChatTwo_Client_Protocol.ServerAddress = new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 9020);
#else
            // My server IP and port.
            // Need to make this changable.
            ChatTwo_Client_Protocol.ServerAddress = new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 87, 52, 32, 46 }), 9020);
#endif
            MessageBox.Show(this, "ServerAddress set to " + ChatTwo_Client_Protocol.ServerAddress.ToString() + ".", "ChatTwo ServerAddress");

            StartUdpClient(0);
            //StartUdpClient(9020);

            notifyIcon1.BalloonTipTitle = this.Text;
            notifyIcon1.Text = this.Text;
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private bool StartUdpClient(int port)
        {
            bool worked = _client.Start(port);
#if DEBUG
            if (worked)
                MessageBox.Show(this, "UDP server started on port " + _client.Port + ".", "UdpCommunication");
            else
                MessageBox.Show(this, "UDP server failed on port " + port + ".", "UdpCommunication");
#endif
            return worked;
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLogin loggingin = new FormLogin())
            {
                loggingin.ShowDialog(this);
                if (loggingin.DialogResult == System.Windows.Forms.DialogResult.Yes) // The FormLogin's DialogResult is only set to "Yes" if it was closed by a successful login.
                {
                    // !?!?!?! Logged in?
                    MessageBox.Show(this, "You have successfully logged in. This is about as far as the prototype goes.", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnAddContact.Enabled = true;
                    dgvContacts.Enabled = true;
                    loginToolStripMenuItem.Enabled = false;
                    logoutToolStripMenuItem.Enabled = true;
                    toolStripStatusLabel1.Text = "Logged in as " + loggingin.Username;
                }
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This feature is sadly not implemented yet." + Environment.NewLine +
                "" + Environment.NewLine +
                "Currently the server just detects that you have timed out, but it doesn't forget you were online." + Environment.NewLine +
                "To try again, please restart the server.", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnAddContact.Enabled = false;
            dgvContacts.Enabled = false;
            logoutToolStripMenuItem.Enabled = false;
            ChatTwo_Client_Protocol.LogOut();
            loginToolStripMenuItem.Enabled = true;
        }

        private void btnAddContact_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This feature is sadly not implemented yet.", "Add Contact", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        #region Closing Minimize to Tray
        bool _closing = false;
        private void CloseForm()
        {
            // Exiting the program for real.
            _closing = true;
            this.Close();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if we are exiting the program, or just hiding it.
            if (!_closing)
            {
                e.Cancel = true;
                this.Hide();
                TrayBalloonTip("Minimized to tray", ToolTipIcon.None);
                return;
            }

            // We are exting the program, stop all threaded workers and stuff.
            _client.Stop();
            if (ChatTwo_Client_Protocol.LoggedIn)
                ChatTwo_Client_Protocol.LogOut();
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
