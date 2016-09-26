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
    /// </para>
    /// Supports recursive interpolation, hence interpolating colors of child nodes
    /// </summary>
    public class ColorInterpolator : MonoBehaviour
    {
        public event Action OnForwardComplete;
        public event Action OnBackwardsComplete;

        [SerializeField] private Renderer _Renderer;

        [SerializeField] private Color _StartColor = Color.white;

        [SerializeField] private Color _EndColor = Color.red;

        [SerializeField] private float _Duration = 1f;

        [SerializeField] private bool _IsRecursive = false;

        private float _Time;

        private IEnumerator _Forward;
        private IEnumerator _Backward;

        void Awake()
        {
            if (null == _Renderer)
                throw new Exception("_Renderer is not set!");

            if (_Duration < float.Epsilon)
                throw new Exception("_Duration most be positive!");
        }

        void OnEnable() 
        {
            Renderer[] renderers = new Renderer[0];

            _Renderer.material.color = _StartColor;

            if (_IsRecursive)
                renderers = _Renderer.gameObject.transform.GetComponentsInChildren<Renderer>();

            if (null != renderers)
                foreach (Renderer r in renderers)
                    r.material.color = _StartColor;
        }

        public void Forward()
        {
            // Only initiate if not already in end color state
            if (_Renderer.material.color != _EndColor)
            { 
                if (null != _Backward)
                    StopCoroutine(_Backward);

                Renderer[] renderers = null;
            
                if(_IsRecursive)
                    renderers = _Renderer.gameObject.transform.GetComponentsInChildren<Renderer>();

                StartCoroutine(_Forward = InterpolateForward(renderers));
            }
        }

        public void Backward()
        {
            // Only initiate if not already in start color state
            if (_Renderer.material.color != _StartColor)
            {
                if (null != _Forward)
                    StopCoroutine(_Forward);

                Renderer[] renderers = null;

                if (_IsRecursive)
                    renderers = _Renderer.gameObject.transform.GetComponentsInChildren<Renderer>();

                StartCoroutine(_Backward = InterpolateBackward(renderers));
            }
        }

        private IEnumerator InterpolateForward(Renderer[] renderers)
        {
            while (_Time < _Duration)
            {
                _Time += Time.deltaTime;

                Color now = Color.Lerp(_StartColor, _EndColor, _Time / _Duration);

                _Renderer.material.color = now;

                if(null != renderers)
                    foreach(Renderer r in renderers)
                        r.material.color = now;

                yield return null;
            }

            _Renderer.material.color = _EndColor;

            if (null != OnForwardComplete)
                OnForwardComplete();
        }

        private IEnumerator InterpolateBackward(Renderer[] renderers)
        {
            while (_Time - Time.deltaTime > 0)
            {
                _Time -= Time.deltaTime;

                Color now = Color.Lerp(_StartColor, _EndColor, _Time / _Duration);

                _Renderer.material.color = now;

                if (null != renderers)
                    foreach (Renderer r in renderers)
                        r.material.color = now;

                yield return null;
            }

            _Time = 0;

            _Renderer.material.color = _StartColor;

            if (null != OnBackwardsComplete)
                OnBackwardsComplete();
        }
    }
}
