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
using VRStandardAssets.Utils;
using System;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk;

namespace EyeTribe.Unity 
{
    /// <summary>
    /// Handles the input logic associated with GUI controls. Fires events when states change.
    /// </summary>
    public class GazeGUIController : MonoBehaviour
    {
        public static event Action<bool> OnSmoothModeToggle;
        public static event Action<bool> OnDebugModeToggle;
        public static event Action<bool> OnIndicatorModeToggle;

        // We expose these to be able to set initial values of static members in editor
        [SerializeField] private bool InitialUseSmoothed;
        [SerializeField] private bool InitialShowDebug;
        [SerializeField] private bool InitialShowGazeIndicator;
        [SerializeField] private bool InitialBlockWhenCalibrating = true;

        private static bool _UseSmoothed;
        private static bool _ShowDebug;
        private static bool _ShowGazeIndicator;
        private static bool _BlockWhenCalibrating;

        public static bool UseSmoothed { get { return _UseSmoothed; } }
        public static bool ShowDebug { get { return _ShowDebug; } }
        public static bool ShowGazeIndicator { get { return _ShowGazeIndicator; } }
        public static bool BlockWhenCalibrating { get { return _BlockWhenCalibrating; } }

        void Awake() 
        {
            _UseSmoothed = InitialUseSmoothed;
            _ShowDebug = InitialShowDebug;
            _ShowGazeIndicator = InitialShowGazeIndicator;
            _BlockWhenCalibrating = InitialBlockWhenCalibrating;
        }     

        void OnEnable()
        {
            if (null != OnSmoothModeToggle)
                OnSmoothModeToggle(_UseSmoothed);
            if (null != OnDebugModeToggle)
                OnDebugModeToggle(_ShowDebug);
            if (null != OnIndicatorModeToggle)
                OnIndicatorModeToggle(_ShowGazeIndicator);

            VRInput.OnDoubleClick += OnDoubleClick;
        }

        void OnDisable() 
        {
            VRInput.OnDoubleClick -= OnDoubleClick;
        }
                
        void Update()
        {
            if (!Application.isPlaying)
                return;

            HandleInput();
        }

        private void HandleInput()
        {
            // detect keyboard 'esc' or Android 'back'
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LevelManager.Instance.LoadPreviousLevelOrExit();
            }

            // We block input if calibration is ongoing
            if(!BlockWhenCalibrating ||!GazeManager.Instance.IsCalibrating)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || 
                    Input.GetButtonDown("Fire3")
                    )
                {
                    Debug.Log("Toggle");

                    ToggleIndicatorMode();
                }
                else
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    ToggleSmoothMode();
                }

                if (Input.GetKeyDown(KeyCode.Alpha3) ||
                    Input.GetButtonDown("Jump")
                    )
                {
                    ToggleDebugMode();
                }
            }
        }

        private void OnDoubleClick()
        {
            // detect only if in GearVR mode
            if (VRMode.IsOculusSDKGearVR)
                ToggleDebugMode();
        }

        protected void OnSwipe(VRInput.SwipeDirection swipe)
        {
            // detect only if in GearVR mode
            if (VRMode.IsOculusSDKGearVR)
            {
                if (gameObject.activeInHierarchy)
                {
                    if (VRInput.SwipeDirection.LEFT == swipe)
                    {
                        ToggleIndicatorMode();
                    }
                }
            }
        }

        public static void ToggleIndicatorMode()
        {
            _ShowGazeIndicator = !_ShowGazeIndicator;

            if (null != OnIndicatorModeToggle)
                OnIndicatorModeToggle(ShowGazeIndicator);
        }

        private static void ToggleSmoothMode()
        {
            _UseSmoothed = !_UseSmoothed;

            if (null != OnSmoothModeToggle)
                OnSmoothModeToggle(UseSmoothed);
        }

        public static void ToggleDebugMode()
        {
            _ShowDebug = !_ShowDebug;

            if (null != OnDebugModeToggle)
                OnDebugModeToggle(ShowDebug);
        }
    }
}
