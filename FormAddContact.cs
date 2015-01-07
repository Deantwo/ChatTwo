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
    public partial class FormAddContact : Form
    {
        private bool _waitingForAddContactReply = false;

        public FormAddContact()
        {
            InitializeComponent();

            lblResult.Text = "";
            tbxContactName.Focus();
            // Set the position of the window to the center of the parent.
            this.StartPosition = FormStartPosition.CenterParent;

            ChatTwo_Client_Protocol.AddContactReply += AddContactReply;
        }

        private void ResetWindow()
        {
            lblContactName.ForeColor = Color.Black;
            tbxContactName.ForeColor = Color.Black;
            lblResult.ForeColor = Color.Black;
            lblResult.Text = "";
        }

        private void btnSendRequest_Click(object sender, EventArgs e)
        {
            ResetWindow();

            // If there is no contact name entered.
            if (tbxContactName.Text == "")
            {
                lblContactName.ForeColor = Color.Red;
                tbxContactName.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "You did not enter a contact name.";
                return;
            }

            // You can't add your self. (The Server also checks this.)
            if (tbxContactName.Text == ChatTwo_Client_Protocol.User.Name)
            {
                lblContactName.ForeColor = Color.Red;
                tbxContactName.ForeColor = Color.Red;
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "You can't add your self.";
                return;
            }

            btnSendRequest.Enabled = false;
            btnCancel.Enabled = false;
            tbxContactName.ReadOnly = true;

            lblResult.Text = "Contacting server...";
            _waitingForAddContactReply = true;
            ChatTwo_Client_Protocol.MessageToServer(ChatTwo_Protocol.MessageType.ContactRequest, null, tbxContactName.Text);
            timer1.Start();
        }

        public void AddContactReply(object sender, AddContactReplyEventArgs args)
        {
            if (lblResult.InvokeRequired)
            { // Needed for multi-threading cross calls.
                this.Invoke(new Action<object, AddContactReplyEventArgs>(this.AddContactReply), new object[] { sender, args });
            }
            else
            {
                if (_waitingForAddContactReply)
                {
                    _waitingForAddContactReply = false;
                    timer1.Stop();
                    if (args.Success)
                    {
                        lblResult.Text = "Contact added successful!";
                        this.Close();
                        this.DialogResult = System.Windows.Forms.DialogResult.Yes;
                        return;
                    }
                    else
                    {
                        lblResult.ForeColor = Color.Red;
                        lblResult.Text = args.Message;
                        btnSendRequest.Enabled = true;
                        btnCancel.Enabled = true;
                        tbxContactName.ReadOnly = false;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_waitingForAddContactReply)
            {
                _waitingForAddContactReply = false;
                btnSendRequest.Enabled = true;
                btnCancel.Enabled = true;
                tbxContactName.ReadOnly = false;
                timer1.Stop();
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "No response from server.";
            }
        }
    }
}
