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
    /// Represent <see cref="SmppClient"/> connection state
    /// </summary>
    public enum SmppConnectionState : int
    {
        /// <summary>
        /// Indicates the <see cref="SmppClient"/> is connected to SMSC
        /// </summary>
        Connected = 1,
        /// <summary>
        /// Indicates the <see cref="SmppClient"/> is currently trying to establish a connection
        /// </summary>
        Connecting = 2,
        /// <summary>
        /// Indicates the <see cref="SmppClient"/> is not connected
        /// </summary>
        Closed = 3
    }
}
