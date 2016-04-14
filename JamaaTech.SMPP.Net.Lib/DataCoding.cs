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
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Lib
{
    /// <summary>
    /// Specifies the data coding scheme used to encode a message in a PDU
    /// </summary>
    /// <remarks>
    /// You must set the data coding scheme that must be used to decode a short message contained in a <see cref="SubmitSm"/> PDU 
    /// when sending short messages. When a <see cref="DeliverSm"/> PDU is received, the <see cref="DeliverSm.DataCoding"/> field 
    /// should also contain one of the enumeration members which can be used to choose appropiate scheme to decode the message.
    /// Note that, in this implementation only the SMSCDefault, ASCII, Latin1 and UCS2 encodings are supported.
    /// </remarks>
    /// <seealso cref="JamaaTech.Smpp.Net.Lib.Encoding.SmppEncoding"/>
    [Flags()]
    public enum DataCoding : byte
    {
        /// <summary>
        /// SMSC default alphabet
        /// </summary>
        SMSCDefault = 0x00,
        /// <summary>
        /// IA5 (CCITT T.50)/ASCII (ANSI X3.4)
        /// </summary>
        ASCII = 0x01,
        /// <summary>
        /// Octet unspecified (8-bit binary)
        /// </summary>
        Octet1 = 0x02,
        /// <summary>
        /// Latin 1 (ISO-8859-1)
        /// </summary>
        Latin1 = 0x03,
        /// <summary>
        /// Octet unspecified (8-bit binary)
        /// </summary>
        Octet2 = 0x04,
        /// <summary>
        /// JIS (X 0208-1990)
        /// </summary>
        JIS = 0x05,
        /// <summary>
        /// Cyrllic (ISO-8859-5)
        /// </summary>
        Cyrllic = 0x06,
        /// <summary>
        /// Latin/Hebrew (ISO-8859-8)
        /// </summary>
        Latin_Hebrew = 0x07,
        /// <summary>
        /// UCS2 (ISO/IEC-10646)
        /// </summary>
        UCS2 = 0x08,
        /// <summary>
        /// Pictogram Encoding
        /// </summary>
        Pictogram = 0x09,
        /// <summary>
        /// ISO-2022-JP (Music Codes)
        /// </summary>
        MusicCodes = 0x0a,
        /// <summary>
        /// Extended Kanji JIS(X 0212-1990)
        /// </summary>
        ExtendedKanji = 0x0d,
        /// <summary>
        /// KS C 5601
        /// </summary>
        KS_C_5601 = 0x0e,
        /// <summary>
        /// GSM MWI control
        /// </summary>
        GSM_MWI_1 = 0xc0,
        /// <summary>
        /// GMS MWI control
        /// </summary>
        GSM_MWI_2 = 0xd0,
        /// <summary>
        /// GMS message class control
        /// </summary>
        GMS_MessageClass = 0xf0
    }
}
