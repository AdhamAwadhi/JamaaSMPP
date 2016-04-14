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
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Client
{
    public class TextMessage : ShortMessage
    {
        #region Variables
        private string vText;
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
            :base(segmentId,messageCount,sequenceNumber)
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
        #endregion

        #region Methods
        protected override IEnumerable<SendSmPDU> GetPDUs(DataCoding defaultEncoding)
        {
            //This smpp implementation does not support sending concatenated messages,
            //however, concatenated messages are supported on the receiving side.
            int maxLength = GetMaxMessageLength(defaultEncoding, false);
            byte[] bytes = SMPPEncodingUtil.GetBytesFromString(vText, defaultEncoding);
            //Check message size
            if(bytes.Length > maxLength)
            {
                throw new InvalidOperationException(string.Format(
                    "Encoding '{0}' does not support messages of length greater than '{1}' charactors",
                    defaultEncoding, maxLength));
            }
            SubmitSm sm = new SubmitSm();
            sm.SetMessageBytes(bytes);
            sm.SourceAddress.Address = vSourceAddress;
            sm.DestinationAddress.Address = vDestinatinoAddress;
            sm.DataCoding = defaultEncoding;
            if (vRegisterDeliveryNotification) { sm.RegisteredDelivery = RegisteredDelivery.DeliveryReceipt; }
            yield return sm;
        }

        private static int GetMaxMessageLength(DataCoding encoding, bool includeUdh)
        {
            switch (encoding)
            {
                case DataCoding.SMSCDefault:
                    return includeUdh ? 153 : 160;
                case DataCoding.Latin1:
                    return includeUdh ? 134 : 140;
                case DataCoding.ASCII:
                    return includeUdh ? 153 : 160;
                case DataCoding.UCS2:
                    return includeUdh ? 67 : 70;
                default:
                    throw new InvalidOperationException("Invalid or unsuported encoding for text message ");
            }
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
