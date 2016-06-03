/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using EyeTribe.ClientSdk.Data;
using EyeTribe.Unity;
using EyeTribe.ClientSdk;
using System;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles the UI associated to displaying eye pupils
    /// </summary>
    public class EyeUI : MonoBehaviour
    {
        [SerializeField]private Camera _Camera;

        [SerializeField]private GameObject _LeftEye;
        [SerializeField]private GameObject _RightEye;

        [SerializeField]private float _EyeScaleInitSize = 1f;

        private double _EyesDistance;
        private Vector3 _EyeBaseScale;
        private double _DepthMod;

        private Eye _LastLeftEye;
        private Eye _LastRightEye;

        void Awake()
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _LeftEye)
                throw new Exception("_LeftEye is not set!");

            if (null == _RightEye)
                throw new Exception("_RightEye is not set!");
        }

        void OnEnable()
        {
            // Only use in 'remote' mode
            if (VRMode.IsRunningInVRMode())
                gameObject.SetActive(false);

            _LeftEye.SetRendererEnabled(true);
            _RightEye.SetRendererEnabled(true);
            _EyeBaseScale = _LeftEye.transform.localScale;
        }

        void Update()
        {
            if (!Application.isPlaying)
                return;

            if (!VRMode.IsRunningInVRMode())
            {
                if (!GazeManager.Instance.IsCalibrating)
                {
                    // If running in 'remote' mode and not calibrating, we position eyes
                    // and set size based on distance

                    _EyesDistance = GazeFrameCache.Instance.GetLastUserPosition().Z;

                    _DepthMod = (1 - _EyesDistance) * .25f;
                    Vector3 scaleVec = new Vector3((float)(_DepthMod), (float)(_DepthMod), (float)_EyeBaseScale.z);

                    Eye left = GazeFrameCache.Instance.GetLastLeftEye();
                    Eye right = GazeFrameCache.Instance.GetLastRightEye();

                    double angle = -GazeFrameCache.Instance.GetLastEyesAngle();

                    if (null != left)
                    {
                        if (!left.Equals(_LastLeftEye))
                        {
                            _LastLeftEye = left;

                            if (!_LeftEye.IsRendererEnabled())
                                _LeftEye.SetRendererEnabled(true);

                            //position GO based on screen coordinates
                            Point2D gp = UnityGazeUtils.GetRelativeToScreenSpace(left.PupilCenterCoordinates);
                            _LeftEye.SetWorldPositionFromGaze(_Camera, gp, _LeftEye.transform.localPosition.z);
                            _LeftEye.transform.localScale = scaleVec * _EyeScaleInitSize;
                            _LeftEye.transform.localEulerAngles = new Vector3(_LeftEye.transform.localEulerAngles.x, _LeftEye.transform.localEulerAngles.y, (float)-angle);
                        }
                    }
                    else
                    {
                        if (_LeftEye.IsRendererEnabled())
                            _LeftEye.SetRendererEnabled(false);
                    }

                    if (null != right)
                    {
                        if (!right.Equals(_LastRightEye))
                        {
                            _LastRightEye = right;

                            if (!_RightEye.IsRendererEnabled())
                                _RightEye.SetRendererEnabled(true);

                            //position GO based on screen coordinates
                            Point2D gp = UnityGazeUtils.GetRelativeToScreenSpace(right.PupilCenterCoordinates);
                            _RightEye.SetWorldPositionFromGaze(_Camera, gp, _RightEye.transform.localPosition.z);
                            _RightEye.transform.localScale = scaleVec * _EyeScaleInitSize;
                            _RightEye.transform.localEulerAngles = new Vector3(_RightEye.transform.localEulerAngles.x, _RightEye.transform.localEulerAngles.y, (float)-angle);
                        }
                    }
                    else
                    {
                        if (_RightEye.IsRendererEnabled())
                            _RightEye.SetRendererEnabled(false);
                    }
                }
            }
            else
            {
                if (_LeftEye.IsRendererEnabled())
                    _LeftEye.SetRendererEnabled(false);
                if (_RightEye.IsRendererEnabled())
                    _RightEye.SetRendererEnabled(false);
            }
        }
    }
}
