/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System;
using System.Collections;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk.Utils;
using System.Collections.Generic;

namespace EyeTribe.Unity
{
    public class UnityCalibUtils : CalibUtils
    {
        public static string GetCalibString(CalibrationResult result)
        {
            int rating = CalibUtils.GetCalibRating(result);

            switch (rating)
            {
                case 1:
                    return "POOR";
                case 2:
                    return "MODERATE";
                case 3:
                    return "GOOD";
                case 4:
                    return "PERFECT";
                default:
                    return "na";
            }
        }
    }
}

