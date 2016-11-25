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
    /// Handles the process of interpolating a GO size between 2 scale states using coroutines.
    /// </summary>
    public class InteractiveScaleInterpolator : MonoBehaviour
    {
        [SerializeField] public VRInteractiveItem InteractiveItem;

        [SerializeField] private Vector3 _StartScale = Vector3.one;

        [SerializeField] private Vector3 _EndScale = Vector3.one * 1.1f;

        [SerializeField] private float _ScaleDuration = 1f;

        private float _ScaleTime;

        private IEnumerator _Over;
        private IEnumerator _Out;

        void Awake()
        {
            if (_ScaleDuration < 0f)
                throw new Exception("_ScaleDuration most be positive!");
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
            if (gameObject.activeInHierarchy)
            {
                if (null != _Out)
                    StopCoroutine(_Out);

                StartCoroutine(_Over = FadeOver());
            }
        }

        private void HandleOut()
        {
            if (gameObject.activeInHierarchy)
            {
                if (null != _Over)
                    StopCoroutine(_Over);

                StartCoroutine(_Out = FadeOut());
            }
        }

        private IEnumerator FadeOver()
        {
            while (_ScaleTime < _ScaleDuration)
            {
                _ScaleTime += Time.deltaTime;

                gameObject.transform.localScale = Vector3.Lerp(_StartScale, _EndScale, _ScaleTime / _ScaleDuration);

                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            while (_ScaleTime - Time.deltaTime > 0)
            {
                _ScaleTime -= Time.deltaTime;

                gameObject.transform.localScale = Vector3.Lerp(_StartScale, _EndScale, _ScaleTime / _ScaleDuration);

                yield return null;
            }

            _ScaleTime = 0;
        }
    }
}
