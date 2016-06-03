/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk.Utils;

namespace EyeTribe.Unity
{
    public class UnityGazeUtils : GazeUtils
    {
        /// <summary>
        /// Converts a 2d point in relative values to a coordinate in Unity screen space.
        /// <para/>
        /// Use to map relative values [0.0f : 1.0f] to screen coordinates.
        /// </summary>
        /// <param name="point">gaze point to base calculation upon</param>
        /// <returns>2D point in screen space</returns>
        public static Point2D GetRelativeToScreenSpace(Point2D gp)
        {
            return GetRelativeToScreenSpace(gp, Camera.main.pixelWidth, Camera.main.pixelHeight);
        }

        /// <summary>
        /// Converts a screen point in pixels to normalized relative values based on EyeTribe Server
        /// screen settings
        /// </summary>
        /// <param name="point">gaze point to base calculation upon</param>
        /// <returns>2D point in relative values</returns>
        public static Point2D GetScreenSpaceToRelative(Point2D gp)
        {
            return new Point2D(gp.X / GazeManager.Instance.ScreenResolutionWidth, gp.Y / GazeManager.Instance.ScreenResolutionHeight);
        }

        /// <summary>
        /// Converts a screen point in pixels to normalized values relative to screen 
        /// center based on EyeTribe Server screen settings.
        /// <para/>
        /// Output point will be in the space [x: -1.0f:1.0f, y: -1.0f:1.0f] to screen coordinates.
        /// </summary>
        public static Point2D GetScreenToRelativeCenter(Point2D gp)
        {
            Point2D rel = GetScreenSpaceToRelative(gp);

            rel.X = (rel.X * 2) - 1;
            rel.Y = (rel.Y * 2) - 1;

            return rel; 
        }
    }
}
