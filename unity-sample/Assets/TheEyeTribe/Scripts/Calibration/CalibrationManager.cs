/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EyeTribe.Unity;
using EyeTribe.Unity.Interaction;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using VRStandardAssets.Utils;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles logic associated to a calibration process.
    /// </summary>
    public class CalibrationManager : MonoBehaviour, ICalibrationProcessHandler
    {
        [SerializeField] protected Camera _Camera;
        [SerializeField] protected UIFader _CalibUIFader;
        [SerializeField] protected UIFader _CalibUIRemoteFader;
        [SerializeField] protected EyeUI _EyeUI;
        [SerializeField] protected StateNotifyer _StateNotifyer;
        [SerializeField] protected GazeRaycaster _GazeRaycaster;

        [SerializeField] protected CalibrationArea _CalibArea;

        [SerializeField] protected GameObject _CalibPoint;
        [SerializeField] protected float _CalibPointDepth = 2f;
        [SerializeField] protected bool _CalibPointShouldMove;
        [SerializeField] protected MoveInterpolator _CalibPointMove;
        [SerializeField] protected bool _CalibPointIsLocal = true;
        [SerializeField] protected float _CalibSampleTimeSecs = 1f;

        private Point2D _CalibPoint2D;

        [SerializeField] protected Text _QualityText;
        [SerializeField] protected Text _InfoText;

        [SerializeField] protected UnityDispatcher _Dispatcher;

        protected List<Point2D> _CalibrationPoints;

        protected const int NUM_MAX_CALIBRATION_ATTEMPTS = 3;
        protected const int NUM_MAX_RESAMPLE_POINTS = 4;
        protected int _ResampleCount;

        protected const int CALIB_NUM_ROWS = 3;
        protected const int CALIB_NUM_COLUMNS = 3;

        protected const float CALIB_INTRO_DELAY = 3f;
        protected const float CALIB_SHOW_NEXT_POINT_DELAY = .25f;
        protected const float CALIB_SHOW_OR_MOVE_DELAY = .65f;
        protected const float CALIB_EYE_SETTLE_DELAY = .5f;

        protected virtual void Awake()
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _CalibUIFader)
                throw new Exception("_CalibUIFader is not set!");

            /* Allowed in inheriting classes
            if (null == _EyeUI)
                throw new Exception("_EyeUI is not set!");*/

            if (null == _StateNotifyer)
                throw new Exception("_StateNotifyer is not set!");

            if (null == _GazeRaycaster)
                throw new Exception("_GazeRaycaster is not set!");

            if (null == _CalibArea)
                throw new Exception("_CalibArea is not set!");

            if (null == _CalibPoint)
                throw new Exception("_CalibPoint is not set!");

            if (_CalibPointShouldMove && null == _CalibPointMove)
                throw new Exception("_CalibPointMove is not set!");

            _CalibPointMove.UseLocalTransform = _CalibPointIsLocal;

            if (null == _InfoText)
                throw new Exception("_InfoText is not set!");

            if (null == _Dispatcher)
                throw new Exception("_Dispatcher is not set!");
        }

        protected virtual void OnEnable()
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

        protected virtual void OnDisable()
        {
            VRInput.OnSwipe -= OnSwipe;
            VRInput.OnClick -= OnClick;
        }

        private void ShowNextCalibrationPoint()
        {
            if (_CalibrationPoints.Count > 0)
            {
                //fetch next calibration point
                _CalibPoint2D = _CalibrationPoints[0];
                _CalibrationPoints.RemoveAt(0);

                if (_CalibPointShouldMove)
                {
                    // animate GO based on screen coordinates
                    _CalibPoint.SetRendererEnabled(true);

                    if (_CalibPointIsLocal)
                    {
                        Vector3 worldPos = _CalibPoint2D.GetWorldPositionFromGaze(_Camera, _CalibPointDepth);
                        Vector3 newPos = worldPos.GetRelativePosition(_Camera.transform);
                        _CalibPointMove.MoveTo(newPos, CALIB_SHOW_OR_MOVE_DELAY);
                    }
                    else
                    {
                        Vector3 newPos = _CalibPoint2D.GetWorldPositionFromGaze(_Camera, _CalibPointDepth);
                        _CalibPointMove.MoveTo(newPos, CALIB_SHOW_OR_MOVE_DELAY);
                    }

                    Invoke("SampleCalibrationPoint", CALIB_SHOW_OR_MOVE_DELAY + CALIB_EYE_SETTLE_DELAY);
                }
                else
                {
                    // position GO based on screen coordinates
                    if (_CalibPointIsLocal)
                    {
                        Vector3 worldPos = _CalibPoint2D.GetWorldPositionFromGaze(_Camera, _CalibPointDepth);
                        _CalibPoint.transform.localPosition = worldPos.GetRelativePosition(_Camera.transform);
                    }
                    else
                    {
                        _CalibPoint.SetWorldPositionFromGaze(_Camera, _CalibPoint2D, _CalibPointDepth);
                    }

                    Invoke("ShowCalibPoint", CALIB_SHOW_OR_MOVE_DELAY);

                    Invoke("SampleCalibrationPoint", CALIB_SHOW_OR_MOVE_DELAY + CALIB_EYE_SETTLE_DELAY);
                }

                //call pause after sampling
                Invoke("EndSampling",
                    CALIB_SHOW_NEXT_POINT_DELAY +
                    CALIB_EYE_SETTLE_DELAY +
                    _CalibSampleTimeSecs
                    );
            }
        }

        private void ShowCalibPoint()
        {
            //enable cp
            _CalibPoint.SetRendererEnabled(true);
        }

        protected virtual void SampleCalibrationPoint()
        {
            GazeManager.Instance.CalibrationPointStart((int)Math.Round(_CalibPoint2D.X), (int)Math.Round(_CalibPoint2D.Y));
        }

        private void EndSampling()
        {
            GazeManager.Instance.CalibrationPointEnd();

            if (!_CalibPointShouldMove || _CalibrationPoints.Count == 0)
            {
                //disable cp
                _CalibPoint.SetRendererEnabled(false);
            }

            //short delay before calling next cp
            if (_CalibrationPoints.Count > 0)
                Invoke("ShowNextCalibrationPoint", CALIB_SHOW_NEXT_POINT_DELAY);
        }

        public void StartCalibration()
        {
            if (!GazeManager.Instance.IsCalibrating)
            {
                _QualityText.enabled = false;
                _InfoText.text = "\n\nFollow the <b>Calibration Point</b>";

                StartCoroutine(_CalibUIRemoteFader.InteruptAndFadeOut());

                StartCoroutine(_CalibUIFader.InteruptAndFadeIn());

                if (null != _EyeUI && _EyeUI.gameObject.activeInHierarchy)
                    _EyeUI.TurnOff();

                Invoke("DelayedShowCalibPoint", CALIB_INTRO_DELAY * .5f);

                Invoke("CallCalibrationStart", CALIB_INTRO_DELAY);
            }
        }

        private void DelayedShowCalibPoint()
        {
            _CalibPoint.SetRendererEnabled(true);
        }

        protected virtual void CallCalibrationStart()
        {
            _CalibrationPoints = _CalibArea.GetCalibrationPoints(CALIB_NUM_ROWS, CALIB_NUM_COLUMNS);

            StartCoroutine(_CalibUIFader.InteruptAndFadeOut());

            GazeManager.Instance.CalibrationStart(CALIB_NUM_ROWS * CALIB_NUM_COLUMNS, this);
        }

        public void OnApplicationQuit()
        {
            GazeManager.Instance.CalibrationAbort();
        }

        public virtual void LoadNextScene()
        {
            LevelManager.Instance.LoadNextLevel(2);
        }

        void Update()
        {
            HandleInput();
        }

        protected virtual void HandleInput()
        {
            // detect keyboard 'space', mouse/joy 0 press in Remote tracking mode
            if ((!VRMode.IsRunningInVRMode && Input.GetButtonDown("Fire1")))
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
            // detect keyboard 'enter', mouse/joy 1 press in Remote tracking mode
            else if (Input.GetButtonDown("Fire2"))
            {
                if (GazeManager.Instance.IsActivated && GazeManager.Instance.IsCalibrated)
                {
                    //If VR and system calibrated, go to main scene
                    LoadNextScene();
                }
            }
        }

        /**
         * Handles VR user input from e.g. GearVR
         */
        private void OnClick()
        {
            if (VRMode.IsRunningInVRMode)
            {
                if (GazeManager.Instance.Trackerstate != GazeManager.TrackerState.TRACKER_CONNECTED)
                {
                    _StateNotifyer.ShowState("Trackerstate is invalid!");
                }
                else
                {
                    if (GazeManager.Instance.IsActivated)
                    {
                        // if GearVR mode, we only start calibration by tap if not calibrated
                        if (VRMode.IsOculusSDKGearVR && !GazeManager.Instance.IsCalibrated)
                        {
                            StartCalibration();
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

        protected void OnSwipe(VRInput.SwipeDirection swipe)
        {
            // detect only if in GearVR mode
            if (VRMode.IsOculusSDKGearVR)
            {
                if (gameObject.activeInHierarchy)
                {
                    if (VRInput.SwipeDirection.RIGHT == swipe)
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
                    else if (VRInput.SwipeDirection.DOWN == swipe)
                    {
                        if (GazeManager.Instance.IsActivated && GazeManager.Instance.IsCalibrated)
                        {
                            //If VR and system calibrated, go to main scene
                            LoadNextScene();
                        }
                    }
                }
            }
        }

        public virtual void OnCalibrationStarted()
        {
            // Dispatch to Unity main thread
            _Dispatcher.Dispatch(() =>
            {
                Invoke("ShowNextCalibrationPoint", 0.1f);
            });
        }

        public virtual void OnCalibrationProgress(double progress)
        {
            //Called every time a new calibration point have been sampled
        }

        public virtual void OnCalibrationProcessing()
        {
            // Dispatch to Unity main thread
            _Dispatcher.Dispatch(() =>
            {
                StartCoroutine(_CalibUIFader.InteruptAndFadeIn());

                if (null != _EyeUI && _EyeUI.gameObject.activeInHierarchy)
                    _EyeUI.TurnOn();

                _InfoText.enabled = true;
                _InfoText.text = "\n\nProcessing Calibration";
            });
        }

        public virtual void OnCalibrationResult(CalibrationResult calibResult)
        {
            // Dispatch to Unity main thread
            _Dispatcher.Dispatch(() =>
            {
                Debug.Log("OnCalibrationResult: result: " + calibResult.Result + ", Avg error: " + calibResult.AverageErrorDegree);

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

                        StartCoroutine(_CalibUIRemoteFader.InteruptAndFadeIn());

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
                        Debug.Log("Calibration SUCCESS");

                        LoadNextScene();
                    }
                    else
                    {
                        _CalibrationPoints.Clear();
                        GazeManager.Instance.CalibrationAbort();

                        StartCoroutine(_CalibUIRemoteFader.InteruptAndFadeIn());

                        Debug.Log("Calibration FAIL");
                    }
                }
            });
        }
    }
}
