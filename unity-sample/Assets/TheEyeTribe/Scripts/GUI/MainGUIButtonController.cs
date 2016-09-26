/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using EyeTribe.Unity;
using EyeTribe.Unity.Calibration;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles GUI buttons if in 'Remote' gaze tracking mode. Disables buttons otherwise.
    /// </summary>
    public class MainGUIButtonController : MonoBehaviour
    {
        [SerializeField] private Button _ExitButton;
        [SerializeField] private Button _ToggleButton;

        void Awake()
        {
            if (null == _ExitButton)
                throw new Exception("_ExitButton is not set!");

            if (null == _ToggleButton)
                throw new Exception("_ToggleButton is not set!");

            _ExitButton.gameObject.SetRendererEnabled(false);
            _ExitButton.onClick.RemoveAllListeners();
            _ExitButton.onClick.AddListener(() => { LevelManager.Instance.LoadPreviousLevelOrExit(); });

            _ToggleButton.gameObject.SetRendererEnabled(false);
            _ToggleButton.onClick.RemoveAllListeners();
            _ToggleButton.onClick.AddListener(() => { GazeGUIController.ToggleIndicatorMode(); });

            _ExitButton.gameObject.SetActive(!VRMode.IsRunningInVRMode);
            _ToggleButton.gameObject.SetActive(!VRMode.IsRunningInVRMode);
        }

        void OnEnable() 
        {
            // Only use in 'Remote' gaze tracking mode
            if (VRMode.IsRunningInVRMode)
                Destroy(this.gameObject);

            CalibrationManager.OnCalibrationStateChange += CalibrationStateChange;
        }

        void OnDisable() 
        {
            CalibrationManager.OnCalibrationStateChange -= CalibrationStateChange;
        }

        private void CalibrationStateChange(bool isCalibrating)
        {
            if (!VRMode.IsRunningInVRMode)
            { 
                if (isCalibrating)
                {
                    _ExitButton.gameObject.SetActive(false);
                    _ToggleButton.gameObject.SetActive(false);
                }
                else if (!isCalibrating)
                {
                    _ExitButton.gameObject.SetActive(true);
                    _ToggleButton.gameObject.SetActive(true);
                }
            }
        }
    }
}
