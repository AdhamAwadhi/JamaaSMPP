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
    [Flags()]
    public enum RegisteredDelivery : byte
    {
        /// <summary>
        /// No SMSC delivery receipt requested,
        /// No recipient SME acknoledgement requested,
        /// No intermediate notification requested
        /// </summary>
        None = 0x00,
        /// <summary>
        /// SMSC delivery receipt requested where final delivery outcome is delivery success or failure
        /// </summary>
        DeliveryReceipt = 0x01,
        /// <summary>
        /// SMSC delivery receipt requested where the final delivery outcome is delivery failure
        /// </summary>
        DeliveryReceiptFailure = 0x02,
        /// <summary>
        /// SME delivery acknoledgement requested
        /// </summary>
        DeliveryAcknoledgement = 0x04,
        /// <summary>
        /// SME Manual/User acknoledgement requested
        /// </summary>
        ManualAcknoledgement = 0x08,
        /// <summary>
        /// Intermediate notification requested
        /// </summary>
        IntermediateNotification = 0x10
    }
}
