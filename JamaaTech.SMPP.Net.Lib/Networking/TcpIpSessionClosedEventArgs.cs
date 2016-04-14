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
    public class TcpIpSessionClosedEventArgs : EventArgs
    {
        #region Variables
        private SessionCloseReason vReason;
        private Exception vException;
        #endregion

        #region Constructors
        public TcpIpSessionClosedEventArgs(SessionCloseReason reason, Exception exception)
        {
            vReason = reason;
            vException = exception;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The reason that caused TcpIpSession to be closed
        /// </summary>
        public SessionCloseReason CloseReason
        {
            get { return vReason; }
        }
        /// <summary>
        /// The exception, if any that caused a TcpIpSession to be terminated
        /// </summary>
        public Exception Exception
        {
            get { return vException; }
        }
        #endregion
    }
}
