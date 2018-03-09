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
    public sealed class DeliverSmResp : ResponsePDU
    {
        #region Constructors
        internal DeliverSmResp(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService) { }
        #endregion

        #region Properties
        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.ESME; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Receiver; }
        }
        #endregion

        #region Methods
        protected override byte[] GetBodyData()
        {
            //deliver_sm_resp has unused param 'message_id'
            //It must always be set to null
            return EncodeCString(null, vSmppEncodingService);
        }

        protected override void Parse(ByteBuffer buffer)
        {
            //deliver_sm_resp must contain one unused parameter 'message_id'
            //thus, at least 1 byte is required for this pdu
            if (buffer.Length < 1) { throw new NotEnoughBytesException("deliver_sm_resp requires at least 1 byte for body data"); }
            //unfortunately we don't have storage variable for this parameter
            /*vMessageID = */ DecodeCString(buffer, vSmppEncodingService);
            //Since this pdu has no optional parameters,
            //If there is still something in the buffer,
            //we then have more than enough
            if (buffer.Length > 0) { throw new TooManyBytesException(); }
        }
        #endregion
    }
}
