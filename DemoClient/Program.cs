using JamaaTech.Smpp.Net.Client;
using JamaaTech.Smpp.Net.Lib.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System.Diagnostics;
using JamaaTech.Smpp.Net.Lib.Logging;
#if NET40
using SettingsReader.Readers;
#endif
using System.Linq;

namespace DemoClient
{
    class Program
    {
        private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static ISmppConfiguration smppConfig;
        private static SmppClient client;
        private static readonly Random RND = new Random();

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        static void Main(string[] args)
        {
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.DebugLoggerFactoryAdapter();
            var encSrv = new SmppEncodingService();

            var hexBytes = "000000dd0000000500000000019182410001013334363439323836383039000501657669636572746961000400000000000000008569643a323533303932393134353232363637333732207375623a30303120646c7672643a303031207375626d697420646174653a3133303932393136353220646f6e6520646174653a3133303932393136353220737461743a44454c49565244206572723a3030303020746578743a1b3c657669534d531b3e0a534d532064652050727565042300030300000427000102001e001332353330393239313435323236363733373200";
            var packet = StringToByteArray(hexBytes);
            var bodyBytes = packet.Skip(16).ToArray();

            var pdu = PDU.CreatePDU(PDUHeader.Parse(new ByteBuffer(packet), encSrv), encSrv);
            pdu.SetBodyData(new ByteBuffer(bodyBytes));

            var receiptedMessageId = pdu.GetOptionalParamString(JamaaTech.Smpp.Net.Lib.Protocol.Tlv.Tag.receipted_message_id);

            //Assert.AreEqual("253092914522667372", pdu.ReceiptedMessageId);

            _Log.Info("Start");
            //Trace.Listeners.Add(new ConsoleTraceListener());

            smppConfig = GetSmppConfiguration();

            //SMPPEncodingUtil.UCS2Encoding = Encoding.UTF8;

            client = CreateSmppClient(smppConfig);
            client.Start();

            // must wait until connected before start sending
            while (client.ConnectionState != SmppConnectionState.Connected)
                Thread.Sleep(100);

            // Accept command input
            bool bQuit = false;

            do
            {
                // Hit Enter in the terminal once the binds are up to see this prompt

                Console.WriteLine("Commands");
                Console.WriteLine("send 123456 hello");
                Console.WriteLine("quit");
                Console.WriteLine("");

                Console.Write("\n#>");

                string command = Console.ReadLine();
                if (command.Length == 0)
                    continue;

                switch (command.Split(' ')[0].ToString())
                {
                    case "quit":
                    case "exit":
                    case "q":
                        bQuit = true;
                        break;

                    default:
                        ProcessCommand(command);
                        break;
                }

                if (bQuit)
                    break;

            } while (true);

            if (client != null)
                client.Dispose();
        }

        private static void ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');

            switch (parts[0])
            {
                case "send":
                    SendMessage(command);
                    break;
            }
        }

        private static void SendMessage(string command)
        {
            var parts = command.Split(' ');
            var dest = parts[1];
            var msgTxt = string.Join(" ", parts, 2, parts.Length - 2);

            if (string.IsNullOrEmpty(msgTxt))
                msgTxt = @"السلام عليكم ورحمة الله وبركاته
هذه رسالة عربية
متعددة الاسطر";

            TextMessage msg = new TextMessage();

            msg.DestinationAddress = dest; //Receipient number
            msg.SourceAddress = smppConfig.SourceAddress; //Originating number
                                                          //msg.Text = "Hello, this is my test message!";
            msg.Text = msgTxt;
            msg.RegisterDeliveryNotification = true; //I want delivery notification for this message
            msg.UserMessageReference = GenerateUserMessageReference(smppConfig.UserMessageReferenceType);
            _Log.DebugFormat($"msg.UserMessageReference: {msg.UserMessageReference}");

            try
            {
                client.SendMessage(msg);
            }
            catch (SmppException smppEx)
            {
                _Log.ErrorFormat("smppEx.ErrorCode:({0}) {1} ", (int)smppEx.ErrorCode, smppEx.ErrorCode);
                _Log.Error(smppEx);
            }
            catch (Exception e)
            {
                _Log.Error("SendMessage:" + e.Message, e);
            }
            //client.BeginSendMessage(msg, SendMessageCompleteCallback, client);
        }

        private static string GenerateUserMessageReference(UserMessageReferenceType userMessageReferenceType)
        {
            switch (userMessageReferenceType)
            {
                case UserMessageReferenceType.Guid:
                    return Guid.NewGuid().ToString();
                case UserMessageReferenceType.Int:
                    return RND.Next(0, int.MaxValue).ToString();
                case UserMessageReferenceType.IntHex:
                    return RND.Next(0, int.MaxValue).ToString("X");
                default:
                    return null;
            }
        }


