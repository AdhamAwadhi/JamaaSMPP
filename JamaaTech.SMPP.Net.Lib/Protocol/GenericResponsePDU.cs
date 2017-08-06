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

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public abstract class GenericResponsePDU : ResponsePDU
    {
        #region Constructors
        internal GenericResponsePDU(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService) { }
        #endregion

        #region Methods
        protected override byte[] GetBodyData()
        {
            return null; 
        }

        protected override void Parse(JamaaTech.Smpp.Net.Lib.Util.ByteBuffer buffer)
        {
            if (buffer.Length > 0) { throw new TooManyBytesException(); }
        }
        #endregion
    }
}
