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

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Provides data for <see cref="SmppClient.MessageReceived"/>, <see cref="SmppClient.MessageDelivered"/> and <see cref="SmppClient.MessageSent"/> events
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        #region Variables
        private ShortMessage vShortMessage;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="MessageEventArgs"/>
        /// </summary>
        /// <param name="message">The message associated with the message event</param>
        public MessageEventArgs(ShortMessage message)
        {
            vShortMessage = message;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the message associated with this event
        /// </summary>
        public ShortMessage ShortMessage
        {
            get { return vShortMessage; }
        }
        #endregion
    }
}
