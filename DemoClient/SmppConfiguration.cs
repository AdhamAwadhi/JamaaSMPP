using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoClient
{
    public class SmppConfiguration : ISmppConfiguration
    {
        public int Id { get; set; }
        public int AutoReconnectDelay { get; set; }
        public string DefaultServiceType { get; set; }
        public DataCoding Encoding { get; set; }
        public string Host { get; set; }
        public bool IgnoreLength { get; set; }
        public int KeepAliveInterval { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public int ReconnectInteval { get; set; }
        public string SourceAddress { get; set; }
        public string SystemID { get; set; }
        public string SystemType { get; set; }
        public int TimeOut { get; set; }
        public bool StartAutomatically { get; set; }
        public string DestinationAddressRegex { get; set; }
        public bool RegisterDeliveryNotification { get; set; }
        public NumberingPlanIndicator? AddressNpi { get; set; }
        public TypeOfNumber? AddressTon { get; set; }
        public Dictionary<string, string> OptionalParams { get; set; }
        public UserMessageReferenceType UserMessageReferenceType { get; set; }
        public bool? UseSeparateConnections { get; set; }
        public bool SubmitUserMessageReference { get; set; }

        public SmppConfiguration()
        {
            TimeOut = 60000;
            StartAutomatically = true;
            Name = "MyLocalClient";
            SystemID = "smppclient1";
            Password = "password";
            Host = "localhost";
            Port = 5016;
            SystemType = "5750";
            DefaultServiceType = "5750";
            SourceAddress = "5750";
            AutoReconnectDelay = 5000;
            KeepAliveInterval = 5000;
            ReconnectInteval = 10000;
            Encoding = JamaaTech.Smpp.Net.Lib.DataCoding.UCS2;
            RegisterDeliveryNotification = true;
            UseSeparateConnections = null;
            SubmitUserMessageReference = true;

        }
    }

    public enum UserMessageReferenceType
    {
        None,
        Guid,
        Int,
        IntHex
    }
}
