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

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public enum PriorityFlag : byte
    {
        /// <summary>
        /// Lowerst priority
        /// </summary>
        Level0 = 0,
        /// <summary>
        /// Level 1 priority
        /// </summary>
        Level1 = 1,
        /// <summary>
        /// Level 2 priority
        /// </summary>
        Level2 = 2,
        /// <summary>
        /// Level 3 priority
        /// </summary>
        Level3 = 3
    }
}
