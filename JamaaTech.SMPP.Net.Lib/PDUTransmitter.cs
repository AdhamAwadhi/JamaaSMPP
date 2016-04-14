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
using System.Text;
using System.Collections.Generic;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Networking;

namespace JamaaTech.Smpp.Net.Lib
{
    public class PDUTransmitter
    {
        #region Variables
        private TcpIpSession vTcpIpSession;
        #endregion

        #region Constructors
        public PDUTransmitter(TcpIpSession session)
        {
            if (session == null) { throw new ArgumentNullException("session"); }
            vTcpIpSession = session;
        }
        #endregion

        #region Methods
        public void Send(PDU pdu)
        {
            if (pdu == null) { throw new ArgumentNullException("pdu"); }
            byte[] bytesToSend = pdu.GetBytes();
            vTcpIpSession.Send(bytesToSend);
        }
        #endregion
    }
}
