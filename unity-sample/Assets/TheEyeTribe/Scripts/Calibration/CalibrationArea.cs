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
using UnityEngine.Rendering;
using EyeTribe.Unity;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles size of rectangular calibration area in remote and VR modes
    /// </summary>
    public class CalibrationArea : MonoBehaviour
    {
        [SerializeField] protected Camera _Camera;

        [SerializeField] protected GameObject _CalibArea;
        [SerializeField] protected Canvas _CalibAreaCanvas;
        [SerializeField] protected Text _CalibAreaInfo;

        [Range(0f, 100f)]
        [SerializeField] protected float _CalibAreaDegreeIncrement = .5f;

        [Range(0f, 360f)]
        protected float _CalibAreaSizeFovDegreeX;
        [Range(0f, 360f)]
        protected float _CalibAreaSizeFovDegreeY;

        [Range(0f, 1f)]
        protected float _CalibAreaSizeRelativeX;
        [Range(0f, 1f)]
        protected float _CalibAreaSizeRelativeY;

        protected bool _IsInitialized;
        protected bool _ShouldUpdate;

        public float CalibAreaSizeRelativeX {
            /// <summary>
            /// Returns the width of the calibration area relative to viewport dimensions at a
            /// given distance.
            /// </para>
            /// Distance is defined by this GOs relative distance to camera.
            /// </summary>
            get
            {
                if (!_IsInitialized)
                    UpdateCalibAreaSize();
                return _CalibAreaSizeRelativeX;
            }
            internal set 
            {
                _CalibAreaSizeRelativeX = value;
            }
        }
        public float CalibAreaSizeRelativeY {
            /// <summary>
            /// Returns the height of the calibration area relative to viewport dimensions at a
            /// given distance.
            /// </para>
            /// Distance is defined by this GOs relative distance to camera.
            /// </summary>
            get
            {
                if (!_IsInitialized)
                    UpdateCalibAreaSize();
                return _CalibAreaSizeRelativeY;
            }
            internal set
            {
                _CalibAreaSizeRelativeY = value;
            }
        }

        public float CalibAreaSizeFovDegreeX
        {
            /// <summary>
            /// Returns the width of the calibration area in fov degrees.
            /// </summary>
            get
            {
                if (!_IsInitialized)
                    UpdateCalibAreaSize();
                return _CalibAreaSizeFovDegreeX;
            }
            internal set 
            {
                _CalibAreaSizeFovDegreeX = value;
            }
        }

        public float CalibAreaSizeFovDegreeY
        {
            /// <summary>
            /// Returns the height of the calibration area in fov degrees.
            /// </summary>
            get
            {
                if (!_IsInitialized)
                    UpdateCalibAreaSize();
                return _CalibAreaSizeFovDegreeY;
            }
            internal set
            {
                _CalibAreaSizeFovDegreeY = value;
            }
        }

        void Awake() 
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _CalibArea)
                throw new Exception("_CalibArea is not set!");

            if (null == _CalibAreaCanvas)
                throw new Exception("_CalibAreaCanvas is not set!");

            if (null == _CalibAreaInfo)
                throw new Exception("_CalibAreaInfo is not set!");

            if (_CalibAreaDegreeIncrement < float.Epsilon || _CalibAreaDegreeIncrement > 100f)
                throw new Exception("_CalibAreaDegreeIncrement must be between 0-1 degrees!");

            _CalibArea.gameObject.SetActive(false);
            _CalibAreaInfo.enabled = false;
        }

        void OnEnable()
        {
            CreateMesh();

            UpdateCalibAreaSize();
        }

        void OnDisable()
        {
            ClearChildren(_CalibArea.transform);
        }

        void OnValidate() 
        {
            if (_IsInitialized)
            {
                _ShouldUpdate = true;
            }
        }

        protected void ClearChildren(Transform parent) 
        {
            foreach (Transform child in parent)
            {
                if (Application.isEditor)
                    GameObject.DestroyImmediate(child.gameObject);
                else
                    GameObject.Destroy(child.gameObject);
            }
        }

        protected virtual void CreateMesh()
        {
            ClearChildren(_CalibArea.transform);

            GameObject rect = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Collider c = rect.GetComponent<Collider>();
            c.enabled = false;
            Renderer r = rect.GetComponent<Renderer>();
            r.material = Resources.Load<Material>("Materials/calib_area_bg");
            r.receiveShadows = false;
            r.shadowCastingMode = ShadowCastingMode.Off;
            rect.transform.parent = _CalibArea.transform;
            rect.transform.localPosition = Vector3.zero;
            rect.transform.localRotation = Quaternion.identity;
            rect.transform.localScale = Vector3.one;
        }

        void Update() 
        {
            if (_ShouldUpdate)
            {
                _ShouldUpdate = false;

                CreateMesh();

                UpdateCalibAreaSize();
            }

            HandleInput();
        }

        protected virtual void HandleInput()
        {
            bool updateCalibArea = false;

            if (!GazeManager.Instance.IsCalibrating)
            {
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    _CalibArea.gameObject.SetActive(!_CalibArea.gameObject.activeInHierarchy);
                    _CalibAreaInfo.enabled = _CalibArea.gameObject.activeInHierarchy;
                }

                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    if (CalibAreaSizeFovDegreeX - _CalibAreaDegreeIncrement > _CalibAreaDegreeIncrement)
                        CalibAreaSizeFovDegreeX -= _CalibAreaDegreeIncrement;

                    updateCalibArea = true;
                }

                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    if (CalibAreaSizeFovDegreeX + _CalibAreaDegreeIncrement < _Camera.fieldOfView)
                        CalibAreaSizeFovDegreeX += _CalibAreaDegreeIncrement;

                    updateCalibArea = true;
                }

                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    if (CalibAreaSizeFovDegreeY - _CalibAreaDegreeIncrement > _CalibAreaDegreeIncrement)
                        CalibAreaSizeFovDegreeY -= _CalibAreaDegreeIncrement;

                    updateCalibArea = true;
                }

                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    if (CalibAreaSizeFovDegreeY + _CalibAreaDegreeIncrement < _Camera.fieldOfView)
                        CalibAreaSizeFovDegreeY += _CalibAreaDegreeIncrement;

                    updateCalibArea = true;
                }

                if (updateCalibArea)
                    UpdateCalibAreaSize();
            }
        }

        protected void UpdateCalibAreaSize()
        {
            if (_Camera.isActiveAndEnabled)
            { 
                Vector2 worldScreen = _Camera.GetPerspectiveWorldScreenBounds(_CalibArea.transform.localPosition.z);

                if (!_IsInitialized)
                {
                    // If _Camera not initialized, we abort update
                    if (float.IsNaN(worldScreen.x) || float.IsNaN(worldScreen.y))
                        return;

                    float paddingDegrees = 8f;
                    _CalibAreaSizeFovDegreeX = _Camera.fieldOfView - paddingDegrees;
                    _CalibAreaSizeFovDegreeY = (_Camera.fieldOfView / (_Camera.pixelWidth / _Camera.pixelHeight)) - 8f;

                    // mode specific adjustments
                    if (VRMode.IsRunningInVRMode)
                    {
                        _CalibAreaSizeFovDegreeX = 16f;
                        _CalibAreaSizeFovDegreeY = _CalibAreaSizeFovDegreeX * _Camera.aspect;
                    }

                    _IsInitialized = true;
                }

                CalibAreaSizeRelativeX = _CalibAreaSizeFovDegreeX / _Camera.fieldOfView;
                CalibAreaSizeRelativeY = _CalibAreaSizeFovDegreeY / _Camera.fieldOfView;

                float localSizeX = worldScreen.x * CalibAreaSizeRelativeX;
                float localSizeY = worldScreen.y * CalibAreaSizeRelativeY;

                _CalibArea.transform.localScale = new Vector3(
                    localSizeX,
                    localSizeY,
                    _CalibArea.transform.localScale.z
                    );

                _CalibAreaCanvas.transform.localPosition =  new Vector3(
                    -localSizeX *.5f,
                    localSizeY * .5f,
                    _CalibArea.transform.localPosition.z
                    );

                _CalibAreaInfo.text = 
                    CalibAreaSizeFovDegreeX.ToString("0.00") + " x " + CalibAreaSizeFovDegreeY.ToString("0.00") + " °\n" +
                    (CalibAreaSizeRelativeX * 100).ToString("0.00") + " x " + (CalibAreaSizeRelativeY * 100).ToString("0.00") + " %\n" +
                    localSizeX.ToString("0.00") + " x " + localSizeY.ToString("0.00") + " m";

                _GizmosDrawn = false;
            }
        }

        bool _GizmosDrawn;
        List<Point2D> _CalibGizmo;
        void OnDrawGizmos()
        {
            if (_CalibArea.gameObject.activeInHierarchy)
            {
                if (!_GizmosDrawn)
                {
                    _GizmosDrawn = true;

                    _CalibGizmo = GetCalibrationPoints(3, 3);
                }

                if (null != _CalibGizmo)
                {
                    Gizmos.color = Color.yellow;
                    foreach (Point2D p in _CalibGizmo)
                    {
                        Vector3 pos = _Camera.ViewportToWorldPoint(new Vector3(p.X / _Camera.pixelWidth, p.Y / _Camera.pixelHeight, 3));
                        Gizmos.DrawSphere(pos, 0.05f);
                    }

                    Vector3 pos2 = _Camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 3));
                    Gizmos.DrawSphere(pos2, 0.05f);
                }
            }
        }

        public virtual List<Point2D> GetCalibrationPoints(int rows, int cols)
        {
            float width = _Camera.pixelWidth;
            float height = _Camera.pixelHeight;
            double calibWidth = Math.Round(width * CalibAreaSizeRelativeX);
            double calibHeight = Math.Round(height * CalibAreaSizeRelativeY);
            double paddingHors = Math.Round((width - calibWidth) * .5f);
            double paddingVert = Math.Round((height - calibHeight) * .5f);

            return UnityCalibUtils.InitCalibrationPoints(
                rows,
                cols,
                width, height,
                paddingHors, paddingVert);
        }
    }
}
