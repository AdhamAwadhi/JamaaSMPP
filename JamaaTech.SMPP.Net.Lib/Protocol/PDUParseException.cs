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
    public class PDUParseException : PDUException
    {
        #region Constructors
        public PDUParseException(SmppErrorCode errorCode)
            : base(errorCode) { }

        public PDUParseException(SmppErrorCode errorCode, string message)
            : base(errorCode, message) { }

        public PDUParseException(SmppErrorCode errorCode, string message, Exception innerException)
            : base(errorCode, message, innerException) { }
        #endregion

        #region Methods
        public static void WrapAndThrow(Exception innerException)
        {
            if (innerException == null) { throw new ArgumentNullException("innerException"); }
            throw new PDUParseException(SmppErrorCode.ESME_RUNKNOWNERR,
                innerException.Message, innerException);
        }
        #endregion
    }
}
