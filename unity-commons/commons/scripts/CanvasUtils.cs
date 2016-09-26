/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace EyeTribe.Unity
{ 
    public class CanvasUtils
    {     
        public static RectTransform GetUiComponentFromTag(Canvas canvas, String tag)
        {
            RectTransform[] components = canvas.GetComponentsInChildren<RectTransform>();

            foreach (RectTransform rt in components)
            {
                if (rt.tag.Equals(tag))
                {
                    return rt;
                }
            }
            return null;
        }
    }
}
