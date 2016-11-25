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

namespace EyeTribe.Unity.Interaction
{
    /// <summary>
    /// Handles the process of interpolating a Renderer between 2 color states using coroutines.
    /// </summary>
    public class InteractiveShakeAndFire : MonoBehaviour
    {
        [SerializeField] public VRInteractiveItem InteractiveItem;
        [SerializeField] public SelectionRadialEyeTribe Selection;
        [SerializeField] public Transform ReticleTransform;
        [SerializeField] public Rigidbody RigidBody;

        private Vector3 _InitialPos;

        private bool _IsOver;
        private bool _IsShaking;
        private float _Duration;

        private float _ShakeTime;

        private float _ShakeSpeed = 50f;
        private float _ShakeAmount = 0.05f;

        private float _ImpulseForce = 15f;

        private IEnumerator _ShakeUp;
        private IEnumerator _ShakeDown;

        private static int i;

        public void Initialize() 
        {
            if (null == InteractiveItem)
                throw new Exception("InteractiveItem is not set!");

            if (null == Selection)
                throw new Exception("SelectionRadialEyeTribe is not set!");

            if (null == ReticleTransform)
                throw new Exception("ReticleTransform is not set!");

            if (null == RigidBody)
                throw new Exception("RigidBody is not set!");

            _Duration = Selection.SelectionDuration;

            RigidBody.useGravity = false;

            InteractiveItem.OnOver += HandleOver;
            InteractiveItem.OnOut += HandleOut;

            SelectionRadialEyeTribe.OnSelectionStarted += ShakeStart;
            SelectionRadialEyeTribe.OnSelectionAborted += ShakeAbort;
            SelectionRadialEyeTribe.OnSelectionComplete += Fire;
        }

        void OnEnable()
        {
            /*
            if (InteractiveItem)
            { 
                InteractiveItem.OnOver += HandleOver;
                InteractiveItem.OnOut += HandleOut;
            }

            if (SelectionRadialEyeTribe)
            {
                SelectionRadialEyeTribe.OnSelectionStarted += ShakeStart;
                SelectionRadialEyeTribe.OnSelectionAborted += ShakeAbort;
                SelectionRadialEyeTribe.OnSelectionComplete += Fire;
            }
             */
        }

        void OnDisable()
        {
            InteractiveItem.OnOver -= HandleOver;
            InteractiveItem.OnOut -= HandleOut;

            SelectionRadialEyeTribe.OnSelectionStarted -= ShakeStart;
            SelectionRadialEyeTribe.OnSelectionAborted -= ShakeAbort;
            SelectionRadialEyeTribe.OnSelectionComplete -= Fire;
        }

        private void HandleOver()
        {
            _IsOver = true;

            if (Selection.IsFilling)
            {
                if (null != _ShakeDown)
                {
                    StopCoroutine(_ShakeDown);
                    _ShakeDown = null;
                }

                if(null == _ShakeUp)
                    StartCoroutine(_ShakeUp = ShakeUp());
            }   
        }

        private void HandleOut()
        {
            _IsOver = false;

            if (Selection.IsFilling)
            {
                if (null != _ShakeUp)
                {
                    StopCoroutine(_ShakeUp);
                    _ShakeUp = null;
                }

                if (null == _ShakeDown)
                    StartCoroutine(_ShakeDown = ShakeDown());
            }
        }

        private void ShakeStart() 
        {
            if (_IsOver)
            { 
                if (null != _ShakeDown)
                { 
                    StopCoroutine(_ShakeDown);
                    _ShakeDown = null;
                }

                if (null == _ShakeUp)
                    StartCoroutine(_ShakeUp = ShakeUp());
            }
        }

        private void ShakeAbort()
        {
            if (null != _ShakeUp) 
            {
                StopCoroutine(_ShakeUp);
                _ShakeUp = null;
                _IsShaking = false;

                if (null == _ShakeDown)
                    StartCoroutine(_ShakeDown = ShakeDown());
            }
        }

        private void Fire()
        {
            if (_IsShaking && _IsOver)
            {
                _IsShaking = false;
                _IsOver = false;
                _ShakeTime = 0f;
                RigidBody.useGravity = true;

                Vector3 dir = ReticleTransform.position - Camera.main.transform.position;
                dir.Normalize();

                RigidBody.AddForce(dir * _ImpulseForce, ForceMode.Impulse);
                enabled = false;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            // If GO hit by other go, we disable this script
            enabled = false;
            Destroy(this);
        }

        private IEnumerator ShakeUp()
        {
            if(_ShakeTime <= 0f)
                _InitialPos = gameObject.transform.position;

            _IsShaking = true;

            while (_IsShaking && _ShakeTime < _Duration)
            {
                _ShakeTime += Time.deltaTime;

                float shake = (Mathf.Sin(Time.time * _ShakeSpeed) * _ShakeAmount) * (_ShakeTime / _Duration);

                transform.position = _InitialPos + new Vector3(shake,0,0) ;

                yield return null;
            }
        }

        private IEnumerator ShakeDown()
        {
            while (_ShakeTime - Time.deltaTime > 0f)
            {
                _ShakeTime -= Time.deltaTime;

                float shake = (Mathf.Sin(Time.time * _ShakeSpeed) * _ShakeAmount) * (_ShakeTime / _Duration);

                transform.position = _InitialPos + new Vector3(shake, 0, 0);

                yield return null;
            }

            transform.position = _InitialPos;

            _ShakeTime = 0;
        }
    }
}
