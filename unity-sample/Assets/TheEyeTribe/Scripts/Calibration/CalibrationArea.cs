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
using EyeTribe.ClientSdk;
using System;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles size of calibration area in remote and VR modes
    /// </summary>
    public class CalibrationArea : MonoBehaviour
    {
        [SerializeField]private Camera _Camera;

        [SerializeField]public GameObject _CalibArea;

        private float _CalibAreaSubSampleLevel = 40;
        private float _CalibAreaSizeIncrementX;
        private float _CalibAreaSizeIncrementY;
        private float _CalibAreaPaddingXNum;
        private float _CalibAreaPaddingYNum;

        public float CalibAreaSizeIncrementRelativeX { get; private set; }
        public float CalibAreaSizeIncrementRelativeY { get; private set; }

        void Awake() 
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _CalibArea)
                throw new Exception("_CalibArea is not set!");
        }

        void OnEnable()
        {
            InitCalibAreaSize(_CalibArea);

            UpdateCalibAreaSize(_CalibArea);

            _CalibArea.gameObject.SetRendererEnabled(false);
        }

        void Update() 
        {
            HandleInput();
        }

        private void HandleInput()
        {
            bool updateCalibArea = false;

            if (!GazeManager.Instance.IsCalibrating)
            {
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    _CalibArea.gameObject.SetRendererEnabled(!_CalibArea.gameObject.IsRendererEnabled());
                }

                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    if (_CalibAreaPaddingXNum < _CalibAreaSubSampleLevel * .5f)
                        ++_CalibAreaPaddingXNum;

                    updateCalibArea = true;
                }

                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    if (_CalibAreaPaddingXNum > 0)
                        --_CalibAreaPaddingXNum;

                    updateCalibArea = true;
                }

                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    if (_CalibAreaPaddingYNum < _CalibAreaSubSampleLevel * .5f)
                        ++_CalibAreaPaddingYNum;

                    updateCalibArea = true;
                }

                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    if (_CalibAreaPaddingYNum > 0)
                        --_CalibAreaPaddingYNum;

                    updateCalibArea = true;
                }

                if (updateCalibArea)
                    UpdateCalibAreaSize(_CalibArea);
            }
        }

        private void InitCalibAreaSize(GameObject calibArea)
        {
            if (null != calibArea)
            {
                // total screen size in Unity units
                Vector2 worldScreen = _Camera.GetPerspectiveWorldScreenBounds(_CalibArea.transform.localPosition.z);

                // initial size de/incrementer
                _CalibAreaSizeIncrementX = worldScreen.x / _CalibAreaSubSampleLevel;
                _CalibAreaSizeIncrementY = worldScreen.y / _CalibAreaSubSampleLevel;
                _CalibAreaPaddingXNum = 34;
                _CalibAreaPaddingYNum = 32;

                // mode specific adjustments
                if (VRMode.IsRunningInVRMode())
                {
                    _CalibAreaPaddingXNum = 8;
                    _CalibAreaPaddingYNum = 7;
                }

                UpdateCalibAreaSize(calibArea);
            }
        }

        private void UpdateCalibAreaSize(GameObject calibArea)
        {
            if (null != calibArea)
            {
                Vector2 worldScreen = _Camera.GetPerspectiveWorldScreenBounds(calibArea.transform.localPosition.z);

                CalibAreaSizeIncrementRelativeX = (_CalibAreaPaddingXNum * _CalibAreaSizeIncrementX) / worldScreen.x;
                CalibAreaSizeIncrementRelativeY = (_CalibAreaPaddingYNum * _CalibAreaSizeIncrementY) / worldScreen.y;

                float localSizeX = worldScreen.x * CalibAreaSizeIncrementRelativeX;
                float localSizeY = worldScreen.y * CalibAreaSizeIncrementRelativeY;

                calibArea.transform.localScale = new Vector3(
                    localSizeX,
                    localSizeY,
                    calibArea.transform.localScale.z
                    );
            }
        }
    }
}
