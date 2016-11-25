/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
using UnityEngine;
using EyeTribe.Unity;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Aligns GO with height of camera
    /// </summary>
    public class SwitchCanvasRenderMode : MonoBehaviour
    {
        [SerializeField] private Canvas _Canvas;
        [SerializeField] private RenderMode _CanvasRenderMode;
        [SerializeField] private Camera _Camera;
        [SerializeField] private bool _VrMode;
        [SerializeField] private bool _RemoteMode;

        void Awake()
        {
            if (null == _Canvas)
                throw new Exception("_Canvas is not set!");
        }

        void OnEnable()
        {
            if (_VrMode && VRMode.IsRunningInVRMode)
            {
                if (_CanvasRenderMode == RenderMode.ScreenSpaceCamera)
                {
                    if (null == _Camera)
                        throw new Exception("_Camera is not set!");
                    _Canvas.worldCamera = _Camera;
                }

                _Canvas.renderMode = _CanvasRenderMode;
            }

            if (_RemoteMode && !VRMode.IsRunningInVRMode)
            {
                if (_CanvasRenderMode == RenderMode.ScreenSpaceCamera)
                {
                    if (null == _Camera)
                        throw new Exception("_Camera is not set!");
                    _Canvas.worldCamera = _Camera;
                }

                _Canvas.renderMode = _CanvasRenderMode;
            }
        }
    }
}