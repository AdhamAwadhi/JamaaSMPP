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
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;
using JamaaTech.Smpp.Net.Lib;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public class BindResponse : ResponsePDU
    {
        #region Variables
        private string vSystemID;
        #endregion

        #region Constructors
        internal BindResponse(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vSystemID = "";
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

        #endregion

        #region Methods
        protected override byte[] GetBodyData()
        {
            return EncodeCString(vSystemID, vSmppEncodingService);
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            //If Error code is not zero, buffer.Length may be zero
            //This may happen because the SMSC may not return the system_id field
            //if the origianl bind request contained an error.
            if (Header.ErrorCode != SmppErrorCode.ESME_ROK && buffer.Length == 0) { vSystemID = ""; return; }
            //Otherwise, there must be something in the buffer
            vSystemID = DecodeCString(buffer, vSmppEncodingService);
            if (buffer.Length > 0) { vTlv = TlvCollection.Parse(buffer, vSmppEncodingService); }
        }
        #endregion
    }
}
