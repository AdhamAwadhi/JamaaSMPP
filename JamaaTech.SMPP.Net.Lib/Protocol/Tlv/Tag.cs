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

namespace JamaaTech.Smpp.Net.Lib.Protocol.Tlv
{
    public enum Tag : ushort
    {
        dest_addr_subunit = 0x0005,
        dest_network_type = 0x0006,
        dest_bearer_type = 0x0007,
        dest_telematics_id = 0x0008,
        source_addr_subunit = 0x000D,
        source_network_type = 0x000E,
        source_bearer_type = 0x000F,
        source_telematics_id = 0x0010,
        qos_time_to_live = 0x0017,
        payload_type = 0x0019,
        additional_status_info_text = 0x001D,
        receipted_message_id = 0x001E,
        ms_msg_wait_facilities = 0x0030,
        privacy_indicator = 0x0201,
        source_subaddress = 0x0202,
        dest_subaddress = 0x0203,
        user_message_reference = 0x0204,
        user_response_code = 0x0205,
        source_port = 0x020A,
        destination_port = 0x020B,
        sar_msg_ref_num = 0x020C,
        language_indicator = 0x020D,
        sar_total_segments = 0x020E,
        sar_segment_seqnum = 0x020F,
        SC_interface_version = 0x0210,
        callback_num_pres_ind = 0x0302,
        callback_num_atag = 0x0303,
        number_of_messages = 0x0304,
        callback_num = 0x0381,
        dpf_result = 0x0420,
        set_dpf = 0x0421,
        ms_availability_status = 0x0422,
        network_error_code = 0x0423,
        message_payload = 0x0424,
        delivery_failure_reason = 0x0425,
        more_messages_to_send = 0x0426,
        message_state = 0x0427,
        ussd_service_op = 0x0501,
        display_time = 0x1201,
        sms_signal = 0x1203,
        ms_validity = 0x1204,
        alert_on_message_delivery = 0x130C,
        its_reply_type = 0x1380,
        its_session_info = 0x1383
    }
}
