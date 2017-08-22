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

namespace JamaaTech.Smpp.Net.Lib.Util
{
    public static class SMPPEncodingUtil
    {
        public static System.Text.Encoding UCS2Encoding { get; set; } = System.Text.Encoding.Unicode;

        #region Methods
        public static byte[] GetBytesFromInt(uint value)
        {
            byte[] result = new byte[4];
            result[0] = (byte)(value >> 24);
            result[1] = (byte)(value >> 16);
            result[2] = (byte)(value >> 8);
            result[3] = (byte)(value);
            return result;
        }

        public static uint GetIntFromBytes(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.Length != 4) { throw new ArgumentException("Array length must be equal to four(4)", "data"); }
            uint result = 0x00000000;
            result |= data[0];
            result <<= 8;
            result |= data[1];
            result <<= 8;
            result |= data[2];
            result <<= 8;
            result |= data[3];
            return result;
        }

        public static byte[] GetBytesFromShort(ushort value)
        {
            byte[] result = new byte[2];
            result[0] = (byte)(value >> 8);
            result[1] = (byte)(value);
            return result;
        }

        public static ushort GetShortFromBytes(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.Length != 2) { throw new ArgumentException("Array length must be equal to two (2)", "data"); }
            ushort result = 0x0000;
            result |= data[0];
            result <<= 8;
            result |= data[1];
            return result;
        }

        public static byte[] GetBytesFromCString(string cStr)
        {
            return GetBytesFromCString(cStr, DataCoding.ASCII);
        }

        public static byte[] GetBytesFromCString(string cStr, DataCoding dataCoding)
        {
            if (cStr == null) { throw new ArgumentNullException("cStr"); }
            if (cStr.Length == 0) { return new byte[] { 0x00 }; }
            byte[] bytes = null;
            switch (dataCoding)
            {
                case DataCoding.ASCII:
                    bytes = System.Text.Encoding.ASCII.GetBytes(cStr);
                    break;
                case DataCoding.Latin1:
                    bytes = Latin1Encoding.GetBytes(cStr);
                    break;
                case DataCoding.UCS2:
                    bytes = UCS2Encoding.GetBytes(cStr);
                    break;
                case DataCoding.SMSCDefault:
                    bytes = SMSCDefaultEncoding.GetBytes(cStr);
                    break;
                default:
                    throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Unsupported encoding");
            }
            ByteBuffer buffer = new ByteBuffer(bytes, bytes.Length + 1);
            buffer.Append(new byte[] { 0x00 }); //Append a null charactor a the end
            return buffer.ToBytes();
        }

        public static string GetCStringFromBytes(byte[] data)
        {
            return GetCStringFromBytes(data, DataCoding.ASCII);
        }

        public static string GetCStringFromBytes(byte[] data, DataCoding dataCoding)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.Length < 1) { throw new ArgumentException("Array cannot be empty","data"); }
            if (data[data.Length - 1] != 0x00) { throw new ArgumentException("CString must be terminated with a null charactor","data"); }
            if (data.Length == 1) { return ""; } //The string is empty if it contains a single null charactor
            string result = null;
            switch (dataCoding)
            {
                case DataCoding.ASCII:
                    result = System.Text.Encoding.ASCII.GetString(data);
                    break;
                case DataCoding.Latin1:
                    result = Latin1Encoding.GetString(data);
                    break;
                case DataCoding.SMSCDefault:
                    result = SMSCDefaultEncoding.GetString(data);
                    break;
                case DataCoding.UCS2:
                    result = UCS2Encoding.GetString(data);
                    break;
                default:
                    throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Unsupported encoding");
            }
            return result.Replace("\x00","");//Replace the terminating null charactor
        }

        public static byte[] GetBytesFromString(string cStr)
        {
            return GetBytesFromCString(cStr, DataCoding.ASCII);
        }

        public static byte[] GetBytesFromString(string cStr,DataCoding dataCoding)
        {
            if (cStr == null) { throw new ArgumentNullException("cStr"); }
            if (cStr.Length == 0) { return new byte[] { 0x00 }; }
            byte[] bytes = null;
            switch (dataCoding)
            {
                case DataCoding.ASCII:
                    bytes = System.Text.Encoding.ASCII.GetBytes(cStr);
                    break;
                case DataCoding.Latin1:
                    bytes = Latin1Encoding.GetBytes(cStr);
                    break;
                case DataCoding.UCS2:
                    bytes = UCS2Encoding.GetBytes(cStr);
                    break;
                case DataCoding.SMSCDefault:
                    bytes = SMSCDefaultEncoding.GetBytes(cStr);
                    break;
                default:
                    throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Unsupported encoding");
            }
            return bytes;
        }

        public static string GetStringFromBytes(byte[] data)
        {
            return GetStringFromBytes(data, DataCoding.ASCII);
        }

        public static string GetStringFromBytes(byte[] data, DataCoding dataCoding)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            string result = null;
            switch (dataCoding)
            {
                case DataCoding.ASCII:
                    result = System.Text.Encoding.ASCII.GetString(data);
                    break;
                case DataCoding.Latin1:
                    result = Latin1Encoding.GetString(data);
                    break;
                case DataCoding.SMSCDefault:
                    result = SMSCDefaultEncoding.GetString(data);
                    break;
                case DataCoding.UCS2:
                    result = UCS2Encoding.GetString(data);
                    break;
                case DataCoding.Octet1:
                case DataCoding.Octet2:
                    StringBuilder hex = new StringBuilder(data.Length * 2);
                    foreach (byte b in data)
                        hex.AppendFormat("{0:x2}", b);
                    result = hex.ToString();
                    break;
                default:
                    throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Unsupported encoding");
            }
            //Since a CString may contain a null terminating charactor
            //Replace all occurences of null charactors
            return result.Replace("\u0000","");

        }
        
        #endregion
    }
}
