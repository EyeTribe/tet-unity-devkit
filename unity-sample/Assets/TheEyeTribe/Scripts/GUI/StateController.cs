/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk;
using System;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles the logic associated to displaying notifications when the EyeTribe DevKit
    /// changes state.
    /// </summary>
    public class StateController : MonoBehaviour
    {
        [SerializeField]private StateNotifyer _StateNotifyer;

        private void Awake()
        {
            if (null == _StateNotifyer)
                throw new Exception("_StateNotifyer is not set!");
        }

        void OnEnable() 
        {
            EyeTribeSDK.OnConnectionStateChange += OnConnectionStateChange;
            EyeTribeSDK.OnCalibrationResult += OnCalibrationChange;
            EyeTribeSDK.OnTrackerStateChange += OnTrackerStateChange;
        }

        void OnDisable()
        {
            EyeTribeSDK.OnConnectionStateChange -= OnConnectionStateChange;
            EyeTribeSDK.OnCalibrationResult -= OnCalibrationChange;
            EyeTribeSDK.OnTrackerStateChange -= OnTrackerStateChange;
        }

        public void OnConnectionStateChange(bool isConnected)
        {
            if (isConnected)
            {
                //_StateNotifyer.ShowState("Connected to EyeTribe Server");
            }
            else
                _StateNotifyer.ShowState("Disconnected from EyeTribe Server");
        }

        public void OnCalibrationChange(bool isCalibrated, CalibrationResult calibResult)
        {
            /*
            if (isCalibrated)
                _StateNotifyer.ShowState("Calib Result: " + calibResult.AverageErrorDegree.ToString("0.00"));
             */
        }

        public void OnTrackerStateChange(GazeManager.TrackerState trackerState)
        {
            switch(trackerState)
            {
                case GazeManager.TrackerState.TRACKER_NOT_CONNECTED:
                    _StateNotifyer.ShowState("Tracker Disconnected");
                    break;
                case GazeManager.TrackerState.TRACKER_CONNECTED_BADFW:
                    _StateNotifyer.ShowState("Tracker Connected - Bad Firmware Detected");
                    break;
                case GazeManager.TrackerState.TRACKER_CONNECTED_NOUSB3:
                    _StateNotifyer.ShowState("Tracker Connected - No USB3 Detected");
                    break;
                case GazeManager.TrackerState.TRACKER_CONNECTED_NOSTREAM:
                    _StateNotifyer.ShowState("Tracker Connected - No Image Stream Detected");
                    break;
            }
        }
    }
}
