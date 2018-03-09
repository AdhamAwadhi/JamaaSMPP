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

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public sealed class QuerySmResp : ResponsePDU
    {
        #region Variables
        private string vMessageID;
        private string vFinalDate;
        private MessageState vMessageState;
        private byte vErrorCode;
        #endregion

        #region Constructors
        internal QuerySmResp(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vMessageID = "";
            vFinalDate = "";
            vMessageState = MessageState.Unknown;
            vErrorCode = 0;
        }
        #endregion

        #region Properties
        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.SMSC; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Transmitter; }
        }

        public string MessageID
        {
            get { return vMessageID; }
            set { vMessageID = value; }
        }

        public string FinalDate
        {
            get { return vFinalDate; }
            set { vFinalDate = value; }
        }

        public MessageState MessageState
        {
            get { return vMessageState; }
            set { vMessageState = value; }
        }

        public byte ErrorCode
        {
            get { return vErrorCode; }
            set { vErrorCode = value; }
        }
        #endregion

        #region Methods
        protected override byte[] GetBodyData()
        {
            ByteBuffer buffer = new ByteBuffer(16);
            buffer.Append(EncodeCString(vMessageID, vSmppEncodingService));
            buffer.Append(EncodeCString(vFinalDate, vSmppEncodingService));
            buffer.Append((byte)vMessageState);
            buffer.Append(vErrorCode);
            return buffer.ToBytes();
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            vMessageID = DecodeCString(buffer, vSmppEncodingService);
            vFinalDate = DecodeCString(buffer, vSmppEncodingService);
            vMessageState = (MessageState)GetByte(buffer);
            vErrorCode = GetByte(buffer);
            //This pdu has no option parameters,
            //If the buffer still contains something,
            //the we received more that required bytes
            if (buffer.Length > 0) { throw new TooManyBytesException(); }
        }
        #endregion
    }
}
