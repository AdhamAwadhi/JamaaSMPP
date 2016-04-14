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
    public sealed class ByteBuffer
    {
        #region Variables
        private byte[] vArrayBuffer;
        private int vNextPosition;
        private int vLength;
        private int vCapacity;
        #endregion

        #region Constants
        private const int DEFAULT_CAPACITY = 64;
        private const int MIN_CAPACITY = 16;
        #endregion

        #region Constructors
        public ByteBuffer()
        {
            //Create array buffer with default capacity
            vArrayBuffer = new byte[DEFAULT_CAPACITY];
            vCapacity = DEFAULT_CAPACITY;
        }

        public ByteBuffer(int capacity)
        {
            //Create array buffer with specified capacity
            //If capacity is less than 32, use default size
            if (capacity < MIN_CAPACITY) { capacity = DEFAULT_CAPACITY; }
            vArrayBuffer = new byte[capacity];
            vCapacity = capacity;
        }

        public ByteBuffer(byte[] array)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            //Create array buffer with capacity equal to the array size
            //Or default capacity if the array size is less than 32
            vCapacity = array.Length < MIN_CAPACITY ? DEFAULT_CAPACITY : array.Length;
            vArrayBuffer = new byte[vCapacity];
            Append(array);
        }

        public ByteBuffer(byte[] array, int capacity)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            //Create array buffer with capacity equal to the specified size
            //If capacity is less than the array size or 32, use default size
            if (capacity < array.Length) { capacity = array.Length; }
            vCapacity = capacity < MIN_CAPACITY ? DEFAULT_CAPACITY : capacity;
            vArrayBuffer = new byte[vCapacity];
            Append(array);
        }

        public string DumpString()
        {
            StringBuilder builder = new StringBuilder();
            bool appendSpace = false;
            foreach (byte @byte in ToBytes())
            {
                if (appendSpace) { builder.Append(" "); }
                else { appendSpace = true; }
                builder.AppendFormat("{0:x}", @byte);
            }
            return builder.ToString();
        }
        #endregion

        #region Properties
        public int Capacity
        {
            get { return vCapacity; }
            set { vCapacity = value; }
        }

        public int Length
        {
            get { return vLength; }
        }

        private int Reserved
        {
            get { return vArrayBuffer.Length - vNextPosition; }
        }
        #endregion

        #region Methods
        #region Public Methods
        public void Append(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (buffer.Length <= 0) { return; }
            Append(buffer.vArrayBuffer, buffer.vNextPosition - buffer.vLength, buffer.vLength);
        }

        public void Append(byte[] bytes)
        {
            if (bytes == null) { throw new ArgumentNullException("bytes"); }
            Append(bytes, 0, bytes.Length);
        }

        public void Append(byte[] bytes, int start, int length)
        {
            if (bytes == null) { throw new ArgumentNullException("bytes"); }
            if (Reserved < length) { RealocateBuffer(length); }
            Array.Copy(bytes, start, vArrayBuffer, vNextPosition, length);
            vNextPosition += length;
            vLength += length;
        }

        public void Append(byte value)
        {
            if (Reserved < 1) { RealocateBuffer(1); }
            vArrayBuffer[vNextPosition] = value;
            vNextPosition++;
            vLength++;
        }

        public byte[] Peek(int count)
        {
            if (count > vLength) { throw new ArgumentException("count cannot be greater than buffer length", "count"); }
            if (count <= 0) { throw new ArgumentException("count must be less greater than or zero"); }
            byte[] result = new byte[count];
            Array.Copy(vArrayBuffer, vNextPosition - vLength, result, 0, count);
            return result;
        }

        public byte[] Remove(int count)
        {
            byte[] result = Peek(count);
            vLength -= count;
            return result;
        }

        public byte Remove()
        {
            if (vLength < 1) { throw new InvalidOperationException("Array must must not be emty"); }
            byte result = vArrayBuffer[vNextPosition - vLength];
            vLength--;
            return result;
        }

        public void Clear()
        {
            vNextPosition = 0;
            vLength = 0;
        }

        public ByteBuffer Clone()
        {
            byte[] newBuffer = new byte[vArrayBuffer.Length];
            if (Length > 0)
            {
                //Copy all bytes onto the new array
                Array.Copy(vArrayBuffer, vNextPosition - vLength, newBuffer, 0, vLength);
            }
            return new ByteBuffer(newBuffer, vCapacity);
        }

        public byte[] ToBytes()
        {
            if (Length <= 0) { return new byte[0]; }
            return Peek(Length);
        }

        public int Find(byte value)
        {
            return Find(value, 0, Length - 1);
        }

        public int Find(byte value, int startIndex, int endIndex)
        {
            if (Length <= 0) { return -1; }
            if (startIndex >= Length || startIndex < 0) { throw new ArgumentOutOfRangeException("startIndex"); }
            if (endIndex < startIndex) { throw new ArgumentException("startIndex cannot be greater than endIndex"); }
            if (endIndex >= Length) { throw new ArgumentOutOfRangeException("endIndex"); }
            startIndex = startIndex + vNextPosition - vLength;
            endIndex = endIndex + vNextPosition - vLength;
            for (; startIndex <= endIndex; startIndex++)
            {
                if (vArrayBuffer[startIndex] == value)
                { return startIndex + vLength - vNextPosition; }
            }
            return -1;
        }
        #endregion

        #region Helper Methods
        private void RealocateBuffer(int increment)
        {
            int newBufferSize = ((int)(increment / vCapacity) + 1) * vCapacity;
            //Hold the current array buffer
            byte[] currentBuffer = vArrayBuffer;
            //Allocate a new array buffer
            vArrayBuffer = new byte[vArrayBuffer.Length + newBufferSize];
            int startIndex = vNextPosition - vLength;
            int length = vLength;
            vNextPosition = 0;
            vLength = 0;
            Append(currentBuffer, startIndex, length);
        }
        #endregion

        #region Overriden System.Object Methods
        public override string ToString()
        {
            return vArrayBuffer.ToString();
        }
        #endregion
        #endregion
    }
}
