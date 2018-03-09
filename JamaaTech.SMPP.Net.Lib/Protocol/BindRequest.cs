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
using JamaaTech.Smpp.Net.Lib.Util;
using JamaaTech.Smpp.Net.Lib;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public abstract class BindRequest : RequestPDU
    {
        #region Variables
        private string vSystemID;
        private string vPassword;
        private string vSystemType;
        private TypeOfNumber vAddressTon;
        private NumberingPlanIndicator vAddressNpi;
        private byte vInterfaceVersion;
        private string vAddressRange;
        #endregion

        #region Constructors
        internal BindRequest(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vSystemID = "";
            vPassword = "";
            vSystemType = "";
            vAddressTon = TypeOfNumber.International; //International
            vAddressNpi = NumberingPlanIndicator.ISDN; //ISDN
            vInterfaceVersion = 34; //SMPP 3.4 version
            vAddressRange = "";
        }
        #endregion

        #region Properties

        public override SmppEntityType AllowedSource
        {
            get { return SmppEntityType.ESME; }
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Open; }
        }

        public string SystemID
        {
            get { return vSystemID; }
            set { vSystemID = value; }
        }

        public string Password
        {
            get { return vPassword; }
            set { vPassword = value; }
        }

        public string SystemType
        {
            get { return vSystemType; }
            set { vSystemType = value; }
        }

        public TypeOfNumber AddressTon
        {
            get { return vAddressTon; }
            set { vAddressTon = value; }
        }

        public NumberingPlanIndicator AddressNpi
        {
            get { return vAddressNpi; }
            set { vAddressNpi = value; }
        }

        public byte InterfaceVersion
        {
            get { return vInterfaceVersion; }
            set { vInterfaceVersion = value; }
        }

        public string AddressRange
        {
            get { return vAddressRange; }
            set { vAddressRange = value; }
        }
        #endregion

        #region Methods
        #region Interface Methods
        public override ResponsePDU CreateDefaultResponce()
        {
            CommandType cmdType = CommandType.BindTransceiverResp;
            switch (vHeader.CommandType)
            {
                case CommandType.BindReceiver:
                    cmdType = CommandType.BindReceiverResp;
                    break;
                case CommandType.BindTransmitter:
                    cmdType = CommandType.BindTransmitterResp;
                    break;
            }
            PDUHeader header = new PDUHeader(cmdType, vHeader.SequenceNumber);
            return (BindResponse)CreatePDU(header, vSmppEncodingService);
        }

        protected override byte[] GetBodyData()
        {
            ByteBuffer buffer = new ByteBuffer(32);
            buffer.Append(EncodeCString(vSystemID, vSmppEncodingService));
            buffer.Append(EncodeCString(vPassword, vSmppEncodingService));
            buffer.Append(EncodeCString(vSystemType, vSmppEncodingService));
            buffer.Append(vInterfaceVersion);
            buffer.Append((byte)vAddressTon);
            buffer.Append((byte)vAddressNpi);
            buffer.Append(EncodeCString(vAddressRange, vSmppEncodingService));
            return buffer.ToBytes();
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            const int minBytes = 7;
            if (buffer.Length < minBytes) { throw new NotEnoughBytesException("BindRequest requires at least 7 bytes for body parameters"); }
            try
            {
                vSystemID = DecodeCString(buffer, vSmppEncodingService);
                vPassword = DecodeCString(buffer, vSmppEncodingService);
                vSystemType = DecodeCString(buffer, vSmppEncodingService);
                vInterfaceVersion = GetByte(buffer);
                vAddressTon = (TypeOfNumber)GetByte(buffer);
                vAddressNpi = (NumberingPlanIndicator)GetByte(buffer);
                vAddressRange = DecodeCString(buffer, vSmppEncodingService);
            }
            catch (InvalidOperationException ex)
            {
                //ByteBuffer.Remove() throws InvalidOperationException exception if called on a empty ByteBuffer instance
                //Wrap this exception as a NotEnoughBytesException exception
                throw new NotEnoughBytesException(ex.Message,ex);
            }
            if (buffer.Length > 0) //If there are some bytes left
            { throw new TooManyBytesException(); }
        }
        #endregion

        #region Helper Methods
        #endregion
        #endregion
    }
}
