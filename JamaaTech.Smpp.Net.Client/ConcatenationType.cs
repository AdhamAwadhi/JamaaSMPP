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

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// The type of concatenation used in message segments.
    /// </summary>
    public enum ConcatenationType
    {
        UDH8bit,

        /// <summary>
        /// Uses User Data Header with 16 bit reference number
        /// </summary>
        UDH16bit,

        SAR
    }
}