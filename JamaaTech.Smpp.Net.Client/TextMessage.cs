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
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;
using System;
using System.Collections.Generic;

namespace JamaaTech.Smpp.Net.Client
{
    public class TextMessage : ShortMessage
    {
        #region Variables
        private string vText;
        private int vMaxMessageLength;
        private ConcatenationType vConcatenationType;
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

        public ConcatenationType ConcatenationType
        {
            get { return vConcatenationType; }
            set { vConcatenationType = value; }
        }
        #endregion

        #region Methods
        protected override IEnumerable<SendSmPDU> GetPDUs(DataCoding dataCoding, SmppAddress srcAddress = null)
        {
            var destAddressLocal = new SmppAddress() { Address = vDestinatinoAddress };
            var srcAddressLocal = srcAddress ?? new SmppAddress() { Address = vSourceAddress };

            vMaxMessageLength = StringHelper.GetMaxMessageLength(dataCoding, false);
            byte[] bytes = SmppEncodingService.Instance.GetBytesFromString(vText, dataCoding);

            // Unicode encoding return 2 items for 1 char 
            // We check vText Length first
            if (vText.Length > vMaxMessageLength && bytes.Length > vMaxMessageLength) // Split into multiple!
            {
                foreach (var sm in SplitMessage(dataCoding, destAddressLocal, srcAddressLocal))
                    yield return sm;
            }
            else
            {
                var sm = CreateSubmitSm(dataCoding, destAddressLocal, srcAddressLocal);
                sm.SetMessageText(vText, dataCoding);
                yield return sm;
            }
        }

        private IEnumerable<SendSmPDU> SplitMessage(DataCoding dataCoding, SmppAddress destAddress, SmppAddress srcAddress)
        {
            return vConcatenationType switch
            {
                ConcatenationType.SAR => SplitMessageSar(dataCoding, destAddress, srcAddress),
                ConcatenationType.UDH16bit => SplitMessageUdh(dataCoding, destAddress, srcAddress, true),
                _ => SplitMessageUdh(dataCoding, destAddress, srcAddress, false)
            };
        }

        private IEnumerable<SendSmPDU> SplitMessageSar(DataCoding dataCoding, SmppAddress destAddress, SmppAddress srcAddress)
        {
            var SegID = SegmentIdGeneratorFactory.Generator.NextSegmentId(srcAddress.Address, destAddress.Address);
            vMaxMessageLength = StringHelper.GetMaxMessageLength(dataCoding, false);
            var messages = StringHelper.Split(vText, vMaxMessageLength);
            var totalSegments = (byte)messages.Count; // get the number of (how many) parts

            byte segNo = 0;

            for (int i = 0; i < totalSegments; i++)
            {
                segNo++; // seq+1 , - parts of the message      

                var sm = CreateSubmitSm(dataCoding, destAddress, srcAddress);
                sm.SetOptionalParamByte<bool>(Tag.more_messages_to_send, segNo != totalSegments);
                sm.SetOptionalParamByte<byte>(Tag.number_of_messages, totalSegments);
                sm.SetOptionalParamByte<byte>(Tag.sar_total_segments, totalSegments);
                sm.SetOptionalParamByte<byte>(Tag.sar_msg_ref_num, (byte)SegID);
                sm.SetOptionalParamByte<byte>(Tag.sar_segment_seqnum, segNo);

                sm.SetMessageText(messages[i], dataCoding); // send parts of the message 
                yield return sm;
            }
        }

        private IEnumerable<SendSmPDU> SplitMessageUdh(DataCoding dataCoding, SmppAddress destAddress, SmppAddress srcAddress, bool useUdh16Bit)
        {
            var SegID = SegmentIdGeneratorFactory.Generator.NextSegmentId(srcAddress.Address, destAddress.Address);
            vMaxMessageLength = StringHelper.GetMaxMessageLength(dataCoding, true, useUdh16Bit);
            var messages = StringHelper.Split(vText, vMaxMessageLength);
            var totalSegments = messages.Count; // get the number of (how many) parts
            Udh udh = useUdh16Bit
                ? new Udh16(SegID, totalSegments, 0)
                : new Udh(SegID, totalSegments, 0); // ID, Total, part

            for (int i = 0; i < totalSegments; i++)
            {
                udh.MessageSequence = i + 1;  // seq+1 , - parts of the message      
                var sm = CreateSubmitSm(dataCoding, destAddress, srcAddress);
                sm.SetMessageText(messages[i], dataCoding, udh); // send parts of the message + all other UDH settings
                yield return sm;
            }
        }

        protected SubmitSm CreateSubmitSm(DataCoding dataCoding, SmppAddress destAddress, SmppAddress srcAddress)
        {
            SubmitSm sm = CreateSubmitSm(destAddress, srcAddress);
            sm.DataCoding = dataCoding;

            if (SubmitUserMessageReference)
                sm.SetOptionalParamString(Lib.Protocol.Tlv.Tag.user_message_reference, UserMessageReference);

            if (SubmitReceiptedMessageId)
                sm.SetOptionalParamString(Lib.Protocol.Tlv.Tag.receipted_message_id, ReceiptedMessageId);

            if (vRegisterDeliveryNotification)
                sm.RegisteredDelivery = RegisteredDelivery.DeliveryReceipt;

            return sm;
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