        private static void SendMessageCompleteCallback(IAsyncResult result)
        {
            try
            {
                SmppClient client = (SmppClient)result.AsyncState;
                client.EndSendMessage(result);
            }
            catch (Exception e)
            {
                _Log.Error("SendMessageCompleteCallback:" + e.Message, e);
            }
        }

        private static ISmppConfiguration GetSmppConfiguration()
        {
#if NET40
            var reader = new AppSettingsReader();
            return reader.Read<SmppConfiguration>();
#else
            return new SmppConfiguration()
            {
                SystemID = "smppclient1",
                Password = "password",
                Host = "localhost",
                Port = 5016,
                SystemType = "5750",
                DefaultServiceType = "5750",
                SourceAddress = "5750",
                Encoding = DataCoding.UCS2,
                AddressNpi = NumberingPlanIndicator.Unknown,
                AddressTon = TypeOfNumber.Alphanumeric,
                UserMessageReferenceType = UserMessageReferenceType.None,
                RegisterDeliveryNotification = true,
                UseSeparateConnections = true
            };
#endif
        }

        static SmppClient CreateSmppClient(ISmppConfiguration config)
        {
            var client = new SmppClient();
            client.Name = config.Name;
            //client.SmppEncodingService = new SmppEncodingService(System.Text.Encoding.UTF8);

            client.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(client_ConnectionStateChanged);
            client.StateChanged += new EventHandler<StateChangedEventArgs>(client_StateChanged);
            client.MessageSent += new EventHandler<MessageEventArgs>(client_MessageSent);
            client.MessageDelivered += new EventHandler<MessageEventArgs>(client_MessageDelivered);
            client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

            SmppConnectionProperties properties = client.Properties;
            properties.SystemID = config.SystemID;// "mysystemid";
            properties.Password = config.Password;// "mypassword";
            properties.Port = config.Port;// 2034; //IP port to use
            properties.Host = config.Host;// "196.23.3.12"; //SMSC host name or IP Address
            properties.SystemType = config.SystemType;// "mysystemtype";
            properties.DefaultServiceType = config.DefaultServiceType;// "mydefaultservicetype";
            properties.DefaultEncoding = config.Encoding;
            properties.UseSeparateConnections = config.UseSeparateConnections;

            //Resume a lost connection after 30 seconds
            client.AutoReconnectDelay = config.AutoReconnectDelay;

            //Send Enquire Link PDU every 15 seconds
            client.KeepAliveInterval = config.KeepAliveInterval;

            return client;
        }

        private static void client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            var client = (SmppClient)sender;
            Console.WriteLine("SMPP client {1} - State {1}", client.Name, e.CurrentState);

            if (client.LastException != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(client.LastException.ToString());
                Console.ResetColor();
            }

            switch (e.CurrentState)
            {
                case SmppConnectionState.Closed:
                    //Connection to the remote server is lost
                    //Do something here
                    {
                        Console.WriteLine("SMPP client {0} - CLOSED", client.Name);
                        e.ReconnectInteval = smppConfig.ReconnectInteval; //Try to reconnect after Interval in seconds
                        break;
                    }
                case SmppConnectionState.Connected:
                    //A successful connection has been established
                    Console.WriteLine("SMPP client {0} - CONNECTED", client.Name);
                    break;
                case SmppConnectionState.Connecting:
                    //A connection attemp is still on progress
                    Console.WriteLine("SMPP client {0} - CONNECTING", client.Name);
                    break;
            }

        }

        private static void client_StateChanged(object sender, StateChangedEventArgs e)
        {
            var client = (SmppClient)sender;
            Console.WriteLine("SMPP client {0}: {1}", client.Name, e.Started ? "STARTED" : "STOPPED");
        }

        private static void client_MessageSent(object sender, MessageEventArgs e)
        {
            var client = (SmppClient)sender;
            Console.WriteLine("SMPP client {0} - Message Sent to: {1} {2}", client.Name, e.ShortMessage.DestinationAddress, e.ShortMessage.UserMessageReference);
            // CANDO: save sent sms
        }

        private static void client_MessageDelivered(object sender, MessageEventArgs e)
        {
            var client = (SmppClient)sender;
            //Console.WriteLine("SMPP client {0} Message Delivered to: {1}", client.Name, e.ShortMessage.DestinationAddress);
            Console.WriteLine("SMPP client {0} - Message Delivered: MessageId: {1}", client.Name, e.ShortMessage.UserMessageReference);


            // CANDO: save delivered sms
        }

        private static void client_MessageReceived(object sender, MessageEventArgs e)
        {
            var client = (SmppClient)sender;
            TextMessage msg = e.ShortMessage as TextMessage;
            Console.WriteLine("SMPP client {0} - Message Received from: {1}, msg: {2}", client.Name, msg.SourceAddress, msg.Text);

            // CANDO: save received sms
        }
    }
}
