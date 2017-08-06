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
using System.Text;
using System.Collections.Generic;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Lib
{
    public class Udh
    {
        #region Variables
        private int vSegmentId;
        private int vMessageCount;
        private int vMessageSequence;

        private static object vSyncRoot;
        #endregion

        #region Constructors
        static Udh()
        {
            vSyncRoot = new object();
        }

        public Udh(int segmentId, int messageCount, int messageSequence)
        {
            vSegmentId = segmentId;
            vMessageCount = messageCount;
            vMessageSequence = messageSequence;
        }

        public Udh(int segmentid, int messageCount)
        {
            vSegmentId = segmentid;
            vMessageCount = messageCount;
        }
        #endregion

        #region Properties
        public int SegmentID
        {
            get { return vSegmentId; }
        }

        public int MessageCount
        {
            get { return vMessageCount; }
            set { vMessageCount = value; }
        }

        public int MessageSequence
        {
            get { return vMessageSequence; }
            set { vMessageSequence = value; }
        }
        #endregion

        #region Methods
        public static Udh Parse(ByteBuffer buffer, SmppEncodingService smppEncodingService)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            //There must be at least 3 bytes for UDHL, IEI, IEDL
            if (buffer.Length < 3) { throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Invalid UDH field"); }
            int length = buffer.Remove(); //UDH Length
            int iei = buffer.Remove(); //Information element identifier
            int ieidl = buffer.Remove(); //Information element identifier data length
            /*
             * This udh implementation supports only concatenated messages with 
             * 8 bits (IEI = 0) and 16 bits (IEI = 8) reference number.
             * Therefore, the expected number of bytes indicated by the UDHL field
             * should be either 5 or 6 octects, otherwise the udh is unsupported.
             */
            int segId = 0;
            int count = 0;
            int seq = 0;
            //--
            //Confirm that we have enough bytes as indicated by the UDHL
            if (buffer.Length < ieidl) { throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Invalid UDH field"); }
            if (length == 5 && iei == 0 && ieidl == 3) //8 bits message reference
            {
                segId = buffer.Remove();
                count = buffer.Remove();
                seq = buffer.Remove();
            }
            else if (length == 6 && iei == 8 && ieidl == 4) //16 bits message reference
            {
                segId = smppEncodingService.GetShortFromBytes(buffer.Remove(2));
                count = buffer.Remove();
                seq = buffer.Remove();
            }
            else { throw new SmppException(SmppErrorCode.ESME_RUNKNOWNERR, "Invalid or unsupported UDH field"); }
            Udh udh = new Udh(segId, count, seq);
            return udh;
        }

        public byte[] GetBytes()
        {
            ByteBuffer buffer = new ByteBuffer(5);
            buffer.Append(0x05); //User 8 bits reference number
            buffer.Append(0x00); //IEI = 0 concatenated message
            buffer.Append(0x03); //Three bytes follow
            buffer.Append((byte)vSegmentId);
            buffer.Append((byte)vMessageCount);
            buffer.Append((byte)vMessageSequence);
            return buffer.ToBytes();
        }

        #endregion
    }
}
