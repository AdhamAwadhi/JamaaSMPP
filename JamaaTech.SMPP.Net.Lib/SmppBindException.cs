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

namespace JamaaTech.Smpp.Net.Lib
{
    public class SmppBindException : SmppException
    {
        #region Constructors
        internal SmppBindException()
            : base(SmppErrorCode.ESME_RBINDFAIL, "Bind operation failed") { }

        internal SmppBindException(SmppErrorCode errorCode)
            : base(errorCode, "Bind operation failed") { }

        internal SmppBindException(Exception innerException)
            : base(SmppErrorCode.ESME_RBINDFAIL, "Bind operation failed", innerException) { }
        #endregion

        #region Methods
        public static new void WrapAndThrow(Exception innerException)
        {
            if (innerException == null) { throw new ArgumentNullException("innerException"); }
            throw new SmppBindException(innerException);
        }
        #endregion
    }
}
