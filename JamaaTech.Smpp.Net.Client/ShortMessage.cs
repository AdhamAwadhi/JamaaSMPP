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
        // Set the source and destination address has a smpp address to be able to set the 
        // NPI and TON settings for each adress.
        protected SmppAddress vSourceAddress;
        protected SmppAddress vDestinationAddress;
        protected int vMessageCount;
        protected int vSegmentID;
        protected int vSequenceNumber;
        protected bool vRegisterDeliveryNotification;
        protected string vReceiptedMessageId;
        protected string vUserMessageReference;
        #endregion

        #region Constructors
        public ShortMessage()
        {
            //Update the source and destination address
            vSourceAddress = new SmppAddress();
            vDestinationAddress = new SmppAddress();
            vSegmentID = -1;
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
        /// Gets the <see cref="ShortMessage"/> source address <see cref="SmppAddress"/>.
        /// </summary>
        public SmppAddress SourceAddress 
        {
        	  //Set the source address as read only the address must be changed inside SmppAddress class.
            get { return vSourceAddress; }
        }
        /// <summary>
        /// Gets the <see cref="ShortMessage"/> destination address <see cref="SmppAddress"/>.
        /// </summary>
        public SmppAddress DestinationAddress
        {
        	  //Set the destination address as read only the address must be changed inside SmppAddress class.
            get { return vDestinationAddress; }
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
