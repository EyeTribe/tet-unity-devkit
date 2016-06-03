/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using EyeTribe.ClientSdk;
using System.Collections.Generic;
using EyeTribe.ClientSdk.Data;
using System;
using EyeTribe.Unity;
using VRStandardAssets.Utils;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles logic associated to a calibration process.
    /// </summary>
    public class CalibrationManager : MonoBehaviour, ICalibrationProcessHandler
    {
        [SerializeField]protected Camera _Camera;
        [SerializeField]private Canvas _Canvas;
        [SerializeField]private EyeUI _EyeUI;
        [SerializeField]private StateNotifyer _StateNotifyer;
        [SerializeField]private GazeRaycaster _GazeRaycaster;
        
        [SerializeField]private CalibrationArea _CalibArea;

        [SerializeField]protected GameObject _CalibPoint;
        [SerializeField]private float _CalibPointDepth = 2f;
        private Point2D _CalibPoint2D;

        [SerializeField]private Text _InfoText;

        [SerializeField]private UnityDispatcher _Dispatcher;

        private List<Point2D> _CalibrationPoints;

        private const int NUM_MAX_CALIBRATION_ATTEMPTS = 3;
        private const int NUM_MAX_RESAMPLE_POINTS = 4;
        private int _ResampleCount;

        private const float CALIB_SHOW_NEXT_POINT_DELAY = .25f;
        private const float CALIB_EYE_SETTLE_DELAY = .5f;
        private const float CALIB_PAUSE_AFTER_SAMPLE = 1.5f;

        void Awake()
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _Canvas)
                throw new Exception("_Canvas is not set!");

            if (null == _EyeUI)
                throw new Exception("_EyeUI is not set!");

            if (null == _StateNotifyer)
                throw new Exception("_StateNotifyer is not set!");

            if (null == _GazeRaycaster)
                throw new Exception("_GazeRaycaster is not set!");

            if (null == _CalibArea)
                throw new Exception("_CalibArea is not set!");

            if (null == _CalibPoint)
                throw new Exception("_CalibPoint is not set!");

            if (null == _InfoText)
                throw new Exception("_InfoText is not set!");

            if (null == _Dispatcher)
                throw new Exception("_Dispatcher is not set!");
        }

        void OnEnable () 
        {
            //preprare calibration point container
            _CalibrationPoints = new List<Point2D>();

            _CalibPoint.SetRendererEnabled(false);
            _CalibArea.gameObject.SetRendererEnabled(false);

            //reset calibration
            GazeManager.Instance.CalibrationAbortAsync();

            // Listen for VR input events
            VRInput.OnSwipe += OnSwipe;
            VRInput.OnClick += OnClick;
        }

        void OnDisable()
        {
            VRInput.OnSwipe -= OnSwipe;
            VRInput.OnClick -= OnClick;
        }

        private void ShortDelay()
        {
            GazeManager.Instance.CalibrationPointEnd();

            //disable cp
            _CalibPoint.SetRendererEnabled(false);

            //short delay before calling next cp
            if (_CalibrationPoints.Count > 0)
                Invoke("ShowNextCalibrationPoint", CALIB_SHOW_NEXT_POINT_DELAY);
        }

        private void ShowNextCalibrationPoint()
        {
            if (_CalibrationPoints.Count > 0)
            {
                //fetch next calibration point
                _CalibPoint2D = _CalibrationPoints[0];
                _CalibrationPoints.RemoveAt(0);

                //position GO based on screen coordinates
                _CalibPoint.SetWorldPositionFromGaze(_Camera, _CalibPoint2D, _CalibPointDepth);

                //enable cp
                _CalibPoint.SetRendererEnabled(true);

                //short delay allowing eye to settle before sampling
                Invoke("SampleCalibrationPoint", CALIB_EYE_SETTLE_DELAY);

                //call pause after sampling
                Invoke("ShortDelay", CALIB_PAUSE_AFTER_SAMPLE);
            }
        }

        protected virtual void SampleCalibrationPoint()
        {
            GazeManager.Instance.CalibrationPointStart((int)Math.Round(_CalibPoint2D.X), (int)Math.Round(_CalibPoint2D.Y));
        }

        public void StartCalibration()
        {
            if (!GazeManager.Instance.IsCalibrating)
            {
                float width = _Camera.pixelWidth;
                float height = _Camera.pixelHeight;

                int calibWidth = (int)Math.Round(width * _CalibArea.CalibAreaSizeIncrementRelativeX);
                int calibHeight = (int)Math.Round(height * _CalibArea.CalibAreaSizeIncrementRelativeY);

                int paddingHors = (int)Math.Round((width - calibWidth) * .5f);
                int paddingVert = (int)Math.Round((height - calibHeight) * .5f);

                _CalibrationPoints = UnityCalibUtils.InitCalibrationPoints(3, 3,
                    width, height,
                    paddingHors, paddingVert);

                GazeManager.Instance.CalibrationStart(9, this);
            }
        }

        public void OnApplicationQuit()
        {
            GazeManager.Instance.CalibrationAbort();
        }

        public void LoadNextScene()
        {
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        }

        void Update() 
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Joystick1Button0)
                )
            {
                if (GazeManager.Instance.Trackerstate != GazeManager.TrackerState.TRACKER_CONNECTED)
                {
                    _StateNotifyer.ShowState("Trackerstate is invalid!");
                }
                else
                {
                    // start new calibration
                    StartCalibration();
                }
            }

            // detect keyboard 'enter', mouse press or tap
            if (GazeManager.Instance.IsActivated &&
                GazeManager.Instance.IsCalibrated &&
                (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Joystick1Button1)
                )
                )
            {
                //If system calibrated, go to main scene
                LoadNextScene();
            }

            // Handle exit if _Canvas inactive
            if (!_Canvas.gameObject.activeInHierarchy)
            {
                // detect keyboard 'esc' or Android 'back'
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GoBackOrExit();
                }
            }
        }

        private void GoBackOrExit()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
                UnityEditor.EditorApplication.isPlaying = false;
            else
