/************************************************************************
 * Copyright (C) 2008 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Client Library.
 *
 * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Client Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Represents an exception thrown during an SMPP operation
    /// </summary>
    [Serializable]
    public class SmppClientException : Exception
    {
        #region Variables
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <see cref="SmppClientException"/>
        /// </summary>
        public SmppClientException() { }

        /// <summary>
        /// Initializes a new instance of <see cref="SmppClientException"/>
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public SmppClientException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of <see cref="SmppClientException"/>
        /// </summary>
        /// <param name="message">A message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception or null if not exception is specified</param>
        public SmppClientException(string message, Exception innerException)
            : base(message, innerException) { }
        #endregion
    }
}
