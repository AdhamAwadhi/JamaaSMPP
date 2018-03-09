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
    public sealed class SubmitSmResp : ResponsePDU
    {
        #region Variables
        private string vMessageID;
        #endregion

        #region Constructors
        internal SubmitSmResp(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vMessageID = "";
        }
        #endregion

        #region properties
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
        #endregion

        #region Methods
        protected override byte[] GetBodyData()
        {
            return EncodeCString(vMessageID, vSmppEncodingService);
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            //Note that the body part may have not been returned by
            //the SMSC if the command status is not 0
            if (buffer.Length == 0) { return; }
            vMessageID = DecodeCString(buffer, vSmppEncodingService);
            //This pdu has no optional parameters,
            //after preceding statements, the buffer must remain with no data
            if (buffer.Length > 0) { throw new TooManyBytesException(); }
        }
        #endregion
    }
}
