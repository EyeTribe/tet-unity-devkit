/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Threading;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Base class for an arbitrary Unity script/component
    /// </summary>
    public class EyeTribeUnityScript : MonoBehaviour, IGazeListener
    {
        protected bool _UsingVR;    
    
        protected bool _FramesRecieved;

        public virtual void Awake()
        {
            //turn off VR if no VR device present
            _UsingVR = VRMode.IsRunningInVRMode();
        }

        public virtual void OnEnable()
        {
            //register for gaze updates
            GazeManager.Instance.AddGazeListener(this);
        }

        public virtual void OnDisable()
        {
            //deregister gaze updates
            GazeManager.Instance.RemoveGazeListener(this);
        }

        public virtual void OnGazeUpdate(GazeData gazeData)
        {
            if (!_FramesRecieved && GazeManager.Instance.IsActivated)
            { 
                _FramesRecieved = true;
                OnFirstGazeFrame();
            }
        }

        protected virtual void OnFirstGazeFrame()
        { 
        }
    }
}

