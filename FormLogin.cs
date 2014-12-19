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
    public partial class FormLogin : Form
    {
        private bool _waitingForLoginReply = false;

        private string _loggedInUsername;
        public string Username
        {
            get { return _loggedInUsername; }
        }

        public FormLogin()
        {
            InitializeComponent();

            lblResult.Text = "";
            tbxUsername.Focus();
            // Set the position of the window to the center of the parent.
            this.StartPosition = FormStartPosition.CenterParent;

            ChatTwo_Client_Protocol.LoginReply += LoginReply;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using (FormRegister registering = new FormRegister())
            {
                registering.ShowDialog(this);
                if (registering.DialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    tbxUsername.Text = registering.Username;
                    tbxPassword.Focus();
                }
            }
        }

        private void ResetWindow()
        {
            lblUsername.ForeColor = Color.Black;
            tbxUsername.ForeColor = Color.Black;
            lblPassword.ForeColor = Color.Black;
            tbxPassword.ForeColor = Color.Black;
            lblResult.ForeColor = Color.Black;
            lblResult.Text = "";
        }

        private void ResetControls()
        {
            btnLogin.Enabled = true;
            btnRegister.Enabled = true;
            btnCancel.Enabled = true;

            _waitingForLoginReply = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            ResetWindow();

            // If there is no username entered.
            if (tbxUsername.Text == "")
            {
                lblUsername.ForeColor = Color.Red;
                tbxUsername.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "You did not enter a username.";
                return;
            }

            // If there is no password entered.
            if (tbxPassword.Text == "")
            {
                lblPassword.ForeColor = Color.Red;
                tbxPassword.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "You did not enter a password.";
                return;
            }

            btnLogin.Enabled = false;
            btnRegister.Enabled = false;

            lblResult.Text = "Contacting server...";
            _waitingForLoginReply = true;
            byte[] passwordHash = ByteHelper.GetHashBytes(Encoding.Unicode.GetBytes(tbxPassword.Text));
            ChatTwo_Client_Protocol.MessageToServer(ChatTwo_Protocol.MessageType.Login, passwordHash, tbxUsername.Text);
            timer1.Start();
        }

        public void LoginReply(object sender, LoginReplyEventArgs args)
        {
            if (lblResult.InvokeRequired)
            { // Needed for multi-threading cross calls.
                this.Invoke(new Action<object, LoginReplyEventArgs>(this.LoginReply), new object[] { sender, args });
            }
            else
            {
                if (_waitingForLoginReply)
                {
                    _waitingForLoginReply = false;
                    timer1.Stop();
                    if (args.Success)
                    {
                        lblResult.Text = "Login successful!";
                        _loggedInUsername = args.Name;
                        this.Close();
                        this.DialogResult = System.Windows.Forms.DialogResult.Yes;
                        return;
                    }
                    else
                    {
                        lblResult.ForeColor = Color.Red;
                        lblResult.Text = args.Message;
                        btnRegister.Enabled = true;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_waitingForLoginReply)
            {
                ResetControls();
                timer1.Stop();
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "No response from server.";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_waitingForLoginReply)
            {
                ResetControls();
                timer1.Stop();
                ResetWindow();
                lblResult.Text = "Login canceled.";
            }
            else
            {
                this.Close();
                return;
            }
        }
    }
}
