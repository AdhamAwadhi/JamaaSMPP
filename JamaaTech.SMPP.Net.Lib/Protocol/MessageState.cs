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
    public enum MessageState : byte
    {
        /// <summary>
        /// The message is in enroute state
        /// </summary>
        EnRoute = 1,
        /// <summary>
        /// The message is delivered to destination
        /// </summary>
        Delivered = 2,
        /// <summary>
        /// Message validity period has expired
        /// </summary>
        Expired = 3,
        /// <summary>
        /// Message has been deleted
        /// </summary>
        Deleted = 4,
        /// <summary>
        /// Message is undeliverable
        /// </summary>
        Undeliverable = 5,
        /// <summary>
        /// Message is in accepted state (i.e. has been manually read on behalf of the subscriber by the customer service
        /// </summary>
        Accepted = 6,
        /// <summary>
        /// Message is invalid state
        /// </summary>
        Unknown = 7,
        /// <summary>
        /// Message is in rejected state
        /// </summary>
        Rejected = 8
    }
}
