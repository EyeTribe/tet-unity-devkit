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
    /// Handles the logic associated to Gaze & Debug UI
    /// </summary>
    public class GazeUiController : MonoBehaviour
    {
        public static event Action<bool> OnSmoothModeToggle;
        public static event Action<bool> OnDebugModeToggle;
        public static event Action<bool> OnIndicatorModeToggle;

        [SerializeField]private Camera _Camera;

        [SerializeField]private int _UpdatesPerSecond = 5;
        private float _UpdateDelaySeconds;

        [SerializeField]private ReticleEyeTribe _GazeReticle;
        
        [SerializeField]private VRInput _VRInput;

        [SerializeField]private GameFPS _GameFPS;
        [SerializeField]private GazeFPS _GazeFPS;

        [SerializeField]private Text _ScreenSizeText;
        [SerializeField]private Text _ScreenCoordsText;
        [SerializeField]private Text _EyeTribeFpsText;
        [SerializeField]private Text _GameFPSText;

        [SerializeField]private Button _ExitButton;
        [SerializeField]private Button _ToggleButton;

        private RectTransform _LastUiRectransform;

        private bool _UseSmoothed;
        private bool _ShowDebug;
        private bool _ShowGazeIndicator = true;

        public bool UseSmoothed { get { return _UseSmoothed;} }
        public bool ShowDebug { get { return _ShowDebug; } }
        public bool ShowGazeIndicator { get { return _ShowGazeIndicator; } }

        private IEnumerator _GazeCoordUpdater;
        private IEnumerator _GameFPSUpdater;
        private IEnumerator _GazeFPSUpdater;

        void Awake()
        {
            if (null == _Camera)
                Debug.LogError("No Main Camera found!");

            if (null == _GazeReticle)
                Debug.LogError("No GazeReticle found!");

            if (null == _VRInput)
                throw new Exception("_VRInput is not set!");

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

            if (null == _ExitButton)
                throw new Exception("_ExitButton is not set!");

            if (null == _ToggleButton)
                throw new Exception("_ToggleButton is not set!");

            if (_UpdatesPerSecond <= 0)
                throw new Exception("UpdatesPerSecond must be a positive number");

            _UpdateDelaySeconds = 1f / _UpdatesPerSecond;

            _ExitButton.gameObject.SetRendererEnabled(false);
            _ExitButton.onClick.RemoveAllListeners();
            _ExitButton.onClick.AddListener(() => { LevelManager.Instance.LoadPreviousLevelOrExit(); });

            _ToggleButton.gameObject.SetRendererEnabled(false);
            _ToggleButton.onClick.RemoveAllListeners();
            _ToggleButton.onClick.AddListener(() => { ToggleIndicatorMode(); });

            _EyeTribeFpsText.rectTransform.SetRendererEnabled(ShowDebug);
            _GameFPSText.rectTransform.SetRendererEnabled(ShowDebug);
            _ScreenCoordsText.rectTransform.SetRendererEnabled(ShowDebug);
            _ScreenSizeText.rectTransform.SetRendererEnabled(ShowDebug);
        }

        void OnEnable()
        {
            Debug.Log("VRMode.IsRunningInVRMode: " + VRMode.IsRunningInVRMode());

            if (VRMode.IsRunningInVRMode())
            {
                InitUiVrMode();
            }
            else
            {
                InitUiRemoteMode();
            }

            _ScreenSizeText.text = "Screen w: " + Screen.width + ", h: " + Screen.height;

            StartCoroutine(_GazeCoordUpdater=GazeCoordUpdater());
            StartCoroutine(_GameFPSUpdater=GameFPSUpdater());
            StartCoroutine(_GazeFPSUpdater=GazeFPSUpdater());
        }

        void OnDisable() 
        {
            if (null != _GazeCoordUpdater)
                StopCoroutine(_GazeCoordUpdater);
            if (null != _GameFPSUpdater)
                StopCoroutine(_GameFPSUpdater);
            if (null != _GazeFPSUpdater)
                StopCoroutine(_GazeFPSUpdater);
        }

        private void InitUiVrMode()
        {
            _ExitButton.gameObject.SetActive(false);
            _ToggleButton.gameObject.SetActive(false);

            Vector2 anchor = new Vector2(0, (_Camera.pixelRect.height * .325f));

            _EyeTribeFpsText.rectTransform.anchorMin = new Vector2(.5f, 0);
            _EyeTribeFpsText.rectTransform.anchorMax = new Vector2(.5f, 0);
            _EyeTribeFpsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _EyeTribeFpsText.rectTransform.anchoredPosition = anchor;

            _GameFPSText.rectTransform.anchorMin = new Vector2(.5f, 0);
            _GameFPSText.rectTransform.anchorMax = new Vector2(.5f, 0);
            _GameFPSText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _GameFPSText.rectTransform.anchoredPosition = new Vector2(_EyeTribeFpsText.rectTransform.anchoredPosition.x,
                _EyeTribeFpsText.rectTransform.anchoredPosition.y + _GameFPSText.rectTransform.sizeDelta.y);

            _ScreenCoordsText.rectTransform.anchorMin = new Vector2(.5f, 0);
            _ScreenCoordsText.rectTransform.anchorMax = new Vector2(.5f, 0);
            _ScreenCoordsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _ScreenCoordsText.rectTransform.anchoredPosition = new Vector2(_GameFPSText.rectTransform.anchoredPosition.x,
                _GameFPSText.rectTransform.anchoredPosition.y + _ScreenCoordsText.rectTransform.sizeDelta.y);

            _ScreenSizeText.rectTransform.anchorMin = new Vector2(.5f, 0);
            _ScreenSizeText.rectTransform.anchorMax = new Vector2(.5f, 0);
            _ScreenSizeText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _ScreenSizeText.rectTransform.anchoredPosition = new Vector2(_ScreenCoordsText.rectTransform.anchoredPosition.x,
                _ScreenCoordsText.rectTransform.anchoredPosition.y + _ScreenSizeText.rectTransform.sizeDelta.y);

            _LastUiRectransform = _ScreenSizeText.rectTransform;
        }

        private void InitUiRemoteMode()
        {
            Vector2 anchor = new Vector2(-(_EyeTribeFpsText.rectTransform.sizeDelta.x / 2) - 5, -10);

            _EyeTribeFpsText.rectTransform.anchorMin = new Vector2(1, 1);
            _EyeTribeFpsText.rectTransform.anchorMax = new Vector2(1, 1);
            _EyeTribeFpsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _EyeTribeFpsText.rectTransform.anchoredPosition = anchor;

            _GameFPSText.rectTransform.anchorMin = new Vector2(1, 1);
            _GameFPSText.rectTransform.anchorMax = new Vector2(1, 1);
            _GameFPSText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _GameFPSText.rectTransform.anchoredPosition = new Vector2(-(_GameFPSText.rectTransform.sizeDelta.x / 2) - 5,
                _EyeTribeFpsText.rectTransform.anchoredPosition.y - _GameFPSText.rectTransform.sizeDelta.y);

            _ScreenCoordsText.rectTransform.anchorMin = new Vector2(1, 1);
            _ScreenCoordsText.rectTransform.anchorMax = new Vector2(1, 1);
            _ScreenCoordsText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _ScreenCoordsText.rectTransform.anchoredPosition = new Vector2(-(_ScreenCoordsText.rectTransform.sizeDelta.x / 2) - 5,
                _GameFPSText.rectTransform.anchoredPosition.y - _ScreenCoordsText.rectTransform.sizeDelta.y);

            _ScreenSizeText.rectTransform.anchorMin = new Vector2(1, 1);
            _ScreenSizeText.rectTransform.anchorMax = new Vector2(1, 1);
            _ScreenSizeText.rectTransform.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            _ScreenSizeText.rectTransform.anchoredPosition = new Vector2(-(_ScreenSizeText.rectTransform.sizeDelta.x / 2) - 5,
                _ScreenCoordsText.rectTransform.anchoredPosition.y - _ScreenSizeText.rectTransform.sizeDelta.y);

            _LastUiRectransform = _ScreenSizeText.rectTransform;
        }

        public RectTransform GetLastUiRectTransform() 
        {
            return _LastUiRectransform;
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

            if(!GazeManager.Instance.IsCalibrating)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || 
                    Input.GetKeyDown(KeyCode.Joystick1Button2)
                    )
                {
                    ToggleIndicatorMode();
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    ToggleSmoothMode();
                }

                if (Input.GetKeyDown(KeyCode.Alpha3) || 
                    Input.GetKeyDown(KeyCode.Joystick1Button3)
                    )
                {
                    ToggleDebugMode();
                }
            }
        }

        public void ToggleIndicatorMode()
        {
            _ShowGazeIndicator = !_ShowGazeIndicator;

            _GazeReticle.enabled = _ShowGazeIndicator;

            if (!_GazeReticle.enabled)
                _GazeReticle.Hide();

            if (null != OnIndicatorModeToggle)
                OnIndicatorModeToggle(ShowGazeIndicator);
        }

        private void ToggleSmoothMode()
        {
            _UseSmoothed = !_UseSmoothed;

            if (null != OnSmoothModeToggle)
                OnSmoothModeToggle(UseSmoothed);
        }

        public void ToggleDebugMode()
        {
            _ShowDebug = !_ShowDebug;

            _EyeTribeFpsText.rectTransform.SetRendererEnabled(ShowDebug);
            _GameFPSText.rectTransform.SetRendererEnabled(ShowDebug);
            _ScreenCoordsText.rectTransform.SetRendererEnabled(ShowDebug);
            _ScreenSizeText.rectTransform.SetRendererEnabled(ShowDebug);

            if (null != OnDebugModeToggle)
                OnDebugModeToggle(ShowDebug);
        }

        private IEnumerator GazeCoordUpdater()
        {
            while (enabled)
            {
                Point3D gazeVec = GazeFrameCache.Instance.GetLastGazeVectorAvg();
                String coords = "";

                if (Point3D.Zero != gazeVec)
                {
                    coords = "RAY: " + gazeVec.ToVec3().ToString("0.00");
                }
                else 
                {
                    // If no gaze vec, define ray based on 2D gaze coordinates
                    Point2D gazeCoords;
                    if (UseSmoothed)
                        gazeCoords = GazeFrameCache.Instance.GetLastSmoothedGazeCoordinates();
                    else
                        gazeCoords = GazeFrameCache.Instance.GetLastRawGazeCoordinates();

                    if (Point2D.Zero != gazeCoords)
                    {
                        // update gaze coords ui
                        Point2D gazeCoordsToUi = Point2D.Zero != gazeCoords ? gazeCoords : Point2D.Zero;
                        coords = (UseSmoothed ? "SMOOTH: " : "RAW: ") + gazeCoordsToUi.X.ToString("0.00") + ", " + gazeCoordsToUi.Y.ToString("0.00");
                    }
                    else 
                    {
                        coords = (UseSmoothed ? "SMOOTH: " : "RAW: ") + "NA";
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
                _GameFPSText.text = "Game FPS: " + _GameFPS.AvgFPS.ToString("0.00");

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }

        private IEnumerator GazeFPSUpdater()
        {
            while (enabled)
            {
                _EyeTribeFpsText.text = "EyeTribe FPS: " + _GazeFPS.AvgFPS.ToString("0.00");

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }
    }
}
