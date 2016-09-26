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
using EyeTribe.ClientSdk.Utils;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles the UI associated to a calibration process.
    /// </summary>
    public class CalibrationUI : MonoBehaviour
    {
        [SerializeField] private CalibrationManager _CalibrationManager;
        [SerializeField] private GazeInfoController _GazeInfoController;
        [SerializeField] private StateNotifyer _StateNotifyer;

        [SerializeField] private Button _ActionButton;
        [SerializeField] private Button _StartButton;

        [SerializeField] private Text _InfoText;
        [SerializeField] private Text _QualityText;

        private System.Object _UpdateLock = new System.Object();

        private bool _ToggleIndicatorState;

        void Awake()
        {
            if (null == _CalibrationManager)
                throw new Exception("_CalibrationManager is not set!");

            if (null == _StateNotifyer)
                throw new Exception("_StateNotifyer is not set!");

            if (null == _ActionButton && VRMode.IsRunningInVRMode)
                throw new Exception("_ActionButton is not set!");

            if (null == _StartButton && VRMode.IsRunningInVRMode)
                throw new Exception("_StartButton is not set!");

            if (null == _InfoText)
                throw new Exception("_InfoText is not set!");

            if (null == _QualityText)
                throw new Exception("_QualityText is not set!");

            _StartButton.onClick.RemoveAllListeners();
            _StartButton.onClick.AddListener(() => { _CalibrationManager.LoadNextScene(); });

            _QualityText.enabled = false;
            _InfoText.enabled = false;

            _StartButton.gameObject.SetActive(!VRMode.IsRunningInVRMode);
            _ActionButton.gameObject.SetActive(!VRMode.IsRunningInVRMode);
        }

        void OnEnable()
        {
            StartCoroutine(DelayedInitializer());

            EyeTribeSDK.OnConnectionStateChange += OnConnectionStateChange;
            EyeTribeSDK.OnCalibrationResult += OnCalibrationChange;
            EyeTribeSDK.OnTrackerStateChange += OnTrackerStateChange;
            EyeTribeSDK.OnCalibrationStateChange += OnCalibrationStateChange;
        }

        void OnDisable()
        {
            EyeTribeSDK.OnConnectionStateChange -= OnConnectionStateChange;
            EyeTribeSDK.OnCalibrationResult -= OnCalibrationChange;
            EyeTribeSDK.OnTrackerStateChange -= OnTrackerStateChange;
            EyeTribeSDK.OnCalibrationStateChange -= OnCalibrationStateChange;
        }

        private IEnumerator DelayedInitializer()
        {
            while (null == _GazeInfoController.GUIAnchor)
            {
                // we wait until anchor is initialized

                yield return new WaitForSeconds(.1f);
            }

            if (VRMode.IsRunningInVRMode)
            {
                InitUiVrMode(_GazeInfoController.GUIAnchor);
            }
            else
            {
                InitUiRemoteMode();
            }

            UpdateGUIState();
        }

        private void InitUiVrMode(RectTransform last)
        {
            _ActionButton.gameObject.SetActive(false);
            _StartButton.gameObject.SetActive(false);

            _InfoText.enabled = false;

            _QualityText.alignment = TextAnchor.MiddleCenter;
            _QualityText.rectTransform.anchoredPosition = new Vector2(last.anchoredPosition.x, last.anchoredPosition.y + _QualityText.rectTransform.sizeDelta.y);
            _QualityText.rectTransform.anchorMin = new Vector2(.5f, .5f);
            _QualityText.rectTransform.anchorMax = new Vector2(.5f, .5f);
            _QualityText.enabled = false;
        }

        private void InitUiRemoteMode()
        {
            _ActionButton.gameObject.SetActive(false);
            _StartButton.gameObject.SetActive(false);

            _QualityText.alignment = TextAnchor.MiddleLeft;
            _QualityText.rectTransform.anchorMin = new Vector2(0, 0);
            _QualityText.rectTransform.anchorMax = new Vector2(0, 0);
            _QualityText.rectTransform.anchoredPosition = new Vector2(10 + _QualityText.rectTransform.sizeDelta.x * .5f, 10 + _QualityText.rectTransform.sizeDelta.y * .5f);
        }

        void UpdateGUIState()
        {
            if (!VRMode.IsRunningInVRMode)
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

                    if (GazeGUIController.ShowGazeIndicator)
                    {
                        _ToggleIndicatorState = true;
                        GazeGUIController.ToggleIndicatorMode();
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
                            GazeGUIController.ToggleIndicatorMode();
                        }
                    }

                    if (!_StartButton.gameObject.activeInHierarchy)
                    {
                        if (GazeManager.Instance.Trackerstate == GazeManager.TrackerState.TRACKER_CONNECTED)
                            _StartButton.gameObject.SetActive(true);
                    }

                    if (!_QualityText.rectTransform.IsRendererEnabled())
                    {
                        CalibrationResult result = GazeManager.Instance.LastCalibrationResult;

                        string calibText = UnityCalibUtils.GetCalibString(result);
                        _QualityText.text = "<b>Calibration Quality: </b>" + calibText;

                        _QualityText.enabled = true;
                    }
                }
            }
        }

        private void UpdateUiVrMode()
        {
            if (!GazeManager.Instance.IsActivated)
            {
                // GazeManager not connected
                _QualityText.enabled = true;
                _InfoText.enabled = true;
                _InfoText.text = "Unable to connect to Server";
            }
            else
            {
                if (GazeManager.Instance.IsCalibrating)
                {
                    // Hide UI during calibration

                    if (GazeGUIController.ShowGazeIndicator)
                    {
                        _ToggleIndicatorState = true;
                        GazeGUIController.ToggleIndicatorMode();
                    }
                }
                else
                {
                    // Update main calibration info text
                    string calibrate;
                    if (!GazeManager.Instance.IsCalibrated)
                    {
                        calibrate = "Press <b>FIRE1*</b> or <b>SWIPE FORWARD</b>\nto begin calibration";
                    }
                    else
                    {
                        calibrate = "Press <b>FIRE1*</b> or <b>SWIPE FORWARD</b>\nto begin re-calibration\n\nPress <b>FIRE2</b> or <b>SWIPE DOWN</b> to start demo";
                    }
                    _InfoText.text = calibrate;
                    _InfoText.enabled = true;

                    // Update Calibration quality
                    CalibrationResult result = GazeManager.Instance.LastCalibrationResult;
                    string calibText = UnityCalibUtils.GetCalibString(result);

                    if (null != result)
                    { 
                        _QualityText.rectTransform.SetRendererEnabled(true);
                        _QualityText.text = "<b>Calibration Quality: </b>" + calibText;
                    }

                    //restore pre-calibration indicator state
                    if (_ToggleIndicatorState)
                    {
                        _ToggleIndicatorState = !_ToggleIndicatorState;
                        GazeGUIController.ToggleIndicatorMode();
                    }
                }
            }
        }

        private void ResetUiState()
        {
            _ActionButton.gameObject.SetActive(false);
            _StartButton.gameObject.SetActive(false);

            _InfoText.enabled = false;
            _QualityText.enabled = false;
        }

        public void OnConnectionStateChange(bool isConnected)
        {
            lock (_UpdateLock)
            { 
                ResetUiState();
                UpdateGUIState();
            }
        }

        public void OnCalibrationChange(bool isCalibrated, CalibrationResult calibResult)
        {
            lock (_UpdateLock)
            {
                ResetUiState();
                UpdateGUIState();
            }
        }

        public void OnTrackerStateChange(GazeManager.TrackerState trackerState)
        {
            lock (_UpdateLock)
            {
                ResetUiState();
                UpdateGUIState();
            }
        }

        public void OnCalibrationStateChange(bool isCalibrating, bool isCalibrated)
        {
            lock (_UpdateLock)
            {
                if (isCalibrating)
                    _QualityText.enabled = false;
                else
                    _QualityText.enabled = true;

                UpdateGUIState();
            }
        }
    }
}
