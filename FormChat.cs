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
    public partial class FormChat : Form
    {
        ContactObj _contact;

        public FormChat(ContactObj contact)
        {
            InitializeComponent();
            _contact = contact;
            _contact.ChatWindow = this;
            this.Text = "ChatTwo Chat (" + _contact.Name + ")";
        }

        public void ReceiveMessage(string message)
        {
            if (rtbxChat.InvokeRequired)
            { // Needed for multi-threading cross calls.
                this.Invoke(new Action<string>(this.ReceiveMessage), new object[] { message });
            }
            else
            {
                FlashWindow.Flash(this);
                WriteMessage(_contact.Name + ": " + message);
            }
        }

        private void WriteMessage(string message, int colorARGB = -16777216) // 0xFF000000 (Color.Black)
        {
                // Add timestamp to the log entry.
                string timestamp = DateTime.Now.ToString("HH:mm:ss"); // "yyyy-MM-dd HH:mm:ss"
                message = timestamp + " " + message;
                // And this part only matters for the "### Start\n    ..." message.
                if (message.Contains(Environment.NewLine))
                {
                    int i = timestamp.Length;
                    timestamp = "";
                    for (; i > 0; i--)
                        timestamp += " ";
                    message = message.Replace(Environment.NewLine, Environment.NewLine + timestamp + " ");
                }

                // Just to prevent the first line from being empty.
                if (rtbxChat.Text != String.Empty)
                    rtbxChat.AppendText(Environment.NewLine);

                int lengthBeforeAppend = rtbxChat.Text.Length;
                // Write log to the textbox.
                rtbxChat.AppendText(message);
                // Put the focus on the textbox.
                rtbxChat.Focus();
                // Color the text.
                rtbxChat.SelectionStart = lengthBeforeAppend;
                rtbxChat.SelectionLength = message.Length;
                rtbxChat.SelectionColor = Color.FromArgb(colorARGB);

                // Delete the top line when there is over 1000 lines.
                if (rtbxChat.Lines.Length > 1000)
                {
                    rtbxChat.SelectionStart = 0;
                    rtbxChat.SelectionLength = rtbxChat.GetFirstCharIndexFromLine(1);
                    rtbxChat.ReadOnly = false; // Can't edit the text when the RichTextBox is in ReadOnly mode.
                    rtbxChat.SelectedText = String.Empty;
                    rtbxChat.ReadOnly = true;
                }

                // Put the cursor at the end of the text.
                rtbxChat.Select(rtbxChat.Text.Length, 0);
                // Scroll to the bottom of the textbox.
                rtbxChat.ScrollToCaret();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendChatMessage();
        }

        private void tbxSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers != Keys.Shift)
            {
                SendChatMessage();
                e.SuppressKeyPress = true;
            }
        }

        private void SendChatMessage()
        {
            ChatTwo_Client_Protocol.MessageToUser(_contact.ID, ChatTwo_Protocol.MessageType.Message, null, tbxSend.Text);
            WriteMessage(ChatTwo_Client_Protocol.User.Name + ": " + tbxSend.Text, Color.Blue.ToArgb());
            tbxSend.Clear();
            tbxSend.Focus();
        }

        private void FormChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }

    public class ContactObj : UserObj
    {
        public FormChat ChatWindow { set; get; }
        public bool RelationshipTo { set; get; }
        public bool RelationshipFrom { set; get; }
        
        public ContactObj()
        {
        }

        public string GetStatus()
        {
            if (Online)
                return "Online";
            else if (!RelationshipTo)
                return "Not Mutual";
            else if (!RelationshipFrom)
                return "Request";
            else
                return "Offline";
        }
    }
}
