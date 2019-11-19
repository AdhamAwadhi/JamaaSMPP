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
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;
using JamaaTech.Smpp.Net.Lib.Util;
using JamaaTech.Smpp.Net.Lib;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public abstract class PDU
    {
        #region Varibles
        protected PDUHeader vHeader;
        protected TlvCollection vTlv;
        protected SmppEncodingService vSmppEncodingService;
        #endregion

        #region Constructors
        internal PDU(PDUHeader header, SmppEncodingService smppEncodingService)
        {
            vHeader = header;
            vSmppEncodingService = smppEncodingService;
            vTlv = new TlvCollection();
        }
        #endregion

        #region Properties
        public PDUHeader Header
        {
            get { return vHeader; }
        }

        public TlvCollection Tlv
        {
            get { return vTlv; }
        }       

        public abstract SmppEntityType AllowedSource { get; }

        public abstract SmppSessionState AllowedSession { get; }

        #endregion

        #region Methods
        #region Interface Methods
        public static GenericNack GenericNack(PDUHeader header, SmppErrorCode errorCode, SmppEncodingService smppEncodingService)
        {
            if (header == null) { throw new ArgumentNullException("header"); }
            GenericNack gNack = (GenericNack)CreatePDU(header, smppEncodingService);
            gNack.Header.ErrorCode = errorCode;
            return gNack;
        }

        public virtual GenericNack GenericNack(SmppErrorCode errorCode)
        {
            PDUHeader header = new PDUHeader(CommandType.GenericNack, vHeader.SequenceNumber);
            header.ErrorCode = errorCode;
            GenericNack gNack = (GenericNack)CreatePDU(header, vSmppEncodingService);
            return gNack;
        }

        public virtual byte[] GetBytes()
        {
            byte[] bodyData = GetBodyData();
            byte[] tlvData = vTlv.GetBytes(vSmppEncodingService);
            int length = 16;
            length += bodyData == null ? 0 : bodyData.Length;
            length += tlvData == null ? 0 : tlvData.Length;
            vHeader.CommandLength = (uint)length;
            ByteBuffer buffer = new ByteBuffer(length); //Allocate buffer with enough capacity
            buffer.Append(vHeader.GetBytes(vSmppEncodingService));
            if (bodyData != null) { buffer.Append(bodyData); }
            if (tlvData != null) { buffer.Append(tlvData); }
            return buffer.ToBytes();
        }

        protected abstract byte[] GetBodyData();

        protected abstract void Parse(ByteBuffer buffer);

        public void SetBodyData(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            try { Parse(buffer); }
            catch (PDUException) { throw; }
            catch (Exception ex) { PDUParseException.WrapAndThrow(ex); }
        }

        public static PDU CreatePDU(PDUHeader header, SmppEncodingService smppEncodingService)
        {
            if (header == null) { throw new ArgumentNullException("header"); }
            switch (header.CommandType)
            {
                case CommandType.BindReceiver:
                    return new BindReceiver(header, smppEncodingService);
                //--
                case CommandType.BindTransceiver:
                    return new BindTransceiver(header, smppEncodingService);
                //--
                case CommandType.BindTransmitter:
                    return new BindTransmitter(header, smppEncodingService);
                //--
                case CommandType.BindTransmitterResp:
                    return new BindTransmitterResp(header, smppEncodingService);
                //--
                case CommandType.BindTransceiverResp:
                    return new BindTransceiverResp(header, smppEncodingService);
                //--
                case CommandType.BindReceiverResp:
                    return new BindReceiverResp(header, smppEncodingService);
                //--
                case CommandType.OutBind:
                    return new Outbind(header, smppEncodingService);
                //--
                case CommandType.EnquireLink:
                    return new EnquireLink(header, smppEncodingService);
                //--
                case CommandType.EnquireLinkResp:
                    return new EnquireLinkResp(header, smppEncodingService);
                //--
                case CommandType.UnBind:
                    return new Unbind(header, smppEncodingService);
                //--
                case CommandType.UnBindResp:
                    return new UnbindResp(header, smppEncodingService);
                //--
                case CommandType.GenericNack:
                    return new GenericNack(header, smppEncodingService);
                //--
                case CommandType.SubmitSm:
                    return new SubmitSm(header, smppEncodingService);
                //--
                case CommandType.SubmitSmResp:
                    return new SubmitSmResp(header, smppEncodingService);
                //--
                case CommandType.DataSm:
                    return new DataSm(header, smppEncodingService);
                //--
                case CommandType.DataSmResp:
                    return new DataSmResp(header, smppEncodingService);
                //--
                case CommandType.DeliverSm:
                    return new DeliverSm(header, smppEncodingService);
                //--
                case CommandType.DeliverSmResp:
                    return new DeliverSmResp(header, smppEncodingService);
                //--
                case CommandType.CancelSm:
                    return new CancelSm(header, smppEncodingService);
                //--
                case CommandType.CancelSmResp:
                    return new CancelSmResp(header, smppEncodingService);
                //--
                case CommandType.ReplaceSm:
                    return new ReplaceSm(header, smppEncodingService);
                //--
                case CommandType.ReplaceSmResp:
                    return new ReplaceSmResp(header, smppEncodingService);
                //--
                case CommandType.QuerySm:
                    return new QuerySm(header, smppEncodingService);
                //--
                case CommandType.QuerySmResp:
                    return new QuerySmResp(header, smppEncodingService);
                //--
                default:
                    throw new InvalidPDUCommandException();
            }
        }
        #endregion

        #region Helper Methods
        internal static string DecodeCString(ByteBuffer buffer, SmppEncodingService smppEncodingService)
        {
            //Get next terminating null value
            int pos = buffer.Find(0x00);
            if (pos < 0) { throw new PDUFormatException("CString type field could not be read. The terminating charactor is missing"); }
            try { string value = smppEncodingService.GetCStringFromBytes(buffer.Remove(pos + 1)); return value; }
            catch (ArgumentException ex)
            {
                //ByteBuffer.Remove(int count) throw ArgumentException if the buffer length is less than count
                //This is the indication that the amount of bytes required could not be met
                //We wrap this exception as NotEnoughtBytesException exception
                throw new NotEnoughBytesException("PDU requires more bytes than supplied", ex);
            }
        }

        internal static byte[] EncodeCString(string str, SmppEncodingService smppEncodingService)
        {
            if (str == null) { str = ""; }
            return smppEncodingService.GetBytesFromCString(str);
        }

        internal static string DecodeString(ByteBuffer buffer, int length, SmppEncodingService smppEncodingService)
        {
            try { string value = smppEncodingService.GetStringFromBytes(buffer.Remove(length)); return value; }
            catch (ArgumentException ex)
            {
                //ByteBuffer.Remove(int count) throw ArgumentException if the buffer length is less than count
                //This is the indication that the amount of bytes required could not be met
                //We wrap this exception as NotEnoughtBytesException exception
                throw new NotEnoughBytesException(
                    "Octet String field could not be decoded because no enough bytes are evailable in the buffer",
                    ex);
            }
        }

        internal static byte GetByte(ByteBuffer buffer)
        {
            try { return buffer.Remove(); }
            catch (InvalidOperationException)
            {
                //ByteBuffer.Remove() throws invalid operation exception if the buffer is empty
                //We rethrow this error as a NotEnoughBytesException exception
                throw new NotEnoughBytesException("A byte field could not be read because the buffer is empty");
            }
        }

        internal static byte[] EncodeString(string str, SmppEncodingService smppEncodingService)
        {
            if (str == null) { str = ""; }
            return smppEncodingService.GetBytesFromString(str);
        }
        #endregion

        #region Overriden System.Object Members
        public override string ToString()
        {
            return vHeader.ToString();
        }
        #endregion

        #region TLV collection methods

        /// <summary>
        /// Gets the optional parameter string associated with
        /// the given tag.
        /// </summary>
        /// <param name="tag">The tag in TLV.</param>
        /// <returns>The optional parameter string, or null if not found.</returns>
        public string GetOptionalParamString(Tag tag)
        {
            var tlv = vTlv.GetTlvByTag(tag);
            if (tlv == null) return null;

            return vSmppEncodingService.GetStringFromBytes(tlv.RawValue);
        }

        /// <summary>
        /// Gets the optional parameter string (with null terminated) associated with
        /// the given tag.
        /// </summary>
        /// <param name="tag">The tag in TLV.</param>
        /// <returns>The optional parameter string, or null if not found.</returns>
        public string GetOptionalParamCString(Tag tag)
        {
            var tlv = vTlv.GetTlvByTag(tag);
            if (tlv == null) return null;

            return vSmppEncodingService.GetStringFromBytes(tlv.RawValue);
        }

        /// <summary>
        /// Gets the optional parameter bytes associated with
        /// the given tag.
        /// </summary>
        /// <param name="tag">The tag in TLV.</param>
        /// <returns>The optional parameter bytes, or null if not found</returns>
        public byte[] GetOptionalParamBytes(Tag tag)
        {
            var tlv = vTlv.GetTlvByTag(tag);
            if (tlv == null) return null;

            return tlv.RawValue;
        }
        /// <summary>
        /// Gets the optional parameter of type T associated with
        /// the given tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The optional parameter value, or null if not found</returns>
        public byte? GetOptionalParamByte(Tag tag)
        {
            return this.GetOptionalParamByte<byte>(tag);
        }
        /// <summary>
        /// Gets the optional parameter of type T associated with
        /// the given tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The optional parameter value, or null if not found</returns>
        public T? GetOptionalParamByte<T>(Tag tag)
            where T : struct
        {
            var tlv = vTlv.GetTlvByTag(tag);
            if (tlv == null) return null;

            var data = tlv.RawValue[0];

            return typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), data) : (T)Convert.ChangeType(data, typeof(T));
        }

        /// <summary>
        /// Sets the given TLV(as a string)into the table.
        /// This will reverse the byte order in the tag for you (necessary for encoding).
        /// If the value is null, the parameter TLV will be removed instead.
        /// </summary>
        /// <param name="tag">The tag for this TLV.</param>
        /// <param name="val">The value of this TLV.</param>
        public void SetOptionalParamString(Tag tag, string val, bool nullTerminated = false)
        {
            if (val == null)
            {
                this.RemoveOptionalParameter(tag);
            }
            else
            {
                var bytes = vSmppEncodingService.GetBytesFromCString(val, nullTerminated);

                SetOptionalParamBytes(tag, bytes);
            }
        }

        /// <summary>
        /// Sets the given TLV(as a byte) into the table.  This will not take
        /// care of big-endian/little-endian issues, although it will reverse the byte order 
        /// in the tag for you (necessary for encoding). 
        /// If the value is null, the parameter TLV will be removed instead.
        /// </summary>
        /// <param name="tag">The tag for this TLV.</param>
        /// <param name="val">The value of this TLV.</param>
        public void SetOptionalParamByte<T>(Tag tag, T? val)
            where T : struct
        {
            if (!val.HasValue)
            {
                SetOptionalParamBytes(tag, null);
            }
            else
            {
                SetOptionalParamBytes(tag, new[] { Convert.ToByte(val.Value) });
            }
        }
        /// <summary>
        /// Sets the given TLV(as a byte array)into the table.  This will not take
        /// care of big-endian/little-endian issues, although it will reverse the byte order 
        /// in the tag for you(necessary for encoding).
        /// If the value is null, the parameter TLV will be removed instead.
        /// </summary>
        /// <param name="tag">The tag for this TLV.</param>
        /// <param name="val">The value of this TLV.</param>
        public void SetOptionalParamBytes(Tag tag, byte[] val)
        {
            if (val == null)
            {
                this.RemoveOptionalParameter(tag);
            }
            else
            {
                vTlv.Add(new Tlv.Tlv(tag, (ushort)val.Length, val));
            }
        }

        /// <summary>
        /// Removes the optional parameter.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public void RemoveOptionalParameter(Tag tag)
        {
            var tlv = vTlv.GetTlvByTag(tag);
            if (tlv == null) return;

            vTlv.Remove(tlv);
        }
        #endregion TLV collection methods
        #endregion
    }
}
