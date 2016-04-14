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

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public enum CommandType : uint
    {
        GenericNack =  0x80000000,
        BindReceiver = 0x00000001,
        BindReceiverResp = 0x80000001,
        BindTransmitter = 0x00000002,
        BindTransmitterResp = 0x80000002,
        QuerySm = 0x00000003,
        QuerySmResp = 0x80000003,
        SubmitSm = 0x00000004,
        SubmitSmResp = 0x80000004,
        DeliverSm = 0x00000005,
        DeliverSmResp = 0x80000005,
        UnBind = 0x00000006,
        UnBindResp = 0x80000006,
        ReplaceSm = 0x00000007,
        ReplaceSmResp = 0x80000007,
        CancelSm = 0x00000008,
        CancelSmResp = 0x80000008,
        BindTransceiver = 0x00000009,
        BindTransceiverResp = 0x80000009,
        OutBind = 0x0000000b,
        EnquireLink = 0x00000015,
        EnquireLinkResp = 0x80000015,
        DataSm = 0x00000103,
        DataSmResp =0x80000103,
        AlertNotification = 0x00000103,
        SubmitMulti = 0x00000021,
        SubmitMultiResp = 0x80000012
    }
}
