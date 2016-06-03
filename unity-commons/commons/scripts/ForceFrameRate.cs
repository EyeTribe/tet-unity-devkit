/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Utility class for forcing a desired framerate in Unity
    /// </summary>
    public class ForceFrameRate : MonoBehaviour
    {

        public int ForcedFrameRate;

        public void Awake()
        {
            Application.targetFrameRate = ForcedFrameRate;
        }
    }
}
