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
using UnityEngine.UI;
using VRStandardAssets.Utils;
using EyeTribe.Unity.Calibration;
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles updates of the GazeInfo GUI at a user defined interval.
    /// </summary>
    public class GazeInfoController : MonoBehaviour
    {
        [SerializeField] private Camera _Camera;
        [SerializeField] private UIFader _UiFader;

        [SerializeField] private int _UpdatesPerSecond = 5;
        private float _UpdateDelaySeconds;

        [SerializeField] private GameFPS _GameFPS;
        [SerializeField] private GazeFPS _GazeFPS;

        [SerializeField] private Text _ScreenSizeText;
        [SerializeField] private Text _ScreenCoordsText;
        [SerializeField] private Text _EyeTribeFpsText;
        [SerializeField] private Text _GameFPSText;

        private RectTransform _LastUiRectransform;

        private IEnumerator _GazeCoordUpdater;
        private IEnumerator _GameFPSUpdater;
        private IEnumerator _GazeFPSUpdater;

        public RectTransform GUIAnchor { get { return _LastUiRectransform; } private set{} }

        private bool _IsLocked;
        private bool _PreLockState;

        void Awake()
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _UiFader)
                throw new Exception("_UiFader is not set!");

            if (null == _GameFPS)
                throw new Exception("_GameFPS is not set!");

            if (null == _GazeFPS)
                throw new Exception("_GazeFPS is not set!");

            if (null == _ScreenSizeText)
                throw new Exception("_ScreenSizeText is not set!");

            if (null == _ScreenCoordsText)
                throw new Exception("_ScreenCoordsText is not set!");

            if (null == _EyeTribeFpsText)
                throw new Exception("_EyeTribeFpsText is not set!");

            if (null == _GameFPSText)
                throw new Exception("_GameFPSText is not set!");

            if (_UpdatesPerSecond <= 0)
                throw new Exception("UpdatesPerSecond must be a positive number");

            _UpdateDelaySeconds = 1f / _UpdatesPerSecond;
        }

        void OnEnable()
        {
            StartCoroutine(DelayedInitializer());

            _ScreenSizeText.text = "<b>Screen w: </b>" + Screen.width + ", h: " + Screen.height;

            _UiFader.SetInvisible();

            GazeGUIController.OnDebugModeToggle += ToggleDebug;

            EyeTribeSDK.OnCalibrationStateChange += OnCalibrationStateChange;
        }

        void OnDisable()
        {
            GazeGUIController.OnDebugModeToggle -= ToggleDebug;

            EyeTribeSDK.OnCalibrationStateChange -= OnCalibrationStateChange;
        }

        private IEnumerator DelayedInitializer()
        {
            float delay = .1f;
            float t = 0;

            while (t < delay || !_Camera.isActiveAndEnabled)
            {
                t += Time.deltaTime;

                // we wait until Camera & anchor is initialized

                yield return new WaitForSeconds(.1f);
            }

            ToggleDebug(GazeGUIController.ShowDebug);

            // Initialize placement of gaze info
            if (VRMode.IsRunningInVRMode)
            {
                InitUiVrMode();
            }
            else
            {
                InitUiRemoteMode();
            }
        }

        private void InitUiVrMode()
        {
            Vector2 anchor = new Vector2(0, (-_Camera.pixelRect.height * .2f));

            _EyeTribeFpsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _EyeTribeFpsText.rectTransform.anchorMin = new Vector2(.5f, .5f);
            _EyeTribeFpsText.rectTransform.anchorMax = new Vector2(.5f, .5f);
            _EyeTribeFpsText.rectTransform.anchoredPosition = anchor;

            _GameFPSText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _GameFPSText.rectTransform.anchorMin = new Vector2(.5f, .5f);
            _GameFPSText.rectTransform.anchorMax = new Vector2(.5f, .5f);
            _GameFPSText.rectTransform.anchoredPosition = new Vector2(_EyeTribeFpsText.rectTransform.anchoredPosition.x,
                _EyeTribeFpsText.rectTransform.anchoredPosition.y + _GameFPSText.rectTransform.sizeDelta.y);

            _ScreenCoordsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _ScreenCoordsText.rectTransform.anchorMin = new Vector2(.5f, .5f);
            _ScreenCoordsText.rectTransform.anchorMax = new Vector2(.5f, .5f);
            _ScreenCoordsText.rectTransform.anchoredPosition = new Vector2(_GameFPSText.rectTransform.anchoredPosition.x,
                _GameFPSText.rectTransform.anchoredPosition.y + _ScreenCoordsText.rectTransform.sizeDelta.y);

            _ScreenSizeText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _ScreenSizeText.rectTransform.anchorMin = new Vector2(.5f, .5f);
            _ScreenSizeText.rectTransform.anchorMax = new Vector2(.5f, .5f);
            _ScreenSizeText.rectTransform.anchoredPosition = new Vector2(_ScreenCoordsText.rectTransform.anchoredPosition.x,
                _ScreenCoordsText.rectTransform.anchoredPosition.y + _ScreenSizeText.rectTransform.sizeDelta.y);

            _LastUiRectransform = _ScreenSizeText.rectTransform;
        }

        private void InitUiRemoteMode()
        {
            Vector2 anchor = new Vector2(-_EyeTribeFpsText.rectTransform.sizeDelta.x * .5f - 10, -_EyeTribeFpsText.rectTransform.sizeDelta.y * .5f - 10);

            _EyeTribeFpsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _EyeTribeFpsText.rectTransform.anchorMin = new Vector2(1, 1);
            _EyeTribeFpsText.rectTransform.anchorMax = new Vector2(1, 1);
            _EyeTribeFpsText.rectTransform.anchoredPosition = anchor;

            _GameFPSText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _GameFPSText.rectTransform.anchorMin = new Vector2(1, 1);
            _GameFPSText.rectTransform.anchorMax = new Vector2(1, 1);
            _GameFPSText.rectTransform.anchoredPosition = new Vector2(anchor.x,
                _EyeTribeFpsText.rectTransform.anchoredPosition.y - _GameFPSText.rectTransform.sizeDelta.y);

            _ScreenCoordsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _ScreenCoordsText.rectTransform.anchorMin = new Vector2(1, 1);
            _ScreenCoordsText.rectTransform.anchorMax = new Vector2(1, 1);
            _ScreenCoordsText.rectTransform.anchoredPosition = new Vector2(anchor.x,
                _GameFPSText.rectTransform.anchoredPosition.y - _ScreenCoordsText.rectTransform.sizeDelta.y);

            _ScreenSizeText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _ScreenSizeText.rectTransform.anchorMin = new Vector2(1, 1);
            _ScreenSizeText.rectTransform.anchorMax = new Vector2(1, 1);
            _ScreenSizeText.rectTransform.anchoredPosition = new Vector2(anchor.x,
                _ScreenCoordsText.rectTransform.anchoredPosition.y - _ScreenSizeText.rectTransform.sizeDelta.y);

            _LastUiRectransform = _ScreenSizeText.rectTransform;
        }

        private void ToggleDebug(bool isDebug)
        {
            if (isDebug)
            {
                StartCoroutine(_GazeCoordUpdater = GazeCoordUpdater());
                StartCoroutine(_GameFPSUpdater = GameFPSUpdater());
                StartCoroutine(_GazeFPSUpdater = GazeFPSUpdater());
            }
            else
            {
                if (null != _GazeCoordUpdater)
                    StopCoroutine(_GazeCoordUpdater);
                if (null != _GameFPSUpdater)
                    StopCoroutine(_GameFPSUpdater);
                if (null != _GazeFPSUpdater)
                    StopCoroutine(_GazeFPSUpdater);
            }

            if(isDebug)
                StartCoroutine(_UiFader.InteruptAndFadeIn());
            else
                StartCoroutine(_UiFader.InteruptAndFadeOut());
        }

        private void OnCalibrationStateChange(bool isCalibrating, bool isCalibrated)
        {
            if (!_IsLocked && isCalibrating)
            {
                _IsLocked = isCalibrating;
                _PreLockState = GazeGUIController.ShowDebug;
                ToggleDebug(false);
            }
            else if (_IsLocked && !isCalibrating)
            {
                _IsLocked = isCalibrating;
                ToggleDebug(_PreLockState);
            }
        }

        private IEnumerator GazeCoordUpdater()
        {
            while (enabled)
            {
                Point3D gazeVec = GazeFrameCache.Instance.GetLastGazeVectorAvg();
                String coords = "";

                if (Point3D.Zero != gazeVec)
                {
                    coords = "<b>RAY: </b>" + gazeVec.ToVec3().ToString("0.00");
                }
                else
                {
                    // If no gaze vec, define ray based on 2D gaze coordinates
                    Point2D gazeCoords;
                    if (GazeGUIController.UseSmoothed)
                        gazeCoords = GazeFrameCache.Instance.GetLastSmoothedGazeCoordinates();
                    else
                        gazeCoords = GazeFrameCache.Instance.GetLastRawGazeCoordinates();

                    if (Point2D.Zero != gazeCoords)
                    {
                        // update gaze coords ui
                        Point2D gazeCoordsToUi = Point2D.Zero != gazeCoords ? gazeCoords : Point2D.Zero;
                        coords = (GazeGUIController.UseSmoothed ? "<b>SMOOTH: </b>" : "<b>RAW: </b>") + gazeCoordsToUi.X.ToString("0.00") + ", " + gazeCoordsToUi.Y.ToString("0.00");
                    }
                    else
                    {
                        coords = (GazeGUIController.UseSmoothed ? "<b>SMOOTH: </b>" : "<b>RAW: </b>") + "NA";
                    }
                }

                _ScreenCoordsText.text = coords;

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }

        private IEnumerator GameFPSUpdater()
        {
            while (enabled)
            {
                _GameFPSText.text = "<b>Game FPS: </b>" + _GameFPS.AvgFPS.ToString("0.00");

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }

        private IEnumerator GazeFPSUpdater()
        {
            while (enabled)
            {
                _EyeTribeFpsText.text = "<b>EyeTribe FPS: </b>" + _GazeFPS.AvgFPS.ToString("0.00");

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }
    }
}