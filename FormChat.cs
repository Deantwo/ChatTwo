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
                return "Asked";
            else if (!RelationshipFrom)
                return "Asking";
            else
                return "Offline";
        }
    }
}
