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
    public partial class FormRegister : Form
    {
        public string Username
        {
            get { return tbxUsername.Text; }
        }

        private bool _waitingForCreateUserReply = false;

        private int _newUserId = 0;
        public int UserId
        {
            get { return _newUserId; }
        }

        public FormRegister()
        {
            InitializeComponent();

            lblResult.Text = "";
            tbxUsername.Focus();
            // Set the position of the window to the center of the parent.
            this.StartPosition = FormStartPosition.CenterParent;

            ChatTwo_Client_Protocol.CreateUserReply += CreateUserReply;
        }

        private void ResetWindow()
        {
            lblUsername.ForeColor = Color.Black;
            tbxUsername.ForeColor = Color.Black;
            lblPassword1.ForeColor = Color.Black;
            tbxPassword1.ForeColor = Color.Black;
            lblPassword2.ForeColor = Color.Black;
            tbxPassword2.ForeColor = Color.Black;
            lblResult.ForeColor = Color.Black;
            lblResult.Text = "";
        }

        private void btnRegister_Click(object sender, EventArgs e)
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

            // If the username is too long.
            if (tbxUsername.Text.Length > 30)
            {
                lblUsername.ForeColor = Color.Red;
                tbxUsername.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "The username is too long. Please use 30 or less characters.";
                return;
            }

            // If there is no password entered.
            if (tbxPassword1.Text == "")
            {
                lblPassword1.ForeColor = Color.Red;
                tbxPassword1.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "You did not enter a password.";
                return;
            }

            // If the password and the confirm password textboxes aren't the same.
            if (tbxPassword1.Text != tbxPassword2.Text)
            {
                lblPassword2.ForeColor = Color.Red;
                tbxPassword2.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "The two passwords are not the same.";
                return;
            }

            // If the password is too short.
            // (I hate strict password rules! If it is not a bank or social security thing, don't force the uesr to make insane passwords.)
            if (tbxPassword1.Text.Length < 4)
            {
                lblPassword1.ForeColor = Color.Red;
                tbxPassword1.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "The password is too short. Please use 4 or more characters.";
                return;
            }

            btnRegister.Enabled = false;
            btnCancel.Enabled = false;
            tbxUsername.ReadOnly = true;
            tbxPassword1.ReadOnly = true;
            tbxPassword2.ReadOnly = true;

            lblResult.Text = "Contacting server...";
            _waitingForCreateUserReply = true;
            byte[] passwordHash = ByteHelper.GetHashBytes(Encoding.Unicode.GetBytes(tbxPassword1.Text));
            ChatTwo_Client_Protocol.MessageToServer(ChatTwo_Protocol.MessageType.CreateUser, passwordHash, tbxUsername.Text);
            timer1.Start();
        }

        public void CreateUserReply(object sender, CreateUserReplyEventArgs args)
        {
            if (lblResult.InvokeRequired)
            { // Needed for multi-threading cross calls.
                this.Invoke(new Action<object, CreateUserReplyEventArgs>(this.CreateUserReply), new object[] { sender, args });
            }
            else
            {
                if (_waitingForCreateUserReply)
                {
                    _waitingForCreateUserReply = false;
                    timer1.Stop();
                    if (args.Success)
                    {
                        lblResult.Text = "User created successful!";
                        //_newUserId = args.ID;
                        this.Close();
                        this.DialogResult = System.Windows.Forms.DialogResult.Yes;
                        return;
                    }
                    else
                    {
                        lblResult.ForeColor = Color.Red;
                        lblResult.Text = args.Message;
                        btnRegister.Enabled = true;
                        btnCancel.Enabled = true;
                        tbxUsername.ReadOnly = false;
                        tbxPassword1.ReadOnly = false;
                        tbxPassword2.ReadOnly = false;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_waitingForCreateUserReply)
            {
                _waitingForCreateUserReply = false;
                btnRegister.Enabled = true;
                btnCancel.Enabled = true;
                tbxUsername.ReadOnly = false;
                tbxPassword1.ReadOnly = false;
                tbxPassword2.ReadOnly = false;
                timer1.Stop();
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "No response from server.";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!_waitingForCreateUserReply)
            {
                this.Close();
                return;
            }
        }
    }
}
