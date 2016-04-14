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
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Lib.Util
{
    public abstract class PduProcessor<T> : RunningComponent where T : PDU
    {
        #region Variables
        private Queue<T> vPduQueue;
        private ManualResetEvent vWaitEvent;
        #endregion

        #region Constants
        private const int DEFAULT_CAPACITY = 256;
        #endregion

        #region Constructors
        public PduProcessor()
        {
            InitializeInstance(DEFAULT_CAPACITY);
        }

        public PduProcessor(int defaultQueueCapacity)
        {
            InitializeInstance(defaultQueueCapacity);
        }
        #endregion

        #region Methods
        #region Helper Methods
        private void InitializeInstance(int queueCapacity)
        {
            vPduQueue = new Queue<T>(queueCapacity);
            vWaitEvent = new ManualResetEvent(false); //The state is unsignaled initially
        }

        private T WaitPdu()
        {
            vWaitEvent.WaitOne();
            lock (vPduQueue)
            {
                T pdu = vPduQueue.Dequeue();
                if (vPduQueue.Count == 0) { vWaitEvent.Reset(); }
                return pdu;
            }
        }
        #endregion

        #region Interface Methods
        protected abstract void PostProcessPdu(T pdu);

        internal void ProcessPdu(T pdu)
        {
            lock (vPduQueue)
            {
                if (!Running) { return; }
                vPduQueue.Enqueue(pdu);
                vWaitEvent.Set();
            }
        }

        protected override void RunNow()
        {
            while (true) { T pdu = WaitPdu(); PostProcessPdu(pdu); }
        }
        #endregion
        #endregion

    }
}
