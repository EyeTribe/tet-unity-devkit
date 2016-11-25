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
    public class InteractiveColorInterpolator : MonoBehaviour
    {
        [SerializeField]public VRInteractiveItem InteractiveItem;

        [SerializeField] public Color StartColor = Color.white;

        [SerializeField] public Color EndColor = Color.red;

        [SerializeField] private float _FadeDuration = 1f;

        private float _FadeTime;

        private IEnumerator _Over;
        private IEnumerator _Out;

        private bool _ShouldDie;

        void Awake()
        {
            if (_FadeDuration < 0f)
                throw new Exception("_FadeDuration most be positive!");

            if (null != InteractiveItem)
                Initialize();
        }

        public void Initialize()
        {
            if (null == InteractiveItem)
                throw new Exception("_InteractiveItem is not set!");

            GetComponent<Renderer>().material.color = StartColor;

            InteractiveItem.OnOver += HandleOver;
            InteractiveItem.OnOut += HandleOut;
        }

        void OnEnable()
        {
            GetComponent<Renderer>().material.color = StartColor;

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

        void OnCollisionEnter(Collision collision)
        {
            // When faded out next, script will die
            _ShouldDie = true;
        }

        private IEnumerator FadeOver()
        {
            while (_FadeTime < _FadeDuration)
            {
                _FadeTime += Time.deltaTime;

                GetComponent<Renderer>().material.color = Color.Lerp(StartColor, EndColor, _FadeTime / _FadeDuration);

                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            while (_FadeTime - Time.deltaTime > 0)
            {
                _FadeTime -= Time.deltaTime;

                GetComponent<Renderer>().material.color = Color.Lerp(StartColor, EndColor, _FadeTime / _FadeDuration);

                yield return null;
            }

            _FadeTime = 0;

            if (_ShouldDie)
            {
                // If GO hit by other go, we disable this script
                enabled = false;
                Destroy(this);
            }
        }
    }
}
