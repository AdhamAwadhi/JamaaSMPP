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
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib;

namespace JamaaTech.Smpp.Net.Client
{
    public class TextMessage : ShortMessage
    {
        #region Variables
        private string vText;
        private int vMaxMessageLength;
        private bool vUseUdh16Bit;
        #endregion

        #region Constuctors
        /// <summary>
        /// Initializes a new instance of <see cref="TextMessage"/>
        /// </summary>
        public TextMessage()
            : base()
        {
            vText = "";
        }

        internal TextMessage(int segmentId, int messageCount, int sequenceNumber)
            : base(segmentId, messageCount, sequenceNumber)
        {
            vText = "";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a <see cref="System.String"/> value representing the text content of the message
        /// </summary>
        public string Text
        {
            get { return vText; }
            set { vText = value; }
        }

        public int MaxMessageLength { get { return vMaxMessageLength; } }

        /// <summary>
        /// Gets or sets a value indicating whether concatenated messages use a
        /// 16-bit UDH reference number. The default is <see langword="false"/>.
        /// </summary>
        public bool UseUdh16Bit
        {
            get { return vUseUdh16Bit; }
            set { vUseUdh16Bit = value; }
        }
        #endregion

        #region Methods
        protected override IEnumerable<SendSmPDU> GetPDUs(DataCoding dataCoding, SmppAddress srcAddress = null)
        {
            var destAddressLocal = new SmppAddress() { Address = vDestinatinoAddress };
            var srcAddressLocal = srcAddress ?? new SmppAddress() { Address = vSourceAddress };
            Func<SubmitSm> smFactory = () =>
            {
                SubmitSm sm = CreateSubmitSm(destAddressLocal, srcAddressLocal);
                sm.DataCoding = dataCoding;

                if (SubmitUserMessageReference)
                    sm.SetOptionalParamString(Lib.Protocol.Tlv.Tag.user_message_reference, UserMessageReference);

                if (SubmitReceiptedMessageId)
                    sm.SetOptionalParamString(Lib.Protocol.Tlv.Tag.receipted_message_id, ReceiptedMessageId);

                if (vRegisterDeliveryNotification)
                    sm.RegisteredDelivery = RegisteredDelivery.DeliveryReceipt;

                return sm;
            };

            vMaxMessageLength = StringHelper.GetMaxMessageLength(dataCoding, false);
            byte[] bytes = SmppEncodingService.Instance.GetBytesFromString(vText, dataCoding);

            // Unicode encoding return 2 items for 1 char 
            // We check vText Length first
            if (vText.Length > vMaxMessageLength && bytes.Length > vMaxMessageLength) // Split into multiple!
            {
                var SegID = SegmentIdGeneratorFactory.Generator.NextSegmentId(srcAddressLocal.Address, destAddressLocal.Address);
                vMaxMessageLength = StringHelper.GetMaxMessageLength(dataCoding, true, vUseUdh16Bit);
                var messages = StringHelper.Split(vText, vMaxMessageLength);
                var totalSegments = messages.Count; // get the number of (how many) parts
                Udh udh = vUseUdh16Bit
                    ? new Udh16(SegID, totalSegments, 0)
                    : new Udh(SegID, totalSegments, 0); // ID, Total, part

                for (int i = 0; i < totalSegments; i++)
                {
                    udh.MessageSequence = i + 1;  // seq+1 , - parts of the message      
                    var sm = smFactory();
                    sm.SetMessageText(messages[i], dataCoding, udh); // send parts of the message + all other UDH settings
                    yield return sm;
                }
            }
            else
            {
                var sm = smFactory();
                sm.SetMessageText(vText, dataCoding);
                yield return sm;
            }
        }

        protected virtual SubmitSm CreateSubmitSm(SmppAddress destAddress = null, SmppAddress srcAddress = null)
        {
            var sm = new SubmitSm(destAddress, srcAddress);

            return sm;
        }
        #endregion

        #region Overriden System.Object Members
        public override string ToString()
        {
            return vText == null ? "" : vText;
        }
        #endregion
    }
}
