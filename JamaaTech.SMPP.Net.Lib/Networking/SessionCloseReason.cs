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

namespace JamaaTech.Smpp.Net.Lib.Networking
{
    public enum SessionCloseReason : int
    {
        /// <summary>
        /// A TcpIpSession was closed due to a call on <see cref="TcpIpSession.EndSession()"/>
        /// </summary>
        EndSessionCalled = 1,
        /// <summary>
        /// An exception was thrown which forced the session to be terminated unexpectedly
        /// </summary>
        ExceptionThrown = 2,
        /// <summary>
        /// Existing connection was closed by remote host
        /// </summary>
        SocketShutdown = 3
    }
}
