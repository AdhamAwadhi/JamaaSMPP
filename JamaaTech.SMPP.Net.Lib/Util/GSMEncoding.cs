﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JamaaTech.Smpp.Net.Lib.Util
{
    /// <summary>
    /// Text encoding class for the GSM 03.38 alphabet.
    /// Converts between GSM and the internal .NET Unicode character representation
    /// <!--https://github.com/mediaburst/.NET-GSM-Encoding/blob/master/GSMEncoding.cs-->
    /// </summary>
    public class GSMEncoding : System.Text.Encoding
    {
        private SortedDictionary<char, byte[]> _charToByte;
        private SortedDictionary<uint, char> _byteToChar;

        public GSMEncoding()
            : base()
        {
            PopulateDictionaries();
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            int byteCount = 0;

            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (index < 0 || index > chars.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0 || count > (chars.Length - index))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            for (int i = index; i < count; i++)
            {
                if (_charToByte.ContainsKey(chars[i]))
                {
                    byteCount += _charToByte[chars[i]].Length;
                }
            }

            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int byteCount = 0;

            // Validate the parameters.
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException("charIndex");
            }
            if (charCount < 0 || charCount > (chars.Length - charIndex))
            {
                throw new ArgumentOutOfRangeException("charCount");
            }
            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex");
            }
            if (byteIndex + GetByteCount(chars, charIndex, charCount) > bytes.Length)
            {
                throw new ArgumentException("bytes array too small", "bytes");
            }
            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                byte[] charByte;
                if (_charToByte.TryGetValue(chars[i], out charByte))
                {
                    charByte.CopyTo(bytes, byteIndex + byteCount);
                    byteCount += charByte.Length;
                }
            }
            return byteCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            int charCount = 0;

            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (index < 0 || index > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0 || count > (bytes.Length - index))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            int i = index;
            while (i < index + count)
            {
                if (bytes[i] <= 0x7F)
                {
                    if (bytes[i] == 0x1B)
                    {
                        i++;
                        if (i < bytes.Length && bytes[i] <= 0x7F)
                        {
                            charCount++; // GSM Spec says replace 1B 1B with space
                        }
                    }
                    else
                    {
                        charCount++;
                    }
                }
                i++;
            }

            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int charCount = 0;

            // Validate the parameters.
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex");
            }
            if (byteCount < 0 || byteCount > (bytes.Length - byteIndex))
            {
                throw new ArgumentOutOfRangeException("byteCount");
            }
            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException("charIndex");
            }
            if (charIndex + GetCharCount(bytes, byteIndex, byteCount) > chars.Length)
            {
                throw new ArgumentException("chars array too small", "chars");
            }


            int i = byteIndex;
            while (i < byteIndex + byteCount)
            {
                if (bytes[i] <= 0x7F)
                {
                    if (bytes[i] == 0x1B)
                    {
                        i++;
                        if (i < bytes.Length && bytes[i] <= 0x7F)
                        {
                            char nextChar;
                            uint extendedChar = (0x1B * 255) + (uint)bytes[i];
                            if (_byteToChar.TryGetValue(extendedChar, out nextChar))
                            {
                                chars[charCount] = nextChar;
                                charCount++;
                            }
                            // GSM Spec says to try for normal character if escaped one doesn't exist
                            else if (_byteToChar.TryGetValue((uint)bytes[i], out nextChar))
                            {
                                chars[charCount] = nextChar;
                                charCount++;
                            }
                        }
                    }
                    else
                    {
                        chars[charCount] = _byteToChar[(uint)bytes[i]];
                        charCount++;
                    }
                }
                i++;
            }

            return charCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException("charCount");

            return charCount * 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount");

            return byteCount;
        }


        private void PopulateDictionaries()
        {
            // Unicode char to GSM bytes
            _charToByte = new SortedDictionary<char, byte[]>();
            // GSM bytes to Unicode char
            _byteToChar = new SortedDictionary<uint, char>();

            _charToByte.Add('\u0040', new byte[] { 0x00 });
            _charToByte.Add('\u00A3', new byte[] { 0x01 });
            _charToByte.Add('\u0024', new byte[] { 0x02 });
            _charToByte.Add('\u00A5', new byte[] { 0x03 });
            _charToByte.Add('\u00E8', new byte[] { 0x04 });
            _charToByte.Add('\u00E9', new byte[] { 0x05 });
            _charToByte.Add('\u00F9', new byte[] { 0x06 });
            _charToByte.Add('\u00EC', new byte[] { 0x07 });
            _charToByte.Add('\u00F2', new byte[] { 0x08 });
            _charToByte.Add('\u00C7', new byte[] { 0x09 });
            _charToByte.Add('\u000A', new byte[] { 0x0A });
            _charToByte.Add('\u00D8', new byte[] { 0x0B });
            _charToByte.Add('\u00F8', new byte[] { 0x0C });
            _charToByte.Add('\u000D', new byte[] { 0x0D });
            _charToByte.Add('\u00C5', new byte[] { 0x0E });
            _charToByte.Add('\u00E5', new byte[] { 0x0F });
            _charToByte.Add('\u0394', new byte[] { 0x10 });
            _charToByte.Add('\u005F', new byte[] { 0x11 });
            _charToByte.Add('\u03A6', new byte[] { 0x12 });
            _charToByte.Add('\u0393', new byte[] { 0x13 });
            _charToByte.Add('\u039B', new byte[] { 0x14 });
            _charToByte.Add('\u03A9', new byte[] { 0x15 });
            _charToByte.Add('\u03A0', new byte[] { 0x16 });
            _charToByte.Add('\u03A8', new byte[] { 0x17 });
            _charToByte.Add('\u03A3', new byte[] { 0x18 });
            _charToByte.Add('\u0398', new byte[] { 0x19 });
            _charToByte.Add('\u039E', new byte[] { 0x1A });
            //_charToByte.Add('\u001B', new byte[] { 0x1B }); // Should we convert Unicode escape to GSM?
            _charToByte.Add('\u00C6', new byte[] { 0x1C });
            _charToByte.Add('\u00E6', new byte[] { 0x1D });
            _charToByte.Add('\u00DF', new byte[] { 0x1E });
            _charToByte.Add('\u00C9', new byte[] { 0x1F });
            _charToByte.Add('\u0020', new byte[] { 0x20 });
            _charToByte.Add('\u0021', new byte[] { 0x21 });
            _charToByte.Add('\u0022', new byte[] { 0x22 });
            _charToByte.Add('\u0023', new byte[] { 0x23 });
            _charToByte.Add('\u00A4', new byte[] { 0x24 });
            _charToByte.Add('\u0025', new byte[] { 0x25 });
            _charToByte.Add('\u0026', new byte[] { 0x26 });
            _charToByte.Add('\u0027', new byte[] { 0x27 });
            _charToByte.Add('\u0028', new byte[] { 0x28 });
            _charToByte.Add('\u0029', new byte[] { 0x29 });
            _charToByte.Add('\u002A', new byte[] { 0x2A });
            _charToByte.Add('\u002B', new byte[] { 0x2B });
            _charToByte.Add('\u002C', new byte[] { 0x2C });
            _charToByte.Add('\u002D', new byte[] { 0x2D });
            _charToByte.Add('\u002E', new byte[] { 0x2E });
            _charToByte.Add('\u002F', new byte[] { 0x2F });
            _charToByte.Add('\u0030', new byte[] { 0x30 });
            _charToByte.Add('\u0031', new byte[] { 0x31 });
            _charToByte.Add('\u0032', new byte[] { 0x32 });
            _charToByte.Add('\u0033', new byte[] { 0x33 });
            _charToByte.Add('\u0034', new byte[] { 0x34 });
            _charToByte.Add('\u0035', new byte[] { 0x35 });
            _charToByte.Add('\u0036', new byte[] { 0x36 });
            _charToByte.Add('\u0037', new byte[] { 0x37 });
            _charToByte.Add('\u0038', new byte[] { 0x38 });
            _charToByte.Add('\u0039', new byte[] { 0x39 });
            _charToByte.Add('\u003A', new byte[] { 0x3A });
            _charToByte.Add('\u003B', new byte[] { 0x3B });
            _charToByte.Add('\u003C', new byte[] { 0x3C });
            _charToByte.Add('\u003D', new byte[] { 0x3D });
            _charToByte.Add('\u003E', new byte[] { 0x3E });
            _charToByte.Add('\u003F', new byte[] { 0x3F });
            _charToByte.Add('\u00A1', new byte[] { 0x40 });
            _charToByte.Add('\u0041', new byte[] { 0x41 });
            _charToByte.Add('\u0042', new byte[] { 0x42 });
            _charToByte.Add('\u0043', new byte[] { 0x43 });
            _charToByte.Add('\u0044', new byte[] { 0x44 });
            _charToByte.Add('\u0045', new byte[] { 0x45 });
            _charToByte.Add('\u0046', new byte[] { 0x46 });
            _charToByte.Add('\u0047', new byte[] { 0x47 });
            _charToByte.Add('\u0048', new byte[] { 0x48 });
            _charToByte.Add('\u0049', new byte[] { 0x49 });
            _charToByte.Add('\u004A', new byte[] { 0x4A });
            _charToByte.Add('\u004B', new byte[] { 0x4B });
            _charToByte.Add('\u004C', new byte[] { 0x4C });
            _charToByte.Add('\u004D', new byte[] { 0x4D });
            _charToByte.Add('\u004E', new byte[] { 0x4E });
            _charToByte.Add('\u004F', new byte[] { 0x4F });
            _charToByte.Add('\u0050', new byte[] { 0x50 });
            _charToByte.Add('\u0051', new byte[] { 0x51 });
            _charToByte.Add('\u0052', new byte[] { 0x52 });
            _charToByte.Add('\u0053', new byte[] { 0x53 });
            _charToByte.Add('\u0054', new byte[] { 0x54 });
            _charToByte.Add('\u0055', new byte[] { 0x55 });
            _charToByte.Add('\u0056', new byte[] { 0x56 });
            _charToByte.Add('\u0057', new byte[] { 0x57 });
            _charToByte.Add('\u0058', new byte[] { 0x58 });
            _charToByte.Add('\u0059', new byte[] { 0x59 });
            _charToByte.Add('\u005A', new byte[] { 0x5A });
            _charToByte.Add('\u00C4', new byte[] { 0x5B });
            _charToByte.Add('\u00D6', new byte[] { 0x5C });
            _charToByte.Add('\u00D1', new byte[] { 0x5D });
            _charToByte.Add('\u00DC', new byte[] { 0x5E });
            _charToByte.Add('\u00A7', new byte[] { 0x5F });
            _charToByte.Add('\u00BF', new byte[] { 0x60 });
            _charToByte.Add('\u0061', new byte[] { 0x61 });
            _charToByte.Add('\u0062', new byte[] { 0x62 });
            _charToByte.Add('\u0063', new byte[] { 0x63 });
            _charToByte.Add('\u0064', new byte[] { 0x64 });
            _charToByte.Add('\u0065', new byte[] { 0x65 });
            _charToByte.Add('\u0066', new byte[] { 0x66 });
            _charToByte.Add('\u0067', new byte[] { 0x67 });
            _charToByte.Add('\u0068', new byte[] { 0x68 });
            _charToByte.Add('\u0069', new byte[] { 0x69 });
            _charToByte.Add('\u006A', new byte[] { 0x6A });
            _charToByte.Add('\u006B', new byte[] { 0x6B });
            _charToByte.Add('\u006C', new byte[] { 0x6C });
            _charToByte.Add('\u006D', new byte[] { 0x6D });
            _charToByte.Add('\u006E', new byte[] { 0x6E });
            _charToByte.Add('\u006F', new byte[] { 0x6F });
            _charToByte.Add('\u0070', new byte[] { 0x70 });
            _charToByte.Add('\u0071', new byte[] { 0x71 });
            _charToByte.Add('\u0072', new byte[] { 0x72 });
            _charToByte.Add('\u0073', new byte[] { 0x73 });
            _charToByte.Add('\u0074', new byte[] { 0x74 });
            _charToByte.Add('\u0075', new byte[] { 0x75 });
            _charToByte.Add('\u0076', new byte[] { 0x76 });
            _charToByte.Add('\u0077', new byte[] { 0x77 });
            _charToByte.Add('\u0078', new byte[] { 0x78 });
            _charToByte.Add('\u0079', new byte[] { 0x79 });
            _charToByte.Add('\u007A', new byte[] { 0x7A });
            _charToByte.Add('\u00E4', new byte[] { 0x7B });
            _charToByte.Add('\u00F6', new byte[] { 0x7C });
            _charToByte.Add('\u00F1', new byte[] { 0x7D });
            _charToByte.Add('\u00FC', new byte[] { 0x7E });
            _charToByte.Add('\u00E0', new byte[] { 0x7F });
            // Extended GSM
            _charToByte.Add('\u20AC', new byte[] { 0x1B, 0x65 });
            _charToByte.Add('\u000C', new byte[] { 0x1B, 0x0A });
            _charToByte.Add('\u005B', new byte[] { 0x1B, 0x3C });
            _charToByte.Add('\u005C', new byte[] { 0x1B, 0x2F });
            _charToByte.Add('\u005D', new byte[] { 0x1B, 0x3E });
            _charToByte.Add('\u005E', new byte[] { 0x1B, 0x14 });
            _charToByte.Add('\u007B', new byte[] { 0x1B, 0x28 });
            _charToByte.Add('\u007C', new byte[] { 0x1B, 0x40 });
            _charToByte.Add('\u007D', new byte[] { 0x1B, 0x29 });
            _charToByte.Add('\u007E', new byte[] { 0x1B, 0x3D });

            foreach (KeyValuePair<char, byte[]> charToByte in _charToByte)
            {
                uint charByteVal = 0;
                if (charToByte.Value.Length == 1)
                    charByteVal = (uint)charToByte.Value[0];
                else if (charToByte.Value.Length == 2)
                    charByteVal = ((uint)charToByte.Value[0] * 255) + (uint)charToByte.Value[1];
                _byteToChar.Add(charByteVal, charToByte.Key);
            }
            _byteToChar.Add(0x1B1B, '\u0020'); // GSM char set says to map 1B1B to a space
        }
    }
}