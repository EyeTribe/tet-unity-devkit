/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System.Collections;
using UnityEngine;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Aligns GO with height of camera
    /// </summary>
    public class AlignWithCameraY : MonoBehaviour
    {
        [SerializeField] private Camera _Camera;

        private Vector3 _LastCamPos;

        private IEnumerator CamPosChecker;

        void Awake()
        {
            if (null == _Camera)
                Debug.LogError("_Camera is not set!");
        }

        void OnEnable()
        {
            _LastCamPos = _Camera.transform.position;

            StartCoroutine(CamPosChecker = AlignToCam());
        }

        void OnDisable()
        {
            StopCoroutine(CamPosChecker);
        }

        private IEnumerator AlignToCam()
        {
            while(enabled)
            {
                if (!_LastCamPos.Equals(_Camera.transform.position))
                {
                    _LastCamPos = _Camera.transform.position;
                    transform.position = new Vector3(transform.position.x, _LastCamPos.y, transform.position.z);
                }
            
                yield return new WaitForSeconds(.5f);
            }
        }
    }
}
