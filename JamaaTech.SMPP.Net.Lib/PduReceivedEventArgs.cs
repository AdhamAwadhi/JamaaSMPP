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
    public class PduReceivedEventArgs : EventArgs
    {
        #region Variables
        private RequestPDU vRequest;
        private ResponsePDU vResponse;
        #endregion

        #region Constructors
        public PduReceivedEventArgs(RequestPDU request)
        {
            vRequest = request;
        }
        #endregion

        #region Properties
        public RequestPDU Request
        {
            get { return vRequest; }
        }

        public ResponsePDU Response
        {
            get { return vResponse; }
            set { vResponse = value; }
        }
        #endregion
    }
}
