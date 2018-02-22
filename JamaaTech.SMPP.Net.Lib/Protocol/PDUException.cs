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
    [Serializable]
    public class PDUException : Exception
    {
        #region Variables
        private SmppErrorCode vErrorCode;
        #endregion

        #region Constructors
        public PDUException(SmppErrorCode errorCode) { vErrorCode = errorCode; }

        public PDUException(SmppErrorCode errorCode, string message)
            : base(message) { vErrorCode = errorCode; }

        public PDUException(SmppErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException) { vErrorCode = errorCode; }
        #endregion

        #region Properties
        public SmppErrorCode ErrorCode
        {
            get { return vErrorCode; }
        }
        #endregion
    }
}
