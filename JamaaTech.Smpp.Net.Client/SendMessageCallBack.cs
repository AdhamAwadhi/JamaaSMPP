/************************************************************************
 * Copyright (C) 2008 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Client Library.
 *
 * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Client Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/


namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// A delegate used for asynchorous send message operations
    /// </summary>
    /// <param name="message">A message to send</param>
    /// <param name="timeout">A value indicating the time in miliseconds after which the send operation times out</param>
    internal delegate void SendMessageCallBack(ShortMessage message,int timeout);
}
