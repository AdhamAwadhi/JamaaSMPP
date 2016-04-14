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

namespace JamaaTech.Smpp.Net.Lib.Networking
{
    [Serializable]
    public class TcpIpException : Exception
    {
        #region Constructors
        internal TcpIpException()
            : base() { }

        internal TcpIpException(string message)
            : base(message) { }

        internal TcpIpException(string message, Exception innerException)
            : base(message, innerException) { }
        #endregion

        #region Methods
        internal static void WrapAndThrow(Exception innerException)
        {
            if (innerException == null) { throw new ArgumentNullException("innerException"); }
            throw new TcpIpException(innerException.Message, innerException);
        }
        #endregion
    }
}
