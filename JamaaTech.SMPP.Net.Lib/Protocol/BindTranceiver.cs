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
    public sealed class BindTransceiver : BindRequest
    {
        #region Constructors
        public BindTransceiver(SmppEncodingService smppEncodingService)
            : base(new PDUHeader(CommandType.BindTransceiver), smppEncodingService) { }

        internal BindTransceiver(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService) { }
        #endregion
    }
}
