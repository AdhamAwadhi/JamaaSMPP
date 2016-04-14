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
using System.Collections.Generic;
using System.Text;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Provides data for <see cref="SmppClient.StateChanged"/> event
    /// </summary>
    public class StateChangedEventArgs : EventArgs
    {
        #region Constructors
        public StateChangedEventArgs(bool started)
        { this.Started = started; }
        #endregion

        /// <summary>
        /// Returns true if <see cref="SmppClient"/> is started or <value>false</value> otherwise
        /// </summary>
        #region Properties
        public bool Started { private set; get; }
        #endregion
    }
}
