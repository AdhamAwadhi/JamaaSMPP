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

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public sealed class SmppAddress
    {
        #region Variables
        private TypeOfNumber vTon;
        private NumberingPlanIndicator vNpi;
        private string vAddress;
        #endregion

        #region Constructors
        public SmppAddress()
        {
            vTon = TypeOfNumber.International;
            vNpi = NumberingPlanIndicator.ISDN;
            vAddress = "";
        }

        public SmppAddress(TypeOfNumber ton, NumberingPlanIndicator npi, string address)
        {
            vTon = ton;
            vNpi = npi;
            vAddress = address;
        }
        #endregion

        #region Properties
        public TypeOfNumber Ton
        {
            get { return vTon; }
            set { vTon = value; }
        }

        public NumberingPlanIndicator Npi
        {
            get { return vNpi; }
            set { vNpi = value; }
        }

        public string Address
        {
            get { return vAddress; }
            set { vAddress = value; }
        }
        #endregion

        #region Methods
        internal static SmppAddress Parse(ByteBuffer buffer, SmppEncodingService smppEncodingService)
        {
            //We require at least 3 bytes for SMPPAddress instance to be craeted
            if (buffer.Length < 3) { throw new NotEnoughBytesException("SMPPAddress requires at least 3 bytes"); }
            TypeOfNumber ton = (TypeOfNumber)PDU.GetByte(buffer);
            NumberingPlanIndicator npi = (NumberingPlanIndicator)PDU.GetByte(buffer);
            string address = PDU.DecodeCString(buffer, smppEncodingService);
            return new SmppAddress(ton, npi, address);
        }

        public byte[] GetBytes(SmppEncodingService smppEncodingService)
        {
            //Approximate buffer required;
            int capacity = 4 + vAddress == null ? 1 : vAddress.Length;
            ByteBuffer buffer = new ByteBuffer(capacity);
            buffer.Append((byte)vTon);
            buffer.Append((byte)vNpi);
            buffer.Append(PDU.EncodeCString(vAddress, smppEncodingService));
            return buffer.ToBytes();
        }

        #region override
        public override string ToString()
        {
            return string.Format("{{Address:{0}, Ton:{1}, Npi:{2}}}", Address, Ton, Npi);
        }
        #endregion
        #endregion
    }
}
