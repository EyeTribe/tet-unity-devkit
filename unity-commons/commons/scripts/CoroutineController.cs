/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using System;

namespace EyeTribe.Unity
{
    public class CoroutineController
    {
        public event Action OnFinish;

        private IEnumerator _Routine;
        private Coroutine _Coroutine;
        private CoroutineState _State;
        private MonoBehaviour _Behaviour;

        public CoroutineController(IEnumerator routine)
        {
            _Routine = routine;
            _State = CoroutineState.Ready;
        }

        public void StartCoroutine(MonoBehaviour monoBehaviour)
        {
            _Behaviour = monoBehaviour;
            _Coroutine = _Behaviour.StartCoroutine(Start());
        }

        private IEnumerator Start()
        {
            if (_State != CoroutineState.Ready)
                throw new System.InvalidOperationException("Unable to start coroutine in state: " + _State);

            _State = CoroutineState.Running;
            while (_Routine.MoveNext())
            {
                yield return _Routine.Current;
                while (_State == CoroutineState.Paused)
                {
                    yield return null;
                }
                if (_State == CoroutineState.Finished)
                {
                    yield break;
                }
            }

            _State = CoroutineState.Finished;

            if (OnFinish != null)
                OnFinish();

            //_Behaviour.StopCoroutine(_Routine);
        }

        public void Stop()
        {
            if (_State != CoroutineState.Running && _State != CoroutineState.Paused)
                throw new System.InvalidOperationException("Unable to stop coroutine in state: " + _State);

            _State = CoroutineState.Finished;
        }

        public void Pause()
        {
            if (_State != CoroutineState.Running)
                throw new System.InvalidOperationException("Unable to pause coroutine in state: " + _State);

            _State = CoroutineState.Paused;
        }

        public void Resume()
        {
            if (_State != CoroutineState.Paused)
                throw new System.InvalidOperationException("Unable to resume coroutine in state: " + state);

            _State = CoroutineState.Running;
        }

        public CoroutineState state
        {
            get { return _State; }
        }

        public Coroutine coroutine
        {
            get { return _Coroutine; }
        }

        public IEnumerator routine
        {
            get { return _Routine; }
        }
    }

    public enum CoroutineState
    {
        Ready,
        Running,
        Paused,
        Finished
    }
}

