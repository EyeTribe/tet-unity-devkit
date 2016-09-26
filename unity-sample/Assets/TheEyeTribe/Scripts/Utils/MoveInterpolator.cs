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
    /// Handles the process of interpolating a GO world position between 2 positions.
    /// </summary>
    public class MoveInterpolator : MonoBehaviour
    {
        [SerializeField] private Transform _Transform;
        [SerializeField] private bool _IsMoving;
        [SerializeField] private bool _IsEased;
        [SerializeField] private bool _ShouldDisableCollision;

        public bool UseLocalTransform;
        private IEnumerator _Coroutine;

        public bool IsMoving { get { return _IsMoving; } }

        public Transform MoveTransform { get { return _Transform; } set { _Transform = value; } }

        public bool IsEased { get { return _IsEased; } set { _IsEased = value; } }

        public bool ShouldDisableCollision { get { return _ShouldDisableCollision; } set { _ShouldDisableCollision = value; } }

        void Awake()
        {
            if (null == _Transform)
                Debug.LogWarning("_Transform is not set!");
        }

        public void MoveTo(Vector3 newPos, float duration)
        {
            if (null != _Coroutine)
                StopCoroutine(_Coroutine);

            StartCoroutine(_Coroutine = MoveCoroutine(UseLocalTransform ? _Transform.localPosition : _Transform.position, newPos, duration));
        }

        void OnDisable()
        {
            if (null != _Coroutine)
                StopCoroutine(_Coroutine);
        }

        private IEnumerator MoveCoroutine(Vector3 start, Vector3 end, float duration)
        {
            float t = 0f;
            _IsMoving = true;

            if (ShouldDisableCollision)
                EnableColliders(false);

            while (t <= 1.0)
            {
                t += Time.deltaTime / duration;
                if (IsEased)
                    if (UseLocalTransform)
                        _Transform.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                    else
                        _Transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                else
                    if (UseLocalTransform)
                        _Transform.localPosition = Vector3.Lerp(start, end, t);
                    else
                        _Transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            if (ShouldDisableCollision)
                EnableColliders(true);

            _IsMoving = false;
        }

        private void EnableColliders(bool enable)
        {
            Collider[] cols = GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
                c.enabled = enable;  
        }
    }
}
