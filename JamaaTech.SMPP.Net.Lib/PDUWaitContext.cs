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
using System.Threading;

namespace JamaaTech.Smpp.Net.Lib
{
    internal class PDUWaitContext
    {
        #region Variables
        private uint vSequenceNumber;
        private AutoResetEvent vNotifyEvent;
        private int vTimeOut;
        private bool vTimedOut;
        #endregion

        #region Constructors
        public PDUWaitContext(uint sequenceNumber,int timeOut)
        {
            vSequenceNumber = sequenceNumber;
            vNotifyEvent = new AutoResetEvent(false);
            vTimeOut = timeOut;
        }
        #endregion

        #region Properties
        public uint SequenceNumber
        {
            get { return vSequenceNumber; }
        }

        public bool TimedOut
        {
            get { return vTimedOut; }
        }
        #endregion

        #region Methods
        public bool WaitForAlert()
        {
            vTimedOut = !vNotifyEvent.WaitOne(vTimeOut,false);
            return !vTimedOut;
        }

        public void AlertResponseReceived()
        {
            vNotifyEvent.Set();
        }
        #endregion
    }
}
