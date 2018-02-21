﻿using JamaaTech.Smpp.Net.Client;
using JamaaTech.Smpp.Net.Lib.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System.Diagnostics;

namespace DemoClient
{
    class Program
    {
        static ISmppConfiguration smppConfig;

        static void Main(string[] args)
        {
            Trace.WriteLine("Start");
            //Trace.Listeners.Add(new ConsoleTraceListener());

            smppConfig = GetSmppConfiguration();

            //SMPPEncodingUtil.UCS2Encoding = Encoding.UTF8;

            var client = CreateSmppClient(smppConfig);
            client.Start();
            // must wait until connected before start sending
            while (client.ConnectionState != SmppConnectionState.Connected)
                Thread.Sleep(100);

            TextMessage msg = new TextMessage();

            msg.DestinationAddress = "255455388333"; //Receipient number
            msg.SourceAddress = "255344338333"; //Originating number
            //msg.Text = "Hello, this is my test message!";
            msg.Text = @"السلام عليكم ورحمة الله وبركاته
هذه رسالة عربية
متعددة الاسطر";
            msg.RegisterDeliveryNotification = true; //I want delivery notification for this message
            msg.UserMessageReference = Guid.NewGuid().ToString();
            Console.WriteLine($"msg.UserMessageReference: {msg.UserMessageReference}");

            //client.SendMessage(msg);

            client.BeginSendMessage(msg, SendMessageCompleteCallback, client);

            Console.ReadLine();
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


            }
        }

        private static ISmppConfiguration GetSmppConfiguration()
        {
            return new SmppConfiguration
            {
                TimeOut = 60000,
                StartAutomatically = true,
                Name = "MyLocalClient",
                SystemID = "smppclient1",
                Password = "password",
                Host = "localhost",
                Port = 5016,
                SystemType = "5750",
                DefaultServiceType = "5750",
                SourceAddress = "5750",
                AutoReconnectDelay = 5000,
                KeepAliveInterval = 5000,
                ReconnectInteval = 10000,
                Encoding = JamaaTech.Smpp.Net.Lib.DataCoding.UCS2
            };
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
