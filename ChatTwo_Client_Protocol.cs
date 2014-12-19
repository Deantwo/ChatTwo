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
        private static List<UserObj> _contacts = new List<UserObj>();
        public static List<UserObj> Contacts
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

        private static UserObj _user = new UserObj();
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
        }

        private static void Keepalive() // Threaded looping method.
        {
            try
            {
                while (_loggedIn)
                {
                    Thread.Sleep(500);
                    ChatTwo_Client_Protocol.MessageToServer(ChatTwo_Protocol.MessageType.Status, null, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("### " + _threadKeepalive.Name + " has crashed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
            }
        }

        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            if (args.Data[0] == 0x92)
            {
                string sharedSecret;
                // Position of the Type byte is 30 (SignatureByteLength + MacByteLength + TimezByteLength + UserIdByteLength).
                ChatTwo_Protocol.MessageType type = (ChatTwo_Protocol.MessageType)args.Data[ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4 + 4];
                if (type == ChatTwo_Protocol.MessageType.CreateUserReply)
                {
                    sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
                }
                else if (type == ChatTwo_Protocol.MessageType.LoginReply)
                {
                    sharedSecret = ServerSharedSecret;
                }
                else
                {
                    // Position of the UserID bytes is 26 (SignatureByteLength + MacByteLength + TimezByteLength) with a length of 4.
                    int userId = ByteHelper.ToInt32(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4);
                    sharedSecret = _contacts.Find(x => x.ID == userId).Secret;
                }

                if (ChatTwo_Protocol.ValidateMac(args.Data, sharedSecret))
                {
                    Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

                    IPEndPoint messageSender = message.Ip;
                    //type = message.Type;
                    byte[] messageBytes = message.Data;

                    byte[] messageData = new byte[0];
                    string messageText = "";

                    switch (message.Type)
                    {
                        case ChatTwo_Protocol.MessageType.CreateUserReply:
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
                            }
                            OnCreateUserReply(argsCreateUser);
                            break;
                        case ChatTwo_Protocol.MessageType.LoginReply:
                            // Fire an OnLoginReply event.
                            LoginReplyEventArgs argsLogin = new LoginReplyEventArgs();
                            argsLogin.Success = message.Data[0] == 0x00;
                            switch (message.Data[0])
                            {
                                case 0: // Success.
                                    int userId = ByteHelper.ToInt32(message.Data, 1);
                                    string username = Encoding.Unicode.GetString(ByteHelper.SubArray(message.Data, 5));
                                    argsLogin.Name = username;
                                    LogIn(userId);
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
                        case ChatTwo_Protocol.MessageType.Message:
                            messageData = ByteHelper.SubArray(args.Data, 0, 7);
                            messageText = Encoding.Unicode.GetString(ByteHelper.SubArray(messageBytes, 8));
                            break;
                    }
                }
                else
                    throw new NotImplementedException("Could not validate the MAC of received message.");
                    // Need to add a simple debug message here, but this works as a great breakpoint until then.
            }
            else
                throw new NotImplementedException("Could not validate the signature of the received message. The signature was \"0x" + args.Data[0] + "\" but only \"0x92\" is allowed.");
                // Need to add a simple debug message here, but this works as a great breakpoint until then.
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

        public static void MessageToServer(ChatTwo_Protocol.MessageType type, byte[] data = null, string text = null)
        {
            Message message = new Message();
            message.From = _user.ID;
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
            }

            messageBytes = ChatTwo_Protocol.AddSignatureAndMac(messageBytes, sharedSecret);

            // Fire an OnMessageTransmission event.
            MessageTransmissionEventArgs args = new MessageTransmissionEventArgs();
            args.Ip = message.Ip;
            args.MessageBytes = messageBytes;
            OnMessageTransmission(args);
        }

        private static void OnMessageTransmission(MessageTransmissionEventArgs e)
        {
            EventHandler<MessageTransmissionEventArgs> handler = MessageTransmission;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<MessageTransmissionEventArgs> MessageTransmission;
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
}
