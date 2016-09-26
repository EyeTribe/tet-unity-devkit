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
using System.Collections.Generic;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles the UI associated to fading a notification Text object in and out
    /// over a fixed timeframe.
    /// </para>
    /// Notifications shown in the order they are called.
    /// </summary>
    public class StateNotifyer : MonoBehaviour
    {
        [SerializeField]
        private Text _State;

        [SerializeField]
        private Color _StartColor = Color.white;

        [SerializeField]
        private Color _EndColor = new Color(1f, 1f, 1f, 0f);

        [SerializeField]
        private float _FadeTime = .25f;

        [SerializeField]
        private float _Duration = 1.5f;

        private Queue<StateMessage> _MessageQueue;

        private bool _ShowingMessage;

        void Awake()
        {
            if (null == _State)
                throw new Exception("_State is not set!");

            if (_FadeTime < float.Epsilon)
                throw new Exception("_FadeTime must be positive!");

            if (_Duration < float.Epsilon)
                throw new Exception("_Duration must be positive!");

            _State.rectTransform.SetRendererEnabled(false);
            _State.color = _StartColor;

            _MessageQueue = new Queue<StateMessage>();
        }

        void Update()
        {
            if (!_ShowingMessage)
            {
                if (null != _MessageQueue && _MessageQueue.Count > 0)
                {
                    StartStateCoroutine(_MessageQueue.Dequeue());
                }
            }
        }

        private void StartStateCoroutine(StateMessage sm)
        {
            _ShowingMessage = true;
            _State.text = sm.Message;
            StartCoroutine(FadeIn(sm));
        }

        public void ShowState(string text)
        {
            ShowState(_FadeTime, _Duration, text);
        }

        public void ShowState(float fadeTime, float duration, string text)
        {
            _MessageQueue.Enqueue(new StateMessage(fadeTime, duration, text));
        }

        private IEnumerator FadeIn(StateMessage sm)
        {
            _State.rectTransform.SetRendererEnabled(true);

            for (float t = 0f; t < sm.FadeTime; t += Time.deltaTime)
            {
                _State.color = Color.Lerp(_StartColor, _EndColor, t / sm.FadeTime);

                yield return null;
            }

            _State.color = _EndColor;

            StartCoroutine(Timer(sm));
        }

        private IEnumerator FadeOut(StateMessage sm)
        {
            for (float t = 0f; t < sm.FadeTime; t += Time.deltaTime)
            {
                _State.color = Color.Lerp(_EndColor, _StartColor, t / sm.FadeTime);

                yield return null;
            }

            _State.color = _StartColor;
            _State.rectTransform.SetRendererEnabled(false);

            _ShowingMessage = false;
        }

        private IEnumerator Timer(StateMessage sm)
        {
            yield return new WaitForSeconds(sm.Duration);

            StartCoroutine(FadeOut(sm));
        }

        private struct StateMessage
        {
            public float FadeTime;
            public float Duration;
            public string Message;

            public StateMessage(float fadeTime, float duration, string text)
            {
                FadeTime = fadeTime;
                Duration = duration;
                Message = text;
            }
        }
    }
}
