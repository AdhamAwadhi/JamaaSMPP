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

namespace JamaaTech.Smpp.Net.Client
{
    public class MultiPartTextMessage : TextMessage
    {
        protected override SubmitSm CreateSubmitSm(SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
        {
            var sm = new SubmitSmMultiPart(smppEncodingService, destAddress, srcAddress);

            return sm;
        }
    }

    public class SubmitSmMultiPart : SubmitSm
    {
        private bool _hasResponse;

        #region Properties
        public override bool HasResponse
        {
            get { return _hasResponse; }
        }
        public override bool EmptyResponse
        {
            get { return true; }
        }
        #endregion

        #region Constructors
        public SubmitSmMultiPart(JamaaTech.Smpp.Net.Lib.SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
            : base(smppEncodingService, destAddress, srcAddress)
        {
        }

        public SubmitSmMultiPart(PDUHeader header, JamaaTech.Smpp.Net.Lib.SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
            : base(header, smppEncodingService, destAddress, srcAddress)
        {
        }
        #endregion

        public override void SetMessageText(string message, DataCoding dataCoding, Udh udh)
        {
            _hasResponse = udh == null ? true : udh.IsLast;
            base.SetMessageText(message, dataCoding, udh);
        }

    }
}
