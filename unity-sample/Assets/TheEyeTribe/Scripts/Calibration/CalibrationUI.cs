/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.VR;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using EyeTribe.Unity;
using VRStandardAssets.Utils;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles the UI associated to a calibration process.
    /// </summary>
    public class CalibrationUI : MonoBehaviour
    {
        [SerializeField]private CalibrationManager _CalibrationManager;
        [SerializeField]private GazeUiController _GazeUiController;
        [SerializeField]private StateNotifyer _StateNotifyer;

        [SerializeField]private Button _ActionButton;
        [SerializeField]private Button _StartButton;
        
        [SerializeField]private Text _InfoText;
        [SerializeField]private Text _QualityText;

        private bool _ToggleIndicatorState;

        void Awake()
        {
            if (null == _CalibrationManager)
                throw new Exception("_CalibrationManager is not set!");

            if (null == _GazeUiController)
                throw new Exception("_GazeUiController is not set!");

            if (null == _StateNotifyer)
                throw new Exception("_StateNotifyer is not set!");

            if (null == _ActionButton)
                throw new Exception("_ActionButton is not set!");

            if (null == _StartButton)
                throw new Exception("_StartButton is not set!");

            if (null == _InfoText)
                throw new Exception("_InfoText is not set!");

            if (null == _QualityText)
                throw new Exception("_QualityText is not set!");

            _StartButton.onClick.RemoveAllListeners();
            _StartButton.onClick.AddListener(() => { _CalibrationManager.LoadNextScene(); });

            _QualityText.rectTransform.SetRendererEnabled(false);

            _InfoText.rectTransform.SetRendererEnabled(false);
        }

        void OnEnable()
        {
            RectTransform last = _GazeUiController.GetLastUiRectTransform();

            if (VRMode.IsRunningInVRMode())
            {
                InitUiVrMode(last);
            }
            else
            {
                InitUiRemoteMode(last);
            }

            EyeTribeSDK.OnConnectionStateChange += OnConnectionStateChange;
            EyeTribeSDK.OnCalibration += OnCalibrationChange;
            EyeTribeSDK.OnTrackerStateChange += OnTrackerStateChange;
        }

        void OnDisable()
        {
            EyeTribeSDK.OnConnectionStateChange -= OnConnectionStateChange;
            EyeTribeSDK.OnCalibration -= OnCalibrationChange;
            EyeTribeSDK.OnTrackerStateChange -= OnTrackerStateChange;
        }

        private void InitUiVrMode(RectTransform last)
        {
            _ActionButton.gameObject.SetActive(false);
            _StartButton.gameObject.SetActive(false);

            _InfoText.rectTransform.SetRendererEnabled(false);
            
            _QualityText.alignment = TextAnchor.MiddleCenter;
            _QualityText.rectTransform.anchoredPosition = new Vector2(last.anchoredPosition.x, last.anchoredPosition.y + _QualityText.rectTransform.sizeDelta.y);
            _QualityText.rectTransform.anchorMin = new Vector2(.5f, 0);
            _QualityText.rectTransform.anchorMax = new Vector2(.5f, 0);
            _QualityText.rectTransform.SetRendererEnabled(false);
        }

        private void InitUiRemoteMode(RectTransform last)
        {
            _ActionButton.gameObject.SetActive(false);
            _StartButton.gameObject.SetActive(false);

            _QualityText.alignment = TextAnchor.MiddleLeft;
            _QualityText.rectTransform.anchorMin = new Vector2(0, 0);
            _QualityText.rectTransform.anchorMax = new Vector2(0, 0);
            _QualityText.rectTransform.anchoredPosition = new Vector2(10 + _QualityText.rectTransform.sizeDelta.x * .5f, 10);
        }

        void OnGUI()
        {
            if (!VRMode.IsRunningInVRMode())
            {
                UpdateUiRemoteMode();
            }
            else
            {
                UpdateUiVrMode();
            }
        }

        private void UpdateUiRemoteMode()
        {
            if (!GazeManager.Instance.IsActivated)
            {
                // GazeManager not connected, optional re-connect to server

                if (!_ActionButton.gameObject.activeInHierarchy)
                {
                    _ActionButton.gameObject.SetActive(true);

                    _ActionButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    _ActionButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GazeManager.Instance.ActivateAsync();
                    });
                    _ActionButton.GetComponentInChildren<Text>().text = "Reconnect to server";
                }
            }
            else
            {
                if (GazeManager.Instance.IsCalibrating)
                {
                    // Hide UI during calibration

                    if (_GazeUiController.ShowGazeIndicator)
                    {
                        _ToggleIndicatorState = true;
                        _GazeUiController.ToggleIndicatorMode();
                    }

                    _ActionButton.gameObject.SetActive(false);
                    _StartButton.gameObject.SetActive(false);
                }
                else
                {
                    // Update UI depending on state

                    if (!_ActionButton.gameObject.activeInHierarchy)
                    {
                        _ActionButton.gameObject.SetActive(true);

                        string calibrate;
                        if (!GazeManager.Instance.IsCalibrated)
                        {
                            calibrate = "Calibrate";
                        }
                        else
                        {
                            calibrate = "Re-calibrate";
                        }

                        _ActionButton.GetComponentInChildren<Text>().text = calibrate;

                        _ActionButton.GetComponent<Button>().onClick.RemoveAllListeners();
                        _ActionButton.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            if (GazeManager.Instance.Trackerstate != GazeManager.TrackerState.TRACKER_CONNECTED)
                            {
                                _StateNotifyer.ShowState("Trackerstate is invalid!");
                            }
                            else 
                            {
                                _CalibrationManager.StartCalibration();
                            }

                        });

                        if (_ToggleIndicatorState)
                        {
                            _ToggleIndicatorState = !_ToggleIndicatorState;
                            _GazeUiController.ToggleIndicatorMode();
                        }
                    }

                    if (!_StartButton.gameObject.activeInHierarchy)
                    {
                        if(GazeManager.Instance.Trackerstate == GazeManager.TrackerState.TRACKER_CONNECTED)
                            _StartButton.gameObject.SetActive(true);
                    }

                    if (!_QualityText.rectTransform.IsRendererEnabled())
                    {
                        CalibrationResult result = GazeManager.Instance.LastCalibrationResult;

                        string calibText = UnityCalibUtils.GetCalibString(result);
                        _QualityText.text = "Calibration Quality: " + calibText;

                        _QualityText.rectTransform.SetRendererEnabled(true);
                    }
                }
            }
        }

        private void UpdateUiVrMode()
        {
            if (!GazeManager.Instance.IsActivated)
            {
                if (!_InfoText.rectTransform.IsRendererEnabled())
                {
                    // GazeManager not connected

                    _InfoText.rectTransform.SetRendererEnabled(true);
                    _InfoText.text = "Unable to connect to Server";
                }
            }
            else
            {
                if (GazeManager.Instance.IsCalibrating)
                {
                    // Hide UI during calibration

                    if (_GazeUiController.ShowGazeIndicator)
                    {
                        _ToggleIndicatorState = true;
                        _GazeUiController.ToggleIndicatorMode();
                    }
                }
                else
                {
                    // Update UI depending on state

                    if (!_InfoText.rectTransform.IsRendererEnabled())
                    {
                        string calibrate;
                        if (!GazeManager.Instance.IsCalibrated)
                        {
                            calibrate = "Press SPACE or SWIPE to begin calibration";
                        }
                        else
                        {
                            calibrate = "Press SPACE or SWIPE to begin re-calibration\nPress ENTER to start demo";
                        }

                        _InfoText.text = calibrate;

                        _InfoText.rectTransform.SetRendererEnabled(true);
                    }

                    if (!_QualityText.gameObject.IsRendererEnabled())
                    {
                        CalibrationResult result = GazeManager.Instance.LastCalibrationResult;
                        string calibText = UnityCalibUtils.GetCalibString(result);

                        _QualityText.gameObject.SetRendererEnabled(true);
                        _QualityText.text = "Calibration Quality: " + calibText;
                    }

                    //restore pre-calibration indicator state
                    if (_ToggleIndicatorState)
                    {
                        _ToggleIndicatorState = !_ToggleIndicatorState;
                        _GazeUiController.ToggleIndicatorMode();
                    }
                }
            }
        }

        private void ResetUiState()
        {
            _ActionButton.gameObject.SetActive(false);
            _StartButton.gameObject.SetActive(false);

            _InfoText.rectTransform.SetRendererEnabled(false);
            _QualityText.rectTransform.SetRendererEnabled(false);
        }

        public void OnConnectionStateChange(bool isConnected)
        {
            ResetUiState();
        }

        public void OnCalibrationChange(bool isCalibrated, CalibrationResult calibResult)
        {
            ResetUiState();
        }

        public void OnTrackerStateChange(GazeManager.TrackerState trackerState)
        {
            ResetUiState();
        }
    }
}
