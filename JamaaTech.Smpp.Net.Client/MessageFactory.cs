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

using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// A factory class for constructing messages from PDUs
    /// </summary>
    public static class MessageFactory
    {
        #region Methods
        /// <summary>
        /// Creates a <see cref="TextMessage"/> from a received <see cref="SingleDestinationPDU"/>
        /// </summary>
        /// <param name="pdu">The PDU from which a <see cref="TextMessage"/> is constructed</param>
        /// <returns>A <see cref="TextMessage"/> represening a text message extracted from the received PDU</returns>
        public static TextMessage CreateMessage(SingleDestinationPDU pdu)
        {
            //This version supports only text messages
            Udh udh = null;
            string message = null;
            pdu.GetMessageText(out message, out udh);
            TextMessage sms = null;
            //Check if the udh field is present
            if (udh != null) { sms = new TextMessage(udh.SegmentID, udh.MessageCount, udh.MessageSequence); }
            else { sms = new TextMessage(); }
            sms.Text = message == null ? "" : message;
            sms.SourceAddress = pdu.SourceAddress.Address;
            sms.DestinationAddress = pdu.DestinationAddress.Address;
            return sms;
        }
        #endregion
    }
}
