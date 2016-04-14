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
    public static class ServiceType
    {
        #region Constants
        public const string DEFAULT = ""; //Empty string
        public const string CELLULAR_MESSAGING = "CMT";
        public const string CELLULAR_PAGING = "CPT";
        public const string VOICE_MAIL_NOTIFICATION = "VMN";
        public const string VOICE_MAIL_ALERTING = "VMA";
        public const string WIRELESS_APPLICATION_PROTOCOL = "WAP";
        public const string UNSTRUCTURED_SUPPLIMENTARY_SERVICE_DATA = "USSD";
        #endregion
    }
}
