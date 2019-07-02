using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RefereeAssistant3.IRC
{
    /// <summary>
    /// IRC Client class written at http://tech.reboot.pro
    /// Modified for RA3
    /// </summary>
    public class IrcClient
    {
        #region Variables

        // private TcpClient used to talk to the server
        private TcpClient irc;

        // private network stream used to read/write from/to
        private NetworkStream stream;

        // global variable used to read input from the client
        private string inputLine;

        // stream reader to read from the network stream
        private StreamReader reader;

        // stream writer to write to the stream
        private StreamWriter writer;

        #endregion

        #region Constructors
        /// <summary>
        /// IrcClient used to connect to an IRC Server (default port: 6667)
        /// </summary>
        /// <param name="Server">IRC Server</param>
        public IrcClient(string Server, bool shouldOutput) : this(Server, 6667, shouldOutput) { }

        /// <summary>
        /// IrcClient used to connect to an IRC Server
        /// </summary>
        /// <param name="Server">IRC Server</param>
        /// <param name="Port">IRC Port (6667 if you are unsure)</param>
        public IrcClient(string Server, int Port, bool shouldOutput)
        {
            this.Server = Server;
            this.Port = Port;
            ConsoleOutput = shouldOutput;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the Server address used
        /// </summary>
        public string Server { get; } = "";
        /// <summary>
        /// Returns the Port used
        /// </summary>
        public int Port { get; } = 6667;
        /// <summary>
        /// Returns the password used to auth to the server
        /// </summary>
        public string ServerPass { get; set; } = "";
        /// <summary>
        /// Returns the current nick being used.
        /// </summary>
        public string Nick { get; set; } = "Test";
        /// <summary>
        /// Returns the alternate nick being used
        /// </summary>
        public string AltNick { get; set; } = "";
        /// <summary>
        /// Output RAW IRC data to console
        /// </summary>
        public bool ConsoleOutput { get; set; } = false;
        /// <summary>
        /// Returns true if the client is connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (irc != null)
                    if (irc.Connected)
                        return true;
                return false;
            }
        }
        #endregion

        #region Events

        public event Action<UpdateUsersEventArgs> UpdateUsers = delegate { };
        public event Action<UserJoinedEventArgs> UserJoined = delegate { };
        public event Action<UserLeftEventArgs> UserLeft = delegate { };
        public event Action<UserNickChangedEventArgs> UserNickChange = delegate { };

        public event Action<ChannelMessageEventArgs> ChannelMessage = delegate { };
        public event Action<NoticeMessageEventArgs> NoticeMessage = delegate { };
        public event Action<PrivateMessageEventArgs> PrivateMessage = delegate { };
        public event Action<StringEventArgs> ServerMessage = delegate { };

        public event Action<StringEventArgs> NickTaken = delegate { };

        public event Action OnConnect = delegate { };

        public event Action<Exception> ExceptionThrown = delegate { };

        public event Action<ModeSetEventArgs> ChannelModeSet = delegate { };
        #endregion

        #region PublicMethods
        /// <summary>
        /// Connect to the IRC server
        /// </summary>
        public void Connect()
        {
            //var t = new Thread(DoConnect) { IsBackground = true };
            //t.Start();
            DoConnect();
        }
        private void DoConnect()
        {
            try
            {
                irc = new TcpClient(Server, Port);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                if (!string.IsNullOrEmpty(ServerPass))
                    Send("PASS " + ServerPass);

                Send("NICK " + Nick);
                Send("USER " + Nick + " 0 * :" + Nick);

                Listen();
            }
            catch (Exception ex)
            {
                ExceptionThrown(ex);
            }
        }
        /// <summary>
        /// Disconnect from the IRC server
        /// </summary>
        public void Disconnect()
        {
            if (irc != null)
            {
                if (irc.Connected)
                {
                    Send("QUIT Client Disconnected");
                }
                irc = null;
            }
        }
        /// <summary>
        /// Sends the JOIN command to the server
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void JoinChannel(string channel)
        {
            if (irc != null && irc.Connected)
            {
                Send("JOIN " + channel);
            }
        }
        /// <summary>
        /// Sends the PART command for a given channel
        /// </summary>
        /// <param name="channel">Channel to leave</param>
        public void PartChannel(string channel) => Send("PART " + channel);
        /// <summary>
        /// Send a notice to a user
        /// </summary>
        /// <param name="toNick">User to send the notice to</param>
        /// <param name="message">The message to send</param>
        public void SendNotice(string toNick, string message) => Send("NOTICE " + toNick + " :" + message);

        /// <summary>
        /// Send a message to the channel
        /// </summary>
        /// <param name="channel">Channel to send message</param>
        /// <param name="message">Message to send</param>
        public void SendMessage(string channel, string message) => Send("PRIVMSG " + channel + " :" + message);
        /// <summary>
        /// Send RAW IRC commands
        /// </summary>
        /// <param name="message"></param>
        public void SendRaw(string message) => Send(message);

        public void GetChannelUsers(string channel) => Send($"NAMES {channel}");

        public void Dispose()
        {
            stream.Dispose();
            writer.Dispose();
            reader.Dispose();
        }
        #endregion

        #region PrivateMethods
        /// <summary>
        /// Listens for messages from the server
        /// </summary>
        private async void Listen()
        {
            while ((inputLine = reader.ReadLine()) != null)
            {
                //try
                //{
                ParseData(inputLine);
                if (ConsoleOutput) Console.Write(inputLine);
                //}
                //catch (Exception ex)
                //{
                //ExceptionThrown(ex);
                //}
                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Parses data sent from the server
        /// </summary>
        /// <param name="data">message from the server</param>
        private void ParseData(string data)
        {
            // split the data into parts
            var ircData = data.Split(' ');

            if (ircData.Length < 2)
                return;
            var ircCommand = ircData[1];

            // if the message starts with PING we must PONG back
            if (data.Length > 4)
            {
                if (data.Substring(0, 4) == "PING")
                {
                    Send("PONG " + ircData[1]);
                    return;
                }

            }

            // re-act according to the IRC Commands
            switch (ircCommand)
            {
                case "001": // server welcome message, after this we can join
                    Send("MODE " + Nick + " +B");
                    OnConnect?.Invoke();    //TODO: this might not work
                    break;
                case "353": // member list
                {
                    var channel = ircData[4];
                    var userList = JoinArray(ircData, 5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    UpdateUsers(new UpdateUsersEventArgs(channel, userList));
                }
                break;
                case "401":
                case "403":
                case "404":
                    break;
                case "433":
                    var takenNick = ircData[3];

                    // notify user
                    NickTaken(new StringEventArgs(takenNick));

                    // try alt nick if it's the first time 
                    if (takenNick == AltNick)
                    {
                        var rand = new Random();
                        var randomNick = "Guest" + rand.Next(0, 9) + rand.Next(0, 9) + rand.Next(0, 9);
                        Send("NICK " + randomNick);
                        Send("USER " + randomNick + " 0 * :" + randomNick);
                        Nick = randomNick;
                    }
                    else
                    {
                        Send("NICK " + AltNick);
                        Send("USER " + AltNick + " 0 * :" + AltNick);
                        Nick = AltNick;
                    }
                    break;
                case "JOIN": // someone joined
                {
                    var channel = ircData[2];
                    var user = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                    UserJoined(new UserJoinedEventArgs(channel, user));
                }
                break;
                case "MODE": // MODE was set
                {
                    var channel = ircData[2];
                    if (channel != Nick)
                    {
                        string from;
                        if (ircData[0].Contains("!"))
                            from = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                        else
                            from = ircData[0].Substring(1);

                        var to = ircData[4];
                        var mode = ircData[3];
                        ChannelModeSet(new ModeSetEventArgs(channel, from, to, mode));
                    }

                    // TODO: event for userMode's
                }
                break;
                case "NICK": // someone changed their nick
                    var oldNick = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                    var newNick = JoinArray(ircData, 3);

                    UserNickChange(new UserNickChangedEventArgs(oldNick, newNick));
                    break;
                case "NOTICE": // someone sent a notice
                {
                    var from = ircData[0];
                    var message = JoinArray(ircData, 3);
                    if (from.Contains("!"))
                    {
                        from = from.Substring(1, ircData[0].IndexOf('!') - 1);
                        NoticeMessage(new NoticeMessageEventArgs(from, message));
                    }
                    else
                        NoticeMessage(new NoticeMessageEventArgs(Server, message));
                }
                break;
                case "PRIVMSG": // message was sent to the channel or as private
                {
                    var from = ircData[0].Substring(1, ircData[0].IndexOf('!') - 1);
                    var to = ircData[2];
                    var message = JoinArray(ircData, 3);

                    // if it's a private message
                    if (string.Equals(to, Nick, StringComparison.CurrentCultureIgnoreCase))
                        PrivateMessage(new PrivateMessageEventArgs(from, message));
                    else
                        ChannelMessage(new ChannelMessageEventArgs(to, from, message));
                }
                break;
                case "PART":
                case "QUIT":// someone left
                {
                    var channel = ircData[2];
                    var user = ircData[0].Substring(1, data.IndexOf("!") - 1);

                    UserLeft(new UserLeftEventArgs(channel, user));
                    Send("NAMES " + ircData[2]);
                }
                break;
                default:
                    // still using this while debugging

                    if (ircData.Length > 3)
                        ServerMessage(new StringEventArgs(JoinArray(ircData, 3)));

                    break;
            }

        }
        /// <summary>
        /// Strips the message of unnecessary characters
        /// </summary>
        /// <param name="message">Message to strip</param>
        /// <returns>Stripped message</returns>
        private static string StripMessage(string message)
        {
            // remove IRC Color Codes
            foreach (Match m in new Regex((char)3 + @"(?:\d{1,2}(?:,\d{1,2})?)?").Matches(message))
                message = message.Replace(m.Value, "");

            // if there is nothing to strip
            if (message == "")
                return "";
            else if (message.Substring(0, 1) == ":" && message.Length > 2)
                return message.Substring(1, message.Length - 1);
            else
                return message;
        }
        /// <summary>
        /// Joins the array into a string after a specific index
        /// </summary>
        /// <param name="strArray">Array of strings</param>
        /// <param name="startIndex">Starting index</param>
        /// <returns>String</returns>
        private static string JoinArray(string[] strArray, int startIndex) => StripMessage(string.Join(" ", strArray, startIndex, strArray.Length - startIndex));
        /// <summary>
        /// Send message to server
        /// </summary>
        /// <param name="message">Message to send</param>
        private void Send(string message)
        {
            writer.WriteLine(message);
            try { writer.Flush(); }
            catch { Connect(); }
        }
        #endregion
    }

}