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
    /// Handles the process of interpolating a Renderer between 2 color states using coroutines.
    /// </summary>
    public class InteractiveColorInterpolator : MonoBehaviour
    {
        [SerializeField]public VRInteractiveItem InteractiveItem;

        [SerializeField]private Color _StartColor = Color.white;

        [SerializeField]private Color _EndColor = Color.red;

        [SerializeField]private float _FadeDuration = 1f;

        private float _FadeTime;

        private IEnumerator _Over;
        private IEnumerator _Out;

        void Awake()
        {
            if (_FadeDuration < 0f)
                throw new Exception("_FadeDuration most be positive!");
        }

        public void Initialize() 
        {
            if (null == InteractiveItem)
                throw new Exception("_InteractiveItem is not set!");

            InteractiveItem.OnOver += HandleOver;
            InteractiveItem.OnOut += HandleOut;
        }

        void OnEnable()
        {
            if (InteractiveItem)
            { 
                InteractiveItem.OnOver += HandleOver;
                InteractiveItem.OnOut += HandleOut;
            }
        }

        void OnDisable()
        {
            InteractiveItem.OnOver -= HandleOver;
            InteractiveItem.OnOut -= HandleOut;
        }

        private void HandleOver()
        {
            if (null != _Out)
                StopCoroutine(_Out);

            StartCoroutine(_Over = FadeOver());
        }

        private void HandleOut()
        {
            if (null != _Over)
                StopCoroutine(_Over);

            StartCoroutine(_Out = FadeOut());
        }

        private IEnumerator FadeOver()
        {
            while (_FadeTime < _FadeDuration)
            {
                _FadeTime += Time.deltaTime;

                GetComponent<Renderer>().material.color = Color.Lerp(_StartColor, _EndColor, _FadeTime / _FadeDuration);

                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            while (_FadeTime - Time.deltaTime > 0)
            {
                _FadeTime -= Time.deltaTime;

                GetComponent<Renderer>().material.color = Color.Lerp(_StartColor, _EndColor, _FadeTime / _FadeDuration);

                yield return null;
            }

            _FadeTime = 0;
        }
    }
}
