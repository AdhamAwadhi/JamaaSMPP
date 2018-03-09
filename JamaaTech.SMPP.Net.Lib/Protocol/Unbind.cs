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

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public class Unbind : GenericRequestPDU
    {
        #region Constructors
        internal Unbind(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService) { }

        public Unbind(SmppEncodingService smppEncodingService)
            : base(new PDUHeader(CommandType.UnBind), smppEncodingService)
        {
        }
        #endregion

        #region Properties
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
        public override ResponsePDU CreateDefaultResponce()
        {
            PDUHeader header = new PDUHeader(CommandType.UnBindResp,vHeader.SequenceNumber);
            UnbindResp resp = (UnbindResp)CreatePDU(header, vSmppEncodingService);
            return resp;
        }
        #endregion

    }
}
