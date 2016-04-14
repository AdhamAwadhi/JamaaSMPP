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
    public class PDUErrorEventArgs : EventArgs
    {
        #region Variable
        private PDUException vException;
        private byte[] vByteDump;
        private PDUHeader vHeader;
        private PDU vPdu;
        #endregion

        #region Constructors
        public PDUErrorEventArgs(PDUException exception, byte[] byteDump, PDUHeader header)
        {
            vException = exception;
            vByteDump = byteDump;
            vHeader = header;
        }

        public PDUErrorEventArgs(PDUException exception, byte[] byteDump, PDUHeader header, PDU pdu)
            :this(exception,byteDump,header)
        {
            vPdu = pdu;
        }
        #endregion

        #region Properties
        public PDUException Exception
        {
            get { return vException; }
        }

        public byte[] ByteDump
        {
            get { return vByteDump; }
        }

        public PDUHeader Header
        {
            get { return vHeader; }
        }

        public PDU Pdu
        {
            get { return vPdu; }
        }
        #endregion
    }
}
