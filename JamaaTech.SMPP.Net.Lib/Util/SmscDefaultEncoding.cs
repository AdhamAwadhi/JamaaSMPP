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
    public static class SMSCDefaultEncoding
    {
        private static GSMEncoding gsm = new GSMEncoding();

        public static bool UseGsmEncoding { get; set; } = true;

        #region Variables
        private static char[] vDefaultForwardTable;
        private static byte[] vDefaultReverseTable;
        //private static char[] vExtensionTable;
        #endregion

        #region Type Initializer
        static SMSCDefaultEncoding()
        {
            BuildTable();
        }
        #endregion

        #region Methods
        public static byte[] GetBytes(string str)
        {
            if (string.IsNullOrEmpty(str)) { return null; } //Because there would be nothing to encode
            // Use Gsm Encoding
            if (UseGsmEncoding) return gsm.GetBytes(str);

            ByteBuffer buffer = new ByteBuffer(str.Length);
            foreach (char @char in str) //For each charactor in the string
            {
                byte @byte = (byte)@char; //Get its Latin1 byte order index
                byte index = vDefaultReverseTable[@byte]; //Get its SMSC Default byte order index
                if (index == 128) //This charactor does not exist in the table
                {
                    //Check this charactor in the extension table
                    index = GetExtendedByte(@char);
                    if (index == 0) { continue; } //This charactor is not supported even in the extension table
                    buffer.Append(0x1b); //This is the escape sequence for extended charactors
                    buffer.Append(index); //Extended charactor order index
                }
                else { buffer.Append(index); }
            }
            return buffer.ToBytes();
        }

        public static string GetString(byte[] bytes)
        {
            if (bytes == null) { throw new ArgumentNullException("bytes"); }
            // Use Gsm Encoding
            if (UseGsmEncoding) return gsm.GetString(bytes);

            StringBuilder builder = new StringBuilder(bytes.Length);
            for (int index = 0; index < bytes.Length; ++index)
            {
                byte @byte = bytes[index];
                if (@byte > 127) { continue; } //This is outside range
                if (@byte == 0x1b) //This might be an escape sequence
                {
                    //Check if there is any charactor next to this
                    if (index + 1 < bytes.Length)
                    {
                        char @extChar = GetExtendedChar(bytes[index + 1]);
                        if (@extChar != '\0') //This charactor was escaped by the sequence 0x1b
                        {
                            builder.Append(@extChar);
                            //Move to next charactor
                            index++;
                            continue; //Start over so that we don't read this byte more than once
                        }
                    }
                }
                char @char = vDefaultForwardTable[@byte]; //Get charactor corresponding to this index
                builder.Append(@char); //Append to string
            }
            return builder.ToString();
        }

        private static void BuildTable()
        {
            //Please note that, charactors from 0x10 to 0x1B are not supported by this implementation
            //Instead, they are converted to a space charactor (0x20)
            vDefaultForwardTable = new char[]
            {
                /*    0 , 1 , 2 , 3 , 4 , 5 , 6 , 7 ,  8 , 9 , 10 , 11, 12, 13, 14, 15 */
                /*0*/'@','£','$','¥','è','é','ù','ì', 'ò','Ç','\n','Ø','ø','\r','Å','å',
                ///*1*/'Δ','_','Φ','Γ','Λ','Ω','Π','Ψ', 'Σ','Θ','Ξ', '\x1B','Æ','æ', 'ß','É',
                /*1*/' ','_',' ',' ',' ',' ',' ',' ', ' ',' ',' ', '\x1B','Æ','æ', 'ß','É',
                /*2*/' ','!','"','#','¤','%','&','\'','(',')','*', '+',',','-', '.','/',
                /*3*/'0','1','2','3','4','5','6','7', '8','9',':', ';','<','=', '>','?',
                /*4*/'¡','A','B','C','D','E','F','G', 'H','I','J', 'K','L','M', 'N','O',
                /*5*/'P','Q','R','S','T','U','V','W', 'X','Y','Z', 'Ä','Ö','Ñ', 'Ü','§',
                /*6*/'¿','a','b','c','d','e','f','g', 'h','i','j', 'k','l','m', 'n','o',
                /*7*/'p','q','r','s','t','u','v','w', 'x','y','z', 'ä','ö','ñ', 'ü','à'
            };

            //This table is used for reverse lookup
            vDefaultReverseTable = new byte[byte.MaxValue];
            string chars = new string(vDefaultForwardTable, 0, vDefaultForwardTable.Length);
            int index = 0;
            for (; index < byte.MaxValue; ++index)
            {
                vDefaultReverseTable[index] = 128;
            }

            for (index = 0; index < vDefaultForwardTable.Length; ++index)
            {
                System.Diagnostics.Debug.WriteLine($"BuildTable: {index}");
                vDefaultReverseTable[vDefaultForwardTable[index]] = (byte)index;
            }
        }

        private static char GetExtendedChar(byte @byte)
        {
            switch (@byte)
            {
                case 0x14:
                    return '^';
                case 0x28:
                    return '{';
                case 0x29:
                    return '}';
                case 0x2f:
                    return '\\';
                case 0x3c:
                    return '[';
                case 0x3d:
                    return '~';
                case 0x3e:
                    return ']';
                case 0x40:
                    return '|';
                case 0x65:
                    return '€';
                default:
                    return '\0'; //returns a null char
            }
        }

        private static byte GetExtendedByte(char @char)
        {
            switch (@char)
            {
                case '^':
                    return 0x14;
                case '{':
                    return 0x28;
                case '}':
                    return 0x29;
                case '\\':
                    return 0x2f;
                case '[':
                    return 0x3c;
                case '~':
                    return 0x3d;
                case ']':
                    return 0x3e;
                case '|':
                    return 0x40;
                case '€':
                    return 0x65;
                default:
                    return 0; //returns zero
            }

        }
        #endregion
    }
}
