/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
using UnityEngine;

namespace EyeTribe.Unity.Calibration
{
    /// <summary>
    /// Handles the process of resizing a RectTransform to a references Canvas.
    /// </summary>
    public class ResizeToCanvas : MonoBehaviour
    {
        [SerializeField]private Canvas _Canvas;
        [SerializeField]private RectTransform _RectTransform;

        void Awake()
        {
            if (null == _Canvas)
                throw new Exception("_Canvas is not set!");

            if (null == _RectTransform)
                throw new Exception("_RectTransform is not set!");
        }

        void OnEnable()
        {
            _RectTransform.sizeDelta = new Vector2(_Canvas.pixelRect.width, _Canvas.pixelRect.height);
        }
    }
}
