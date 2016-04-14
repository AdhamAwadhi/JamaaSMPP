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
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public sealed class DataSm : SingleDestinationPDU
    {
        #region Properties
        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.Any; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Transceiver; }
        }
        #endregion

        #region Constructors
        public DataSm()
            : base(new PDUHeader(CommandType.DataSm)) { }

        internal DataSm(PDUHeader header)
            : base(header) { }
        #endregion

        #region Methods
        public override ResponsePDU CreateDefaultResponce()
        {
            PDUHeader header = new PDUHeader(CommandType.DataSmResp, vHeader.SequenceNumber);
            return new DataSmResp(header);
        }

        protected override byte[] GetBodyData()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Append(EncodeCString(vServiceType));
            buffer.Append(vSourceAddress.GetBytes());
            buffer.Append(vDestinationAddress.GetBytes());
            buffer.Append((byte)vEsmClass);
            buffer.Append((byte)vRegisteredDelivery);
            buffer.Append((byte)vDataCoding);
            return buffer.ToBytes();
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            vServiceType = DecodeCString(buffer);
            vSourceAddress = SmppAddress.Parse(buffer);
            vDestinationAddress = SmppAddress.Parse(buffer);
            vEsmClass = (EsmClass)GetByte(buffer);
            vRegisteredDelivery = (RegisteredDelivery)GetByte(buffer);
            vDataCoding = (DataCoding)GetByte(buffer);
            if (buffer.Length > 0) { vTlv = TlvCollection.Parse(buffer); }
        }

        public override byte[] GetMessageBytes()
        {
            //Check if optional parameter message_payload is present 
            Tlv.Tlv tlv = Tlv.GetTlvByTag(Tag.message_payload);
            if (tlv == null) { return null; }
            else { return tlv.RawValue; }
        }

        public override void SetMessageBytes(byte[] message)
        {
            if (message == null) { throw new ArgumentNullException("message"); }
            //Check if optional parameter message_payload is present 
            Tlv.Tlv tlv = Tlv.GetTlvByTag(Tag.message_payload);
            if (tlv == null) { throw new InvalidOperationException("Tlv parameter 'message_payload' is not present"); }
            tlv.ParseValue(message);
        }
        #endregion
    }
}
