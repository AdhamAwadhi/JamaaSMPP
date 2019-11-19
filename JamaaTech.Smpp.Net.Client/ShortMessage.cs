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
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Defines a base class for diffent types of messages that can be used with <see cref="SmppClient"/>
    /// </summary>
    public abstract class ShortMessage
    {
        #region Variables
        protected string vSourceAddress;
        protected string vDestinatinoAddress;
        protected int vMessageCount;
        protected int vSegmentID;
        protected int vSequenceNumber;
        protected bool vRegisterDeliveryNotification;
        protected string vReceiptedMessageId;
        protected string vUserMessageReference;
        protected bool vSubmitUserMessageReference;
        protected MessageState? vMessageState;
        protected byte[] vNetworkErrorCode;
        #endregion

        #region Constructors
        public ShortMessage()
        {
            vSourceAddress = "";
            vDestinatinoAddress = "";
            vSegmentID = -1;
            vSubmitUserMessageReference = true;
        }

        public ShortMessage(int segmentId, int messageCount, int sequenceNumber)
            : base()
        {
            vSegmentID = segmentId;
            vMessageCount = messageCount;
            vSequenceNumber = sequenceNumber;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a <see cref="ShortMessage"/> source address
        /// </summary>
        public string SourceAddress
        {
            get { return vSourceAddress; }
            set { vSourceAddress = value; }
        }
        /// <summary>
        /// Gets or sets a <see cref="ShortMessage"/> destination address
        /// </summary>
        public string DestinationAddress
        {
            get { return vDestinatinoAddress; }
            set { vDestinatinoAddress = value; }
        }
        /// <summary>
        /// Gets or sets a <see cref="ShortMessage"/> receipted message identifier.
        /// </summary>
        /// <value>
        /// The receipted message identifier.
        /// </value>
        public string ReceiptedMessageId
        {
            get { return vReceiptedMessageId; }
            set { vReceiptedMessageId = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="ShortMessage"/> user message reference.
        /// </summary>
        /// <value>
        /// The user message reference.
        /// </value>
        public string UserMessageReference
        {
            get { return vUserMessageReference; }
            set { vUserMessageReference = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Boolean"/> value that indicates if the <see cref="UserMessageReference"/> should be sent to SMSC.
        /// </summary>
        public bool SubmitUserMessageReference
        {
            get { return vSubmitUserMessageReference; }
            set { vSubmitUserMessageReference = value; }
        }

        /// <summary>
        /// Gets the index of this message segment in a group of contatenated message segements
        /// </summary>
        public int SegmentID
        {
            get { return vSegmentID; }
        }

        /// <summary>
        /// Gets the sequence number for a group of concatenated message segments
        /// </summary>
        public int SequenceNumber
        {
            get { return vSequenceNumber; }
        }

        /// <summary>
        /// Gets a value indicating the total number of message segments in a concatenated message
        /// </summary>
        public int MessageCount
        {
            get { return vMessageCount; }
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Boolean"/> value that indicates if a delivery notification should be sent for <see cref="ShortMessage"/>
        /// </summary>
        public bool RegisterDeliveryNotification
        {
            get { return vRegisterDeliveryNotification; }
            set { vRegisterDeliveryNotification = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="MessageStateType"/> value that indicates the ESME the final message state for an SMSC Delivery Receipt.
        /// </summary>
        public MessageState? MessageState
        {
            get { return vMessageState; }
            set { vMessageState = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="Byte[]"/> value that indicates Network error code.  May be present for SMSC Delivery Receipts and
		/// Intermediate Notifications.  See section 5.3.2.31 for more information.
        /// </summary>
        public byte[] NetworkErrorCode
        {
            get { return vNetworkErrorCode; }
            set { vNetworkErrorCode = value; }
        }
        #endregion

        #region Methods       
        internal IEnumerable<SendSmPDU> GetMessagePDUs(DataCoding defaultEncoding, SmppEncodingService smppEncodingService)
        {
            return GetPDUs(defaultEncoding, smppEncodingService);
        }

        protected abstract IEnumerable<SendSmPDU> GetPDUs(DataCoding defaultEncoding, SmppEncodingService smppEncodingService);

        #endregion
    }
}
