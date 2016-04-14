﻿using JamaaTech.Smpp.Net.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoClient
{
    public interface ISmppConfiguration
    {
        int Id { get; }
        int AutoReconnectDelay { get; }
        string DefaultServiceType { get; }
        DataCoding Encoding { get; }
        string Host { get; }
        bool IgnoreLength { get; }
        int KeepAliveInterval { get; }
        string Name { get; }
        string Password { get; }
        int Port { get; }
        int ReconnectInteval { get; }
        string SourceAddress { get; }
        string SystemID { get; }
        string SystemType { get; }
        int TimeOut { get; }
        bool StartAutomatically { get; }
        string DestinationAddressRegex { get; }
    }
}
