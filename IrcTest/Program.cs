using System;
using System.Threading;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace IrcTest
{
    class Program : IDisposable
    {
        public static IrcClient _irc = new IrcClient();
        private static readonly string _hostName = "Gaffel.FR.AfterNET.Org";
        private static readonly string _channel = "#9764";
        private static readonly string _id = "110a9d13-732b-4a49-9d64-36542aab5fbf";
        private static string _clientLogin;
        private static string _clientName;
        private static Thread _listener;
        private static bool _clientReady = false;

        static void Main(string[] args)
        {
            _clientLogin = string.Format("sirius{0}", new Random().Next(0, 10000));
            _irc.SendDelay = 200;
            _irc.AutoRetry = true;
            _irc.ChannelSyncing = true;
            _irc.OnReadLine += IrcOnOnReadLine;
            Console.WriteLine("Enter name");
            _clientName = Console.ReadLine();
            Console.WriteLine("Connecting. Please wait");
            Connect();
            _listener = new Thread(Listen);
            _listener.Start();
            while (!_clientReady)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Connected");

            while (true)
            {
                string message = Console.ReadLine();
                _irc.Message(SendType.Message, _channel, PackMessage(_clientName, message));
            }
        }

        private static void Listen()
        {
            _irc.Listen();
            _irc.Disconnect();
        }

        private static void Connect()
        {
            int port = 6667;
            if (_irc.Connect(new[] { _hostName }, port))
            {
                _irc.Login(_clientLogin, "client");
                _irc.Join(_channel);
            }
        }

        private static void IrcOnOnReadLine(string rawline)
        {
            if (rawline.Contains("End of Channel Ban List"))
            {
                _clientReady = true;
            }

            if (!rawline.Contains(_id))
            {
                return;
            }

            rawline = rawline.Remove(0, rawline.IndexOf('{'));

            try
            {
                Message m = UnpackMessage(rawline);
                Console.WriteLine("{0}> {1}", m.UserName, m.Content);
            }
            catch (Exception)
            {
            }
        }

        private static string PackMessage(string userName, string message)
        {
            var mess = new Message
            {
                Id = _id,
                UserName = userName,
                Content = message
            };

            return JsonConvert.SerializeObject(mess, Formatting.None);
        }

        private static Message UnpackMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }

        public void Dispose()
        {
            _listener.Abort();
        }
    }

    public class Message
    {
        public string Id;
        public string UserName;
        public string Content;
    }
}
