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
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public sealed class ReplaceSm : SmOperationPDU
    {
        #region Variables
        private string vScheduleDeliveryTime;
        private string vValidityPeriod;
        private RegisteredDelivery vRegisteredDelivery;
        private byte vSmDefaultMessageID;
        private byte vSmLength;
        private string vShortMessage;
        #endregion

        #region Constructors
        public ReplaceSm(SmppEncodingService smppEncodingService)
            : base(new PDUHeader(CommandType.ReplaceSm), smppEncodingService)
        {
            vScheduleDeliveryTime = "";
            vValidityPeriod = "";
            vRegisteredDelivery = RegisteredDelivery.None;
            vSmDefaultMessageID = 0;
            vShortMessage = "";
            vSmLength = 0;
        }

        public ReplaceSm(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
             vScheduleDeliveryTime = "";
            vValidityPeriod = "";
            vRegisteredDelivery = RegisteredDelivery.None;
            vSmDefaultMessageID = 0;
            vShortMessage = "";
            vSmLength = 0;
       }
        #endregion

        #region Properties
        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.ESME; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Transmitter; }
        }

        public string ScheduleDeliveryTime
        {
            get { return vScheduleDeliveryTime; }
            set { vScheduleDeliveryTime = value; }
        }

        public string ValidityPeriod
        {
            get { return vValidityPeriod; }
            set { vValidityPeriod = value; }
        }

        public RegisteredDelivery RegisteredDelivery
        {
            get { return vRegisteredDelivery; }
            set { vRegisteredDelivery = value; }
        }

        public byte SmDefaultMessageID
        {
            get { return vSmDefaultMessageID; }
            set { vSmDefaultMessageID = value; }
        }

        public byte SmLength
        {
            get { return vSmLength; }
        }

        public string ShortMessage
        {
            get { return vShortMessage; }
            set { vShortMessage = value; }
        }
        #endregion

        #region Methods
        public override ResponsePDU CreateDefaultResponce()
        {
            PDUHeader header = new PDUHeader(CommandType.ReplaceSm,vHeader.SequenceNumber);
            return new ReplaceSmResp(header, vSmppEncodingService);
        }

        protected override byte[] GetBodyData()
        {
            ByteBuffer buffer = new ByteBuffer(64);
            buffer.Append(EncodeCString(vMessageID, vSmppEncodingService));
            buffer.Append(vSourceAddress.GetBytes(vSmppEncodingService));
            buffer.Append(EncodeCString(vScheduleDeliveryTime, vSmppEncodingService));
            buffer.Append(EncodeCString(vValidityPeriod, vSmppEncodingService));
            buffer.Append((byte)vRegisteredDelivery);
            buffer.Append((byte)vSmDefaultMessageID);
            byte[] shortMessage = EncodeCString(vShortMessage, vSmppEncodingService);
            vSmLength = (byte)shortMessage.Length;
            buffer.Append((byte)vSmLength);
            buffer.Append(shortMessage);
            return buffer.ToBytes();
       }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            vMessageID = DecodeCString(buffer, vSmppEncodingService);
            vSourceAddress = SmppAddress.Parse(buffer, vSmppEncodingService);
            vScheduleDeliveryTime = DecodeCString(buffer, vSmppEncodingService);
            vValidityPeriod = DecodeCString(buffer, vSmppEncodingService);
            vRegisteredDelivery = (RegisteredDelivery)GetByte(buffer);
            vSmDefaultMessageID = GetByte(buffer);
            vSmLength = GetByte(buffer);
            vShortMessage = DecodeString(buffer, (int)vSmLength, vSmppEncodingService);
            //This pdu has no option parameters,
            //If there is something left in the buffer,
            //then we have more than required bytes
            if (buffer.Length > 0) { throw new TooManyBytesException(); }
        }
        #endregion
    }
}
