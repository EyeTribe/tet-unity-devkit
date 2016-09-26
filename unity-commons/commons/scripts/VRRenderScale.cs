/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using EyeTribe.Unity;
using UnityEngine.VR;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Controls VR RenderScale from script. The higher value the better graphics. High value impact performance.
    /// </summary>
    public class VRRenderScale : MonoBehaviour
    {
        [SerializeField]
        private float _RenderScale = 1f;

        void Start()
        {
            if (VRMode.IsRunningInVRMode)
                VRSettings.renderScale = _RenderScale;
        }
    }
}