#endif
                Application.Quit();
        }

        /**
         * Handles VR user input from e.g. GearVR
         */
        private void OnClick()
        {
            if (VRMode.IsRunningInVRMode())
            { 
                if (gameObject.activeInHierarchy)
                {
                    if (GazeManager.Instance.IsActivated && GazeManager.Instance.IsCalibrated)
                    {
                        //If VR and system calibrated, go to main scene
                        LoadNextScene();
                    }
                    else
                    {
                        if (GazeManager.Instance.Trackerstate != GazeManager.TrackerState.TRACKER_CONNECTED)
                        {
                            _StateNotifyer.ShowState("Trackerstate is invalid!");
                        }
                        else
                        {
                            // start new calibration
                            StartCalibration();
                        }
                    }
                }
            }
        }

        /**
         * Handles VR user input from e.g. GearVR
         */
        protected void OnSwipe(VRInput.SwipeDirection swipe)
        {
            if (VRMode.IsRunningInVRMode())
            {
                if (gameObject.activeInHierarchy)
                {
                    if (VRInput.SwipeDirection.LEFT == swipe ||
                        VRInput.SwipeDirection.RIGHT == swipe)
                    {
                        if (GazeManager.Instance.Trackerstate != GazeManager.TrackerState.TRACKER_CONNECTED)
                        {
                            _StateNotifyer.ShowState("Trackerstate is invalid!");
                        }
                        else
                        {
                            // start new calibration
                            StartCalibration();
                        }
                    }
                }
            }
        }

        public void OnCalibrationStarted()
        {
            // Dispatch to Unity main thread
            _Dispatcher.Dispatch(() =>
            {
                if (_Canvas.gameObject.activeInHierarchy)
                    _Canvas.gameObject.SetActive(false);

                if (_EyeUI.gameObject.activeInHierarchy)
                    _EyeUI.gameObject.SetActive(false);

                Invoke("ShowNextCalibrationPoint", 1);
            });
        }

        public void OnCalibrationProgress(double progress)
        {
            //Called every time a new calibration point have been sampled
        }

        public void OnCalibrationProcessing()
        {
            // Dispatch to Unity main thread
            _Dispatcher.Dispatch(() =>
            {
                if (!_Canvas.gameObject.activeInHierarchy)
                    _Canvas.gameObject.SetActive(true);

                if (!_EyeUI.gameObject.activeInHierarchy)
                    _EyeUI.gameObject.SetActive(true);

                _InfoText.rectTransform.SetRendererEnabled(true);
                _InfoText.text = "Processing Calibration";
            });
        }

        public void OnCalibrationResult(CalibrationResult calibResult)
        {
            // Dispatch to Unity main thread
            _Dispatcher.Dispatch(() =>
            {
                Debug.Log("OnCalibrationResult: result: " + calibResult.Result + ", Avg error: " + calibResult.AverageErrorDegree);
                _InfoText.rectTransform.SetRendererEnabled(false);


                //Should we resample?
                if (!calibResult.Result)
                {
                    //Evaluate results
                    foreach (var calPoint in calibResult.Calibpoints)
                    {
                        if (calPoint.State == CalibrationPoint.STATE_RESAMPLE || calPoint.State == CalibrationPoint.STATE_NO_DATA)
                        {
                            _CalibrationPoints.Add(new Point2D(calPoint.Coordinates.X, calPoint.Coordinates.Y));
                        }
                    }

                    //Should we abort?
                    if (_ResampleCount++ >= NUM_MAX_CALIBRATION_ATTEMPTS || _CalibrationPoints.Count == 0 || _CalibrationPoints.Count >= NUM_MAX_RESAMPLE_POINTS)
                    {
                        _CalibrationPoints.Clear();
                        GazeManager.Instance.CalibrationAbort();

                        //ResetUI();

                        Debug.Log("Calibration FAIL");
                    }
                    else
                    {
                        Invoke("ShowNextCalibrationPoint", CALIB_SHOW_NEXT_POINT_DELAY);
                    }
                }
                else
                {
                    if (calibResult.AverageErrorDegree < 1.5)
                    {
                        //ResetUI();
                        //GazeIndicatorSwitch(true);

                        Debug.Log("Calibration SUCCESS");

                        LoadNextScene();
                    }
                    else
                    {
                        _CalibrationPoints.Clear();
                        GazeManager.Instance.CalibrationAbort();

                        //ResetUI();

                        Debug.Log("Calibration FAIL");
                    }
                }
            });
        }
    }
}
