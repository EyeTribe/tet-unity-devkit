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
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Prefab script. Measures average gaze cycle FPS based on number OnGazeUpdate() calls.
    /// </summary>
    public class GazeFPS : EyeTribeUnityScript 
    {
        private FrameRateGazeDataQueue _FpsCache;

        public override void OnEnable()
        {
            base.OnEnable();

            if (null == _FpsCache)
                _FpsCache = new FrameRateGazeDataQueue();
        }
        
        public override void OnGazeUpdate(GazeData gazeData)
        {
            base.OnGazeUpdate(gazeData);
            
            _FpsCache.Enqueue(gazeData);
        }

        public float AvgFPS
        {
            get
            {
                if(null != _FpsCache)
                    return _FpsCache.GetAvgFramesPerSecond();

                return 0f;
            }
        }
     }
}