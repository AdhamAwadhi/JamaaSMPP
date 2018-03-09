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
using JamaaTech.Smpp.Net.Lib.Util;
using JamaaTech.Smpp.Net.Lib;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public class Outbind : RequestPDU
    {
        #region Variables
        private string vSystemID;
        private string vPassword;
        #endregion

        #region Constructors
        internal Outbind(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vSystemID = "";
            vPassword = "";
        }
        #endregion

        #region Properties

        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.SMSC; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Open; }
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
        #endregion

        #region Methods
        public override ResponsePDU CreateDefaultResponce()
        {
            PDUHeader header = new PDUHeader(CommandType.BindTransceiver, vHeader.SequenceNumber);
            return new BindTransceiverResp(header, vSmppEncodingService);
        }

        protected override byte[] GetBodyData()
        {
            ByteBuffer buffer = new ByteBuffer(vSystemID.Length + vPassword.Length + 2);
            buffer.Append(EncodeCString(vSystemID, vSmppEncodingService));
            buffer.Equals(EncodeCString(vPassword, vSmppEncodingService));
            return buffer.ToBytes();
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            //Outbind PDU requires at least 2 bytes
            if (buffer.Length < 2) { throw new NotEnoughBytesException("Outbind PDU requires at least 2 bytes for body data"); }
            vSystemID = DecodeCString(buffer, vSmppEncodingService);
            vPassword = DecodeCString(buffer, vSmppEncodingService);
            //This PDU has no optional parameters
            //If we still have something in the buffer, we are having more bytes than we expected
            if (buffer.Length > 0) { throw new TooManyBytesException(); }
        }
        #endregion
    }
}
