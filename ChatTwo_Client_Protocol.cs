using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatTwo
{
    class ChatTwo_Client_Protocol
    {
        private static List<ContactObj> _contacts = new List<ContactObj>();
        public static List<ContactObj> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        private static string _serverSharedSecret = "";
        public static string ServerSharedSecret
        {
            get { return _serverSharedSecret; }
            set { _serverSharedSecret = value; }
        }

        private static IPEndPoint _serverAddress;
        public static IPEndPoint ServerAddress
        {
            get { return _serverAddress; }
            set { _serverAddress = value; }
        }

        private static UserObj _user;
        public static UserObj User
        {
            get { return _user; }
            set { _user = value; }
        }

        private static bool _loggedIn = false;
        public static bool LoggedIn
        {
            get { return _loggedIn; }
            set { _loggedIn = value; }
        }

        private static Thread _threadKeepalive;

        public static void LogIn(int userId)
        {
            _user = new UserObj();
            _user.ID = userId;
            _loggedIn = true;

            _threadKeepalive = new Thread(() => Keepalive());
            _threadKeepalive.Name = "Keepalive Thread (Keepalive method)";
            _threadKeepalive.Start();
        }

        public static void LogOut()
        {
            _loggedIn = false;

            //_threadKeepalive.Abort();
            _threadKeepalive.Join();

            _user = null;
            _contacts.Clear();
            OnContactUpdate();
        }

        private static void Keepalive() // Threaded looping method.
        {
            try
            {
                while (_loggedIn)
                {
                    Thread.Sleep(500);
                    List<ContactObj> onlineContacts = _contacts.FindAll(x => x.Online == true);
                    byte[] contactIds = new byte[0];
                    foreach (ContactObj contact in onlineContacts)
                    {
                        byte[] contactId = BitConverter.GetBytes(contact.ID);
                        contactIds = ByteHelper.ConcatinateArray(contactIds, contactId);
                    }
                    ChatTwo_Client_Protocol.MessageToServer(ChatTwo_Protocol.MessageType.Status, contactIds, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("### " + _threadKeepalive.Name + " has crashed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
            }
        }

        public static void MessageReceivedHandler(object sender, PacketReceivedEventArgs args)
        {
            if (args.Data[0] == 0x92)
            {
                string sharedSecret;
                // Position of the Type byte is 30 (SignatureByteLength + MacByteLength + TimezByteLength + UserIdByteLength).
                ChatTwo_Protocol.MessageType type = (ChatTwo_Protocol.MessageType)args.Data[ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4 + 4];
                // Position of the UserID bytes is 26 (SignatureByteLength + MacByteLength + TimezByteLength) with a length of 4.
                int senderId = ByteHelper.ToInt32(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4);
                if (type == ChatTwo_Protocol.MessageType.CreateUserReply)
                {
                    sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
                }
                else if (senderId == 0)
                {
                    sharedSecret = ServerSharedSecret;
                }
                else
                {
                    sharedSecret = _contacts.Find(x => x.ID == senderId).Secret;

                    // Testing!!!! REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
                }

                if (ChatTwo_Protocol.ValidateMac(args.Data, sharedSecret))
                {
                    Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

                    switch (message.Type)
                    {
                        case ChatTwo_Protocol.MessageType.CreateUserReply:
                            {
                                // Fire an OnCreateUserReply event.
                                CreateUserReplyEventArgs argsCreateUser = new CreateUserReplyEventArgs();
                                argsCreateUser.Success = message.Data[0] == 0x00;
                                switch (message.Data[0])
                                {
                                    case 0: // Success.
                                        break;
                                    case 1: // Username already exist.
                                        argsCreateUser.Message = "A user already exist with that name.";
                                        break;
                                    case 2:
                                        argsCreateUser.Message = "Username is too short or too long.";
                                        break;
                                }
                                OnCreateUserReply(argsCreateUser);
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.LoginReply:
                            {
                                // Fire an OnLoginReply event.
                                LoginReplyEventArgs argsLogin = new LoginReplyEventArgs();
                                argsLogin.Success = message.Data[0] == 0x00;
                                switch (message.Data[0])
                                {
                                    case 0: // Success.
                                        int userId = ByteHelper.ToInt32(message.Data, 1);
                                        string username = Encoding.Unicode.GetString(ByteHelper.SubArray(message.Data, 5));
                                        LogIn(userId);
                                        _user.Name = username;
                                        argsLogin.Name = username;
                                        break;
                                    case 1: // Wrong password.
                                        argsLogin.Message = "Wrong username or password.";
                                        break;
                                    case 2: // Already online.
                                        argsLogin.Message = "That user is already online.";
                                        break;
                                }
                                OnLoginReply(argsLogin);
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.ContactRequestReply:
                            {
                                // Fire an OnAddContactReply event.
                                AddContactReplyEventArgs argsAddContact = new AddContactReplyEventArgs();
                                argsAddContact.Success = message.Data[0] == 0x00;
                                switch (message.Data[0])
                                {
                                    case 0: // Success.
                                        break;
                                    case 1: // No user with that name.
                                        argsAddContact.Message = "No user with that name.";
                                        break;
                                    case 2: // You can't add your self.
                                        argsAddContact.Message = "You can't add your self.";
                                        break;
                                    case 3: // User is already a contact.
                                        argsAddContact.Message = "User is already a contact.";
                                        break;
                                }
                                OnAddContactReply(argsAddContact);
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.ContactStatus:
                            {
                                int contactId = ByteHelper.ToInt32(message.Data, 0);
                                int nameLength = ((31 & message.Data[4]) * 2);
                                ContactObj contact;
                                if (_contacts.Any(x => x.ID == contactId))
                                    contact = _contacts.Find(x => x.ID == contactId);
                                else
                                {
                                    contact = new ContactObj();
                                    contact.ID = contactId;
                                    contact.Name = Encoding.Unicode.GetString(message.Data, 5, nameLength);
                                    _contacts.Add(contact);
                                }
                                contact.Online = ByteHelper.CheckBitCodeIndex(message.Data[4], 7);
                                contact.RelationshipTo = ByteHelper.CheckBitCodeIndex(message.Data[4], 6);
                                contact.RelationshipFrom = ByteHelper.CheckBitCodeIndex(message.Data[4], 5);
                                if (contact.Online)
                                {   
                                    int port = ByteHelper.ToInt32(message.Data, 5 + nameLength);
                                    contact.Socket = new IPEndPoint(new IPAddress(ByteHelper.SubArray(message.Data, 5 + nameLength + 4)), port);
                                }
                                // Fire an OnContactUpdate event.
                                OnContactUpdate();
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.Message:
                            {
                                ContactObj contact;
                                if (_contacts.Any(x => x.ID == message.From && x.RelationshipTo && x.RelationshipFrom))
                                {
                                    contact = _contacts.Find(x => x.ID == message.From);
                                    OpenChat(contact.ID);
                                    message.Text = Encoding.Unicode.GetString(message.Data);
                                    contact.ChatWindow.ReceiveMessage(message.Text);
                                }
                                else
#if DEBUG
                                    throw new NotImplementedException("You received a message from someone that isn't your contact?");
#else
                                    return;
#endif
                                break;
                            }
                    }
                }
#if DEBUG
                else
                    throw new NotImplementedException("Could not validate the MAC of received message.");
                    // Need to add a simple debug message here, but this works as a great breakpoint until then.
#endif
            }
#if DEBUG
            else
                throw new NotImplementedException("Could not validate the signature of the received message. The signature was \"0x" + args.Data[0] + "\" but only \"0x92\" is allowed.");
                // Need to add a simple debug message here, but this works as a great breakpoint until then.
#endif
        }

        private static void OnCreateUserReply(CreateUserReplyEventArgs e)
        {
            EventHandler<CreateUserReplyEventArgs> handler = CreateUserReply;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<CreateUserReplyEventArgs> CreateUserReply;

        private static void OnLoginReply(LoginReplyEventArgs e)
        {
            EventHandler<LoginReplyEventArgs> handler = LoginReply;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<LoginReplyEventArgs> LoginReply;

        private static void OnAddContactReply(AddContactReplyEventArgs e)
        {
            EventHandler<AddContactReplyEventArgs> handler = AddContactReply;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<AddContactReplyEventArgs> AddContactReply;

        private static void OnContactUpdate()
        {
            EventHandler<EventArgs> handler = ContactUpdate;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }
        public static event EventHandler<EventArgs> ContactUpdate;

        public static void OpenChat(int userId)
        {
            ContactObj contact = ChatTwo_Client_Protocol.Contacts.Find(x => x.ID == userId);
            if (contact.ChatWindow != null)
            {
                if (!contact.ChatWindow.Visible)
                    contact.ChatWindow.Show();
            }
            else
                new FormChat(contact).Show();
        }

        public static void MessageToServer(ChatTwo_Protocol.MessageType type, byte[] data = null, string text = null)
        {
            Message message = new Message();
            if (_user != null)
                message.From = _user.ID;
            else
                message.From = Int32.MaxValue;
            message.To = ChatTwo_Protocol.ServerReserrvedUserID;
            message.Type = type;
            if (data != null && data.Length != 0)
                message.Data = data;
            if (!String.IsNullOrEmpty(text))
                message.Text = text;
            message.Ip = _serverAddress;
            MessageTransmissionHandler(message);
        }

        public static void MessageToUser(int to, ChatTwo_Protocol.MessageType type, byte[] data = null, string text = null)
        {
            Message message = new Message();
            message.From = _user.ID;
            message.To = to;
            message.Type = type;
            if (data != null && data.Length != 0)
                message.Data = data;
            if (!String.IsNullOrEmpty(text))
                message.Text = text;
            if (_contacts.Any(x => x.ID == to))
                message.Ip = _contacts.Find(x => x.ID == to).Socket;
            MessageTransmissionHandler(message);
        }

        public static void MessageTransmissionHandler(Message message)
        {
            byte[] messageBytes = ChatTwo_Protocol.MessageTransmissionHandler(message);

            string sharedSecret;
            if (message.Type == ChatTwo_Protocol.MessageType.CreateUser)
            {
                sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
            }
            else if (message.Type == ChatTwo_Protocol.MessageType.Login)
            {
                ServerSharedSecret = ByteHelper.GetHashString(messageBytes);
                sharedSecret = ServerSharedSecret;
            }
            else if (message.To == ChatTwo_Protocol.ServerReserrvedUserID)
            {
                sharedSecret = ServerSharedSecret;
            }
            else
            {
                int userId = message.To;
                sharedSecret = _contacts.Find(x => x.ID == userId).Secret;

                // Testing!!!! REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
            }

            messageBytes = ChatTwo_Protocol.AddSignatureAndMac(messageBytes, sharedSecret);

            // Fire an OnMessageTransmission event.
            PacketTransmissionEventArgs args = new PacketTransmissionEventArgs();
            args.Destination = message.Ip;
            args.PacketContent = messageBytes;
            OnMessageTransmission(args);
        }

        private static void OnMessageTransmission(PacketTransmissionEventArgs e)
        {
            EventHandler<PacketTransmissionEventArgs> handler = MessageTransmission;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<PacketTransmissionEventArgs> MessageTransmission;
    }

    public class CreateUserReplyEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class LoginReplyEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class AddContactReplyEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
