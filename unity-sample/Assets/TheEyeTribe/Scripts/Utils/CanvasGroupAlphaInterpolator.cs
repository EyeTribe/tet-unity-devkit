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

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles the process of interpolating a CanvasGroup between 2 alpha states using coroutines.
    /// </summary>
    public class CanvasGroupAlphaInterpolator : MonoBehaviour {

        public event Action OnFadeOutComplete;
        public event Action OnFadeInComplete;

        [SerializeField] private CanvasGroup _Group;

        [SerializeField] private float _Duration = 1f;

        private float _Time;

        private IEnumerator _Forward;
        private IEnumerator _Backward;

        void Awake()
        {
            if (null == _Group)
                throw new Exception("_Group is not set!");

            if (_Duration < float.Epsilon)
                throw new Exception("_Duration most be positive!");

            _Group.alpha = 0;
        }

        public void FadeIn()
        {
            // Only initiate if not already in end color state
            if (_Group.alpha < 1f)
            { 
                if (null != _Backward)
                    StopCoroutine(_Backward);

                StartCoroutine(_Forward = InterpolateForward());
            }
        }

        public void FadeOut()
        {
            // Only initiate if not already in start color state
            if (_Group.alpha > 0f)
            {
                if (null != _Forward)
                    StopCoroutine(_Forward);

                StartCoroutine(_Backward = InterpolateBackward());
            }
        }

        private IEnumerator InterpolateForward()
        {
            while (_Time < _Duration)
            {
                _Time += Time.deltaTime;

                _Group.alpha = _Time / _Duration;

                yield return null;
            }

            _Group.alpha = 1f;

            if (null != OnFadeInComplete)
                OnFadeInComplete();
        }

        private IEnumerator InterpolateBackward()
        {
            while (_Time - Time.deltaTime > 0)
            {
                _Time -= Time.deltaTime;

                _Group.alpha = _Time / _Duration;

                yield return null;
            }

            _Time = 0;

            _Group.alpha = 0f;

            if (null != OnFadeOutComplete)
                OnFadeOutComplete();
        }
    }
}
