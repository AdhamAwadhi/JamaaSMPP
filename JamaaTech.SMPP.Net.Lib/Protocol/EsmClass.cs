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
    public enum EsmClass : byte
    {
        /// <summary>
        /// Default SMSC mode, default message type, or no specific features selected
        /// </summary>
        Default = 0x00,
        /// <summary>
        /// Datagram mode
        /// </summary>
        DatagramMode = 0x01,
        /// <summary>
        /// Forward (i.e. Transaction) mode
        /// </summary>
        Transaction = 0x02, //Forward mode
        /// <summary>
        /// Store and forward mode
        /// </summary>
        StoreAndForward = 0x03,
        /// <summary>
        /// Short message contains SMSC delivery receipt
        /// </summary>
        DeliveryReceipt = 0x04,
        /// <summary>
        /// SME contains ESME delivery acknoledgement
        /// </summary>
        DeliveryAcknoledgement = 0x08,
        /// <summary>
        /// SME contains ESME manual/user acknoledgement
        /// </summary>
        ManualUserAcknoledgement = 0x10,
        /// <summary>
        /// Short message contains conversion abort (Korean CDMA)
        /// </summary>
        ConversionAbort = 0x18,
        /// <summary>
        /// Short message contains intermedicate delivery notification
        /// </summary>
        IntermediateDeliveryNotification = 0x20,
        /// <summary>
        /// UDHI Indicator (only relevant for MT network)
        /// </summary>
        UdhiIndicator = 0x40,
        /// <summary>
        /// Set Reply path (only relevant for GSM network)
        /// </summary>
        ReplyPath = 0x80
    }
}
