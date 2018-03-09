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
using JamaaTech.Smpp.Net.Lib.Util;
using System.Diagnostics;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public abstract class SendSmPDU : SmPDU
    {
        private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Variables
        protected string vServiceType;
        protected EsmClass vEsmClass;
        protected RegisteredDelivery vRegisteredDelivery;
        protected DataCoding vDataCoding;
        //--
        private static TraceSwitch vTraceSwitch = new TraceSwitch("SendSmPDUSwitch", "SendSmPDU switch");
        #endregion

        #region Constructors
        internal SendSmPDU(PDUHeader header, SmppEncodingService smppEncodingService)
            : base(header, smppEncodingService)
        {
            vServiceType = "";
            vEsmClass = EsmClass.Default;
            vRegisteredDelivery = RegisteredDelivery.None;
            vDataCoding = DataCoding.ASCII;
        }
        #endregion

        #region Properties
        public string ServiceType
        {
            get { return vServiceType; }
            set { vServiceType = value; }
        }

        public EsmClass EsmClass
        {
            get { return vEsmClass; }
            set { vEsmClass = value; }
        }

        public RegisteredDelivery RegisteredDelivery
        {
            get { return vRegisteredDelivery; }
            set { vRegisteredDelivery = value; }
        }

        public DataCoding DataCoding
        {
            get { return vDataCoding; }
            set { vDataCoding = value; }
        }
        #endregion

        #region Methods
        public abstract byte[] GetMessageBytes();

        public abstract void SetMessageBytes(byte[] message);

        public string GetMessageText()
        {
            byte[] msgBytes = GetMessageBytes();
            if (msgBytes == null) { return null; }
            string message = null;
            Udh udh = null;
            GetMessageText(out message, out udh);
            return message;
        }

        public virtual void GetMessageText(out string message, out Udh udh)
        {
            message = null; udh = null;
            byte[] msgBytes = GetMessageBytes();
            if (msgBytes == null) { return; }
            ByteBuffer buffer = new ByteBuffer(msgBytes);
            //Check if the UDH is set in the esm_class field
            if ((EsmClass & EsmClass.UdhiIndicator) == EsmClass.UdhiIndicator) 
            {
                _Log.Info("200020:UDH field presense detected;");
                if (vTraceSwitch.TraceInfo) { Trace.WriteLine("200020:UDH field presense detected;"); }
                try { udh = Udh.Parse(buffer, vSmppEncodingService); }
                catch (Exception ex)
                {
                    _Log.ErrorFormat("20023:UDH field parsing error - {0}", ex, new ByteBuffer(msgBytes).DumpString());
                    if (vTraceSwitch.TraceError)
                    {
                        Trace.WriteLine(string.Format(
                            "20023:UDH field parsing error - {0} {1};",
                            new ByteBuffer(msgBytes).DumpString(), ex.Message));
                    }
                    throw;
                }
            }
            //Check if we have something remaining in the buffer
            if (buffer.Length == 0) { return; }
            try { message = vSmppEncodingService.GetStringFromBytes(buffer.ToBytes(), DataCoding); }
            catch (Exception ex1)
            {
                _Log.ErrorFormat("200019:SMS message decoding failure - {0}", ex1, new ByteBuffer(msgBytes).DumpString());
                if (vTraceSwitch.TraceError)
                {
                    Trace.WriteLine(string.Format(
                        "200019:SMS message decoding failure - {0} {1};",
                        new ByteBuffer(msgBytes).DumpString(), ex1.Message));
                }
                throw;
            }
        }

        public void SetMessageText(string message, DataCoding dataCoding)
        {
            SetMessageText(message, dataCoding, null);
        }

        public virtual void SetMessageText(string message, DataCoding dataCoding, Udh udh)
        {
            ByteBuffer buffer = new ByteBuffer(160);
            if (udh != null) { buffer.Append(udh.GetBytes()); }
            buffer.Append(vSmppEncodingService.GetBytesFromString(message, dataCoding));
            SetMessageBytes(buffer.ToBytes());
            if (udh != null) { EsmClass = EsmClass | EsmClass.UdhiIndicator; }
            DataCoding = dataCoding;
        }
        #endregion
    }
}
