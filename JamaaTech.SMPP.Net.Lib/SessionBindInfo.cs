/************************************************************************
 * Copyright (C) 2007 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Library.
 *
 * Jamaa SMPP Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Lib
{
    [Serializable()]
    public class SessionBindInfo
    {
        #region Variables
        private string vHost;
        private int vPort;
        private string vSystemID;
        private string vPassword;
        private string vSystemType;
        private InterfaceVersion vInterfaceVersion;
        private bool vAllowReceive;
        private bool vAllowTransmit;
        private TypeOfNumber vAddressTon;
        private NumberingPlanIndicator vAddressNpi;
        #endregion

        #region Constructors
        public SessionBindInfo()
        {
            vHost = "";
            vSystemID = "";
            vPassword = "";
            vSystemType = "";
            vInterfaceVersion = InterfaceVersion.v34;
            vAllowReceive = true;
            vAllowTransmit = true;
            vAddressTon = TypeOfNumber.International;
            vAddressNpi = NumberingPlanIndicator.ISDN;
        }

        public SessionBindInfo(string systemId, string password)
            :this()
        {
            vSystemID = systemId;
            vPassword = password;
        }
        #endregion

        #region Properties
        public string ServerName
        {
            get { return vHost; }
            set { vHost = value; }
        }

        public int Port
        {
            get { return vPort; }
            set { vPort = value; }
        }

        public InterfaceVersion InterfaceVersion
        {
            get { return vInterfaceVersion; }
            set { vInterfaceVersion = value; }
        }

        public string SystemID
        {
            get { return vSystemID; }
            set { vSystemID = value; }
        }

        public string Password
        {
            get { return vPassword; }
            set { vPassword = value; }
        }

        public string SystemType
        {
            get { return vSystemType; }
            set { vSystemType = value; }
        }

        public NumberingPlanIndicator AddressNpi
        {
            get { return vAddressNpi; }
            set { vAddressNpi = value; }
        }

        public TypeOfNumber AddressTon
        {
            get { return vAddressTon; }
            set { vAddressTon = value; }
        }

        public bool AllowReceive
        {
            get { return vAllowReceive; }
            set { vAllowReceive = value; }
        }

        public bool AllowTransmit
        {
            get { return vAllowTransmit; }
            set { vAllowTransmit = value; }
        }
        #endregion

        #region Methods
        internal BindRequest CreatePdu(SmppEncodingService smppEncodingService)
        {
            BindRequest req = CreateBindBdu(smppEncodingService);
            req.AddressNpi = vAddressNpi;
            req.AddressTon = vAddressTon;
            req.SystemID = vSystemID;
            req.Password = vPassword;
            req.SystemType = vSystemType;
            req.InterfaceVersion = vInterfaceVersion == InterfaceVersion.v33 ? (byte)0x33 : (byte)0x34;
            return req;
        }

        private BindRequest CreateBindBdu(SmppEncodingService smppEncodingService)
        {
            if (vAllowReceive && vAllowTransmit) { return new BindTransceiver(smppEncodingService); }
            else if (vAllowTransmit) { return new BindTransmitter(smppEncodingService); }
            else if (vAllowReceive) { return new BindReceiver(smppEncodingService); }
            else { throw new InvalidOperationException("Both AllowTransmit and AllowReceive cannot be set to false"); }
        }
        #endregion
    }
}
