/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Text;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.Unity
{ 
    /// <summary>
    /// Extending GazeDataDeque to support calculation of frame rate
    /// </summary>
    public class FrameRateGazeDataQueue : GazeDataQueue
    {
        private readonly static long BUFFER_SIZE_MILLIS = 5000;

        public FrameRateGazeDataQueue()
            : base(BUFFER_SIZE_MILLIS)
        {
        }
        
        public FrameRateGazeDataQueue(long timeLimit)
            : base(timeLimit)
        {
        }

        public float GetAvgFramesPerSecond()
        {
            float avgMillis;
            if ((avgMillis = GetAvgMillisFrame()) > 0)
                return 1000f / avgMillis;

            return -1;
        }

        public float GetAvgMillisFrame()
        {
            if (this.Count() > 0)
            {
                GazeData first = this.First();
                GazeData last = this.Last();

                if (null != first && null != last)
                {
                    float delta = last.TimeStamp - first.TimeStamp;

                    // only return value when buffer populated
                    if (delta > (TimeLimit >> 1))
                        return delta / this.Count();
                }
            }

            return -1;
        }
    }
}
