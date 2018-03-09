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
    public sealed class DataSmResp : ResponsePDU
    {
        #region Variables
        private string vMessageID;
        #endregion

        #region Constructors
        internal DataSmResp(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vMessageID = "";
        }
        #endregion

        #region Properties
        public string MessageID
        {
            get { return vMessageID; }
            set { vMessageID = value; }
        }

        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.Any; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Transceiver; }
        }
        #endregion

        #region Methods
        protected override byte[] GetBodyData()
        {
            return EncodeCString(vMessageID, vSmppEncodingService);
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            //We require at least 1 byte for this pdu
            if (buffer.Length < 1) { throw new NotEnoughBytesException("data_sm_resp requires at least 1 byte of body data"); }
            vMessageID = DecodeCString(buffer, vSmppEncodingService);
            if (buffer.Length > 0) { vTlv = TlvCollection.Parse(buffer, vSmppEncodingService); }
        }
        #endregion
    }
}
