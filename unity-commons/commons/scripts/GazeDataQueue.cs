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
    /// Structure holding latest valid GazeData objects. Based on a time limit, the queue 
    /// size is moderated as new items are added.
    /// </summary>
    public class GazeDataQueue : ConcurrentQueue<GazeData>
    {
        #region Variables

        public long TimeLimit { get; set; }

        #endregion

        #region Public methods

        public GazeDataQueue(long timeLimit)
            : base()
        {
            this.TimeLimit = timeLimit;
        }

        public new void Enqueue(GazeData gd)
        {
            lock (this)
            {
                GazeData last;

                while (base.Count > 0 && base.TryPeek(out last) && null != last && (gd.TimeStamp - last.TimeStamp) > TimeLimit)
                {
                    base.TryDequeue(out last);
                }

                base.Enqueue(gd);
            }
        }
        public void Clear()
        {
            lock (this)
            {
                GazeData gd;
                while (!IsEmpty)
                    TryDequeue(out gd);
            }
        }

        #endregion
    }
}
