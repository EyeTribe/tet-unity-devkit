/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Prefab script. Measures average game cycle FPS based on number of Update() calls.
    /// </summary>
    public class GameFPS : MonoBehaviour 
    {
        [SerializeField]private int _SampleFrames = 30;

        private FixedSizedQueue<float> _FrameTimes;
 
        public float AvgFPS 
        { 
            get
            {
                if (null != _FrameTimes && _FrameTimes.Count() >= (_SampleFrames - 1))
                {
                    return 1f / _FrameTimes.Average();
                }

                return 0f;
            }
        }

        void OnEnable() 
        {
            if (null == _FrameTimes)
                _FrameTimes = new FixedSizedQueue<float>(_SampleFrames);
        }
 
        void Update () 
        {
            _FrameTimes.Enqueue(Time.deltaTime);
        }
     }
}