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
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Util;
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public class AlertNotification : SmPDU
    {
        #region Variables
        private SmppAddress vEsmeAddress;
        #endregion

        #region Constructors
        public AlertNotification(SmppEncodingService smppEncodingService)
            : base(new PDUHeader(CommandType.AlertNotification), smppEncodingService)
        {
            vEsmeAddress = new SmppAddress();
        }

        internal AlertNotification(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vEsmeAddress = new SmppAddress();
        }
        #endregion

        #region Properties
        public override bool HasResponse
        {
            get { return false; }
        }

        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.SMSC; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Receiver; }
        }

        public SmppAddress ESMEAddress
        {
            get { return vEsmeAddress; }
        }
        #endregion

        #region Methods
        public override ResponsePDU CreateDefaultResponce()
        {
            return null;
        }

        protected override byte[] GetBodyData()
        {
            byte[] sourceAddrBytes = vSourceAddress.GetBytes(vSmppEncodingService);
            byte[] esmeAddresBytes = vEsmeAddress.GetBytes(vSmppEncodingService);
            ByteBuffer buffer = new ByteBuffer(sourceAddrBytes.Length + esmeAddresBytes.Length);
            buffer.Append(sourceAddrBytes);
            buffer.Append(esmeAddresBytes);
            return buffer.ToBytes();
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            vSourceAddress = SmppAddress.Parse(buffer, vSmppEncodingService);
            vEsmeAddress = SmppAddress.Parse(buffer, vSmppEncodingService);
            //If there are some bytes left,
            //construct a tlv collection
            if (buffer.Length > 0) { vTlv = TlvCollection.Parse(buffer, vSmppEncodingService); }
        }
        #endregion
    }
}
