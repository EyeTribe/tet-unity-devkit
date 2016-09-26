/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using VRStandardAssets.Utils;

namespace EyeTribe.Unity.Interaction
{
    /// <summary>
    /// Handles the process of interpolating a GO world position between 2 rotation states.
    /// </summary>
    public class RotationInterpolator : MonoBehaviour
    {
        [SerializeField] private bool _IsRotating;
        [SerializeField] private bool _IsEased;
        [SerializeField] private bool _ShouldDisableCollision;
        private IEnumerator _Coroutine;

        public bool IsRotating { get { return _IsRotating; } }

        public bool IsEased { get { return _IsEased; } set { _IsEased = value; } }

        public bool ShouldDisableCollision { get { return _ShouldDisableCollision; } set { _ShouldDisableCollision = value; } }

        public void RotateTo(Quaternion newRot, float duration)
        {
            if (null != _Coroutine)
                StopCoroutine(_Coroutine);

            StartCoroutine(_Coroutine = RotateCoroutine(transform.rotation, newRot, duration));
        }

        void OnDisable()
        {
            if (null != _Coroutine)
                StopCoroutine(_Coroutine);
        }

        private IEnumerator RotateCoroutine(Quaternion start, Quaternion end, float duration)
        {
            float t = 0f;
            _IsRotating = true;

            if (ShouldDisableCollision)
                EnableColliders(false);

            while (t <= 1.0)
            {
                t += Time.deltaTime / duration;
                if (IsEased)
                    transform.rotation = Quaternion.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                else
                    transform.rotation = Quaternion.Lerp(start, end, t);
                yield return null;
            }

            if (ShouldDisableCollision)
                EnableColliders(true);

            _IsRotating = false;
        }

        private void EnableColliders(bool enable)
        {
            Collider[] cols = GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
                c.enabled = enable;  
        }
    }
}
