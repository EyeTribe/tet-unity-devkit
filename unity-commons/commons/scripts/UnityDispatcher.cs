/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Helper class for dispatching Actions on the Unity main thread.
    /// <para/>
    /// Useful for multi-thread applications.
    /// </summary>
    public class UnityDispatcher : MonoBehaviour
    {
        private Queue<Action> _ActionQueue;

        void OnEnable() 
        {
            if(null == _ActionQueue)
                _ActionQueue = new Queue<Action>();
        }

        void Update()
        {
            if (null != _ActionQueue)
            { 
                lock (_ActionQueue)
                {
                    while (_ActionQueue.Count > 0)
                        _ActionQueue.Dequeue()();
                }
            }
        }

        public void Dispatch(Action action)
        {
            if (null != _ActionQueue)
            {
                lock (_ActionQueue)
                {
                    _ActionQueue.Enqueue(action);
                }
            }
        }
    }
}
