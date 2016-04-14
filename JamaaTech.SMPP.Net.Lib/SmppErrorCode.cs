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

namespace JamaaTech.Smpp.Net.Lib
{
    public enum SmppErrorCode : uint
    {
        /// <summary>
        /// No error
        /// </summary>
        ESME_ROK = 0x00000000,
        /// <summary>
        /// Message length is invalid
        /// </summary>
        ESME_RINVMSGLEN = 0x00000001,
        /// <summary>
        /// Command length is invalid
        /// </summary>
        ESME_RINVCMDLEN = 0x00000002,
        /// <summary>
        /// Invalid command ID
        /// </summary>
        ESME_RINVCMDID = 0x00000003,
        /// <summary>
        /// Incorrect bind status for given command
        /// </summary>
        ESME_RINVBNDSTS = 0x00000004,
        /// <summary>
        /// ESME already in bound state
        /// </summary>
        ESME_RALYBND = 0x00000005,
        /// <summary>
        /// Invalid priority flag
        /// </summary>
        ESME_RINVPRTFLG = 0x00000006,
        /// <summary>
        /// Invalid registred delivery flag
        /// </summary>
        ESME_RINVREGDLVFLG = 0x00000007,
        /// <summary>
        /// System error
        /// </summary>
        ESME_RSYSERR = 0x00000008,
        /// <summary>
        /// Invalid source address
        /// </summary>
        ESME_RINVSRCADR = 0x0000000A,
        /// <summary>
        /// Invalid destination address
        /// </summary>
        ESME_RINVDSTADR = 0x0000000B,
        /// <summary>
        /// Message ID is invalid
        /// </summary>
        ESME_RINVMSGID = 0x0000000C,
        /// <summary>
        /// Bind failed
        /// </summary>
        ESME_RBINDFAIL = 0x0000000D,
        /// <summary>
        /// Invlaid password
        /// </summary>
        ESME_RINVPASWD = 0x0000000E,
        /// <summary>
        /// Invalid system ID
        /// </summary>
        ESME_RINVSYSID = 0x0000000F,
        /// <summary>
        /// Cancel SM failed
        /// </summary>
        ESME_RCANCELFAIL = 0x00000011,
        /// <summary>
        /// Replace SM failed
        /// </summary>
        ESME_RREPLACEFAIL = 0x00000013,
        /// <summary>
        /// Message queue full
        /// </summary>
        ESME_RMSGQFUL = 0x00000014,
        /// <summary>
        /// Invalid service type
        /// </summary>
        ESME_RINVSERTYP = 0x00000015,
        /// <summary>
        /// Invalid number of destinations
        /// </summary>
        ESME_RINVNUMDESTS = 0x00000033,
        /// <summary>
        /// Invalid distribution list name
        /// </summary>
        ESME_RINVDLNAME = 0x00000034,
        /// <summary>
        /// Destination flag is invalid
        /// </summary>
        ESME_RINVDESTFLAG = 0x00000040,
        /// <summary>
        /// Invalid submit with replace request
        /// </summary>
        ESME_RINVSUBREP = 0x00000042,
        /// <summary>
        /// Invalid esm_class field data
        /// </summary>
        ESME_RINVESMCLASS = 0x00000043,
        /// <summary>
        /// Cannot submit to distribution list
        /// </summary>
        ESME_RCNTSUBDL = 0x00000044,
        /// <summary>
        /// submit_sm or submit_multi failed
        /// </summary>
        ESME_RSUBMITFAIL = 0x00000045,
        /// <summary>
        /// Invalid source address TON
        /// </summary>
        ESME_RINVSRCTON = 0x00000048,
        /// <summary>
        /// Invalid source address NPI
        /// </summary>
        ESME_RINVSRCNPI = 0x00000049,
        /// <summary>
        /// Invalid destionation address TON
        /// </summary>
        ESME_RINVDSTTON = 0x00000050,
        /// <summary>
        /// Invalid destination address NPI
        /// </summary>
        ESME_RINVDSTNPI = 0x00000051,
        /// <summary>
        /// Invalid system type field
        /// </summary>
        ESME_RINVSYSTYP = 0x00000053,
        /// <summary>
        /// Invalid replace if present flag
        /// </summary>
        ESME_RINVREPFLAG = 0x00000054,
        /// <summary>
        /// Invlalid number of messages
        /// </summary>
        ESME_RINVNUMMSGS = 0x00000055,
        /// <summary>
        /// Throttling error. ESME has exceeded allowed message limits
        /// </summary>
        ESME_RTHROTTLED = 0x00000058,
        /// <summary>
        /// Invalid schedule delivery time
        /// </summary>
        ESME_RINVSCHED = 0x00000061,
        /// <summary>
        /// Invalid message validity period
        /// </summary>
        ESME_RINVEXPIRY = 0x00000062,
        /// <summary>
        /// Predefined message invalid or not found
        /// </summary>
        ESME_RINVDFTMSGID = 0x00000063,
        /// <summary>
        /// ESME receiver temporary application error
        /// </summary>
        ESME_RX_T_APPN = 0x00000064,
        /// <summary>
        /// ESME receiver permanent application error
        /// </summary>
        ESME_RX_P_APPN = 0x00000065,
        /// <summary>
        /// ESME receiver reject message error
        /// </summary>
        ESME_RX_R_APPN = 0x00000066,
        /// <summary>
        /// query_sm failed
        /// </summary>
        ESME_RQUERYFAIL = 0x00000067,
        /// <summary>
        /// Error in the optional part of the pdu body
        /// </summary>
        ESME_RINVOPTPARSTREAM = 0x000000C0,
        /// <summary>
        /// Optional parameter not allowed
        /// </summary>
        ESME_ROPTPARNOTALLWD = 0x000000C1,
        /// <summary>
        /// Invalid parameter length
        /// </summary>
        ESME_RINVPARLEN = 0x000000C2,
        /// <summary>
        /// Expected optinal parameter missing
        /// </summary>
        ESME_RMISSINGOPTPARAM = 0x000000C3,
        /// <summary>
        /// Invalid optional parameter value
        /// </summary>
        ESME_RINVOPTPARAMVAL = 0x000000C4,
        /// <summary>
        /// Delivery failure
        /// </summary>
        ESME_RDELIVERYFAILURE = 0x000000FE,
        /// <summary>
        /// Unknown error
        /// </summary>
        ESME_RUNKNOWNERR = 0x000000FF
    }
}
