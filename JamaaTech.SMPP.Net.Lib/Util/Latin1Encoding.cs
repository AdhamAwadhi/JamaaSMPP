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

namespace JamaaTech.Smpp.Net.Lib.Util
{
    public static class Latin1Encoding
    {
        #region Variables
        private static System.Text.Encoding vEncoding;
        #endregion

        #region Type Initializer
        static Latin1Encoding()
        {
            vEncoding = System.Text.Encoding.GetEncoding(28591/*"iso-8859-1"*/); //Latin 1 encoding
        }
        #endregion

        #region Methods
        public static byte[] GetBytes(string str)
        {
            return vEncoding.GetBytes(str);
        }

        public static string GetString(byte[] bytes)
        {
            return vEncoding.GetString(bytes);
        }
        #endregion
    }
}
