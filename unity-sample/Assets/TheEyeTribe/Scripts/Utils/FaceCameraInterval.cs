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
using EyeTribe.Unity;
using System;
using EyeTribe.ClientSdk.Data;
using EyeTribe.Unity.Calibration;
using VRStandardAssets.Utils;

namespace EyeTribe.Unity
{    
    /// <summary>
    /// Ensure that GO always face camera
    /// </summary>
    public class FaceCameraInterval : MonoBehaviour
    {
        [SerializeField] private Camera _Camera;

        [SerializeField] private int _UpdatesPerSecond = 5;
        private float _UpdateDelaySeconds;

        private IEnumerator _DirectionUpdater;

        void Awake()
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (_UpdatesPerSecond <= 0)
                throw new Exception("UpdatesPerSecond must be a positive number");

            _UpdateDelaySeconds = 1f / _UpdatesPerSecond;
        }

        void OnEnable()
        {
            StartCoroutine(_DirectionUpdater = FaceCamera());
        }

        void OnDisable()
        {
            StopCoroutine(_DirectionUpdater);
        }

        private IEnumerator FaceCamera()
        {
            while (enabled)
            {
                transform.LookAt(_Camera.transform.position);

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }
    }
}