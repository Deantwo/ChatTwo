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
            btnRegister.Enabled = false;
            btnCancel.Enabled = false;

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

            lblResult.Text = "Contacting server...";
            byte[] passwordHash = ByteHelper.GetHashBytes(Encoding.Unicode.GetBytes(tbxPassword1.Text));
            ChatTwo_Client_Protocol.MessageToServer(ChatTwo_Protocol.MessageType.CreateUser, passwordHash, tbxUsername.Text);
        }

        public void CreateUserReply(object sender, CreateUserReplyEventArgs args)
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
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    return;
                }
                else
                {
                    lblResult.ForeColor = Color.Red;
                    lblResult.Text = args.Message;
                    btnRegister.Enabled = true;
                    btnCancel.Enabled = true;
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
