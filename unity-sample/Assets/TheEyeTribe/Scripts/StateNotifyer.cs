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

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles the UI associated to fading a notification Text object in and out
    /// over a fixed timeframe
    /// </summary>
    public class StateNotifyer : MonoBehaviour
    {
        [SerializeField]private Text _State;

        private Color _StartColor;

        private Color _EndColor;

        void Awake()
        {
            if (null == _State)
                throw new Exception("_State is not set!");

            _StartColor = _State.color;
            _EndColor = new Color(_StartColor.r, _StartColor.g, _StartColor.b, 0f);

            _State.rectTransform.SetRendererEnabled(false);
        }

        public void ShowState(string text) 
        {
            _State.text = text;

            StartCoroutine(FadeIn(.25f));

            StartCoroutine(Timer(1f));
        }

        private IEnumerator FadeIn(float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                _State.color = Color.Lerp(_EndColor, _StartColor, t / duration);

                if (!_State.rectTransform.IsRendererEnabled())
                    _State.rectTransform.SetRendererEnabled(true);

                yield return null;
            }
        }

        private IEnumerator FadeOut(float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                _State.color = Color.Lerp(_StartColor, _EndColor, t / duration);

                yield return null;
            }

            _State.rectTransform.SetRendererEnabled(false);
        }

        private IEnumerator Timer(float duration)
        {
            yield return new WaitForSeconds(duration);

            StartCoroutine(FadeOut(.25f));
        }
    }
}
