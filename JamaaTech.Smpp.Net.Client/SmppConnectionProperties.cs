/************************************************************************
 * Copyright (C) 2008 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Client Library.
 *
 * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Client Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Represents SMPP connection properties
    /// </summary>
    [Serializable()]
    public class SmppConnectionProperties
    {
        #region Variables
        private string vSystemID;
        private string vPassword;
        private string vHost;
        private int vPort;
        private InterfaceVersion vInterfaceVersion;
        private TypeOfNumber vAddressTon;
        private NumberingPlanIndicator vAddressNpi;
        private DataCoding vDefaultEncoding;
        private string vDefaultServiceType;
        private string vSystemType;
        private object vSyncRoot;
        private string vSourceAddress;
        private bool? vUseSeparateConnections;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <see cref="SmppConnectionProperties"/>
        /// </summary>
        public SmppConnectionProperties()
        {
            vSystemID = "";
            vPassword = "";
            vHost = "";
            vAddressTon = TypeOfNumber.International;
            vAddressNpi = NumberingPlanIndicator.ISDN;
            vInterfaceVersion = InterfaceVersion.v34;
            vSystemType = "";
            vDefaultServiceType = "";
            SmscID = "";
            vSyncRoot = new object();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the system id that identifies this client to the SMPP server
        /// </summary>
        public string SystemID
        {
            get { return vSystemID; }
            set { vSystemID = value; }
        }

        /// <summary>
        /// Gets or sets the password for authenticating the client to the SMPP server
        /// </summary>
        public string Password
        {
            get { return vPassword; }
            set { vPassword = value; }
        }

        /// <summary>
        /// Gets or sets host name or IP address of the remote host
        /// </summary>
        public string Host
        {
            get { return vHost; }
            set { vHost = value; }
        }

        /// <summary>
        /// Gets or sets the TCP/IP Protocol port number
        /// </summary>
        public int Port
        {
            get { return vPort; }
            set { vPort = value; }
        }

        /// <summary>
        /// Gets or sets the default SMPP interface version to be used
        /// </summary>
        public InterfaceVersion InterfaceVersion
        {
            get { return vInterfaceVersion; }
            set { vInterfaceVersion = value; }
        }

        /// <summary>
        /// Gets or sets the Numbering Plan Indicator (NPI)
        /// </summary>
        public NumberingPlanIndicator AddressNpi
        {
            get { return vAddressNpi; }
            set { vAddressNpi = value; }
        }

        /// <summary>
        /// Gets or sets the type of number
        /// </summary>
        public TypeOfNumber AddressTon
        {
            get { return vAddressTon; }
            set { vAddressTon = value; }
        }

        /// <summary>
        /// Gets or sets the default encoding to be used when sending messages
        /// </summary>
        public DataCoding DefaultEncoding
        {
            get { return vDefaultEncoding; }
            set { vDefaultEncoding = value; }
        }

        /// <summary>
        /// Gets or sets the defalt SMPP service type
        /// </summary>
        public string DefaultServiceType
        {
            get { return vDefaultServiceType; }
            set { vDefaultServiceType = value; }
        }

        /// <summary>
        /// Gets or sets SMPP service type
        /// </summary>
        public string SystemType
        {
            get { return vSystemType; }
            set { vSystemType = value; }
        }

        /// <summary>
        /// Gets the ID or the Short Message Service Center (SMSC)
        /// </summary>
        public string SmscID { internal set; get; }

        /// <summary>
        /// Gets an object that can be used for locking in a multi-threaded environment
        /// </summary>
        public object SyncRoot
        {
            get { return vSyncRoot; }
        }

        /// <summary>
        /// Gets or sets the default source address when sending messages
        /// </summary>
        public string SourceAddress
        {
            get { return vSourceAddress; }
            set { vSourceAddress = value; }
        }

        /// <summary>
        /// Gets or sets UseSeparateConnections
        /// When null: Depends on <see cref="InterfaceVersion"/>, if <see cref="InterfaceVersion.v33"/> true, <see cref="InterfaceVersion.v34"/> false.
        /// When true: Use two sessions for Receiver (<see cref="CommandType.BindReceiver"/>) and Transmitter (<see cref="CommandType.BindTransmitter"/>)
        /// When false: Use one session for Receiver and Transmitter in mode <see cref="CommandType.BindTransceiver"/>
        /// </summary>
        public bool? UseSeparateConnections
        {
            get { return vUseSeparateConnections; }
            set { vUseSeparateConnections = value; }
        }

        /// <summary>
        /// <see cref="UseSeparateConnections"/>
        /// </summary>
        public bool CanSeparateConnections => UseSeparateConnections == true || InterfaceVersion == InterfaceVersion.v33;
        #endregion

        #region Methods
        internal SessionBindInfo GetBindInfo()
        {
            SessionBindInfo bindInfo = new SessionBindInfo();
            bindInfo.SystemID = vSystemID;
            bindInfo.Password = vPassword;
            bindInfo.ServerName = vHost;
            bindInfo.Port = vPort;
            bindInfo.InterfaceVersion = vInterfaceVersion;
            bindInfo.AddressTon = vAddressTon;
            bindInfo.AddressNpi = vAddressNpi;
            bindInfo.SystemType = vSystemType;
            return bindInfo;
        }
        #endregion
    }
}
