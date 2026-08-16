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
using System.Collections.Generic;

namespace JamaaTech.Smpp.Net.Client
{
    public class TextMessagePayload : TextMessage
    {
        protected override SubmitSm CreateSubmitSm(SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
        {
            var sm = new SubmitSmMessagePayload(smppEncodingService, destAddress, srcAddress);

            return sm;
        }

        protected override IEnumerable<SendSmPDU> GetPDUs(DataCoding defaultEncoding, SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
        {
            destAddress = destAddress ?? new SmppAddress() { Address = vDestinatinoAddress };
            srcAddress = srcAddress ?? new SmppAddress() { Address = vSourceAddress };
            SubmitSm sm = CreateSubmitSm(smppEncodingService, destAddress, srcAddress);
            sm.DataCoding = defaultEncoding;

            if (SubmitUserMessageReference)
                sm.SetOptionalParamString(Lib.Protocol.Tlv.Tag.user_message_reference, UserMessageReference);

            if (SubmitReceiptedMessageId)
                sm.SetOptionalParamString(Lib.Protocol.Tlv.Tag.receipted_message_id, ReceiptedMessageId);

            if (vRegisterDeliveryNotification)
                sm.RegisteredDelivery = RegisteredDelivery.DeliveryReceipt;


            sm.SetMessageText(Text, defaultEncoding);
            yield return sm;
        }
    }

    public class SubmitSmMessagePayload : SubmitSm
    {

        #region Constructors
        public SubmitSmMessagePayload(JamaaTech.Smpp.Net.Lib.SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
            : base(smppEncodingService, destAddress, srcAddress)
        {
        }

        public SubmitSmMessagePayload(PDUHeader header, JamaaTech.Smpp.Net.Lib.SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
            : base(header, smppEncodingService, destAddress, srcAddress)
        {
        }
        #endregion

        public override void SetMessageText(string message, DataCoding dataCoding, Udh udh)
        {
            var bytes = vSmppEncodingService.GetBytesFromCString(message, dataCoding, false);
            this.SetOptionalParamBytes(Tag.message_payload, bytes);
        }
    }
}
