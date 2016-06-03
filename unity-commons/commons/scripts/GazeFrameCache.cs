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
using System.Collections;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk.Utils;
using UnityEngine;

namespace EyeTribe.Unity
{ 
    /// <summary>
    /// Utility class that maintains a run-time cache of GazeData frames. Based on the cache 
    /// the class analyzes the frame history and finds the currently valid gaze data.
    /// Use this class to avoid the 'glitch' effect of occational poor tracking.
    /// </summary>
    public class GazeFrameCache
    {
        #region Constants

        internal const long DEFAULT_CACHE_TIME_FRAME_MILLIS = 500;
        internal const int NO_TRACKING_MASK = GazeData.STATE_TRACKING_FAIL | GazeData.STATE_TRACKING_LOST;

        #endregion

        #region Variables

        protected float _MinimumEyesDistance = 0.1f;
        protected float _MaximumEyesDistance = 0.4f;

        protected GazeDataQueue _Frames;

        protected Eye _LastLeftEye;
        protected Eye _LastRightEye;

        protected Point2D _LastRawGazeCoords;
        protected Point2D _LastSmoothedGazeCoords;

        protected Point3D _LastUserPosition;

        protected double _LastEyeAngle;

        protected long _UserPosTimeStamp = -1;

        protected long _FrameTimeStamp;
        protected long _FrameDelta;

        //internals
        private Point2D _LastEyesDistHalfVec;
        private float _LastEyeDistance;

        #endregion

        #region Private methods

        private GazeFrameCache(long queueLengthMillis)
        {
            _Frames = new GazeDataQueue(queueLengthMillis);
            _LastUserPosition = new Point3D();
            _FrameTimeStamp = GetCurrentTimeInMillis();
            _FrameDelta = 50;
            //init user distance values
            _LastEyesDistHalfVec = new Point2D(.2f, 0f);
            _LastEyeDistance = 1f - ((_MinimumEyesDistance + ((_MaximumEyesDistance - _MinimumEyesDistance) * .5f)) / _MaximumEyesDistance);
            _LastUserPosition = new Point3D(GazeManager.Instance.ScreenResolutionWidth >> 1, GazeManager.Instance.ScreenResolutionHeight >> 1, (float)_LastEyeDistance);
        }

        private class Holder
        {
            static Holder() { }
            //thread-safe initialization on demand
            internal static readonly GazeFrameCache INSTANCE = new GazeFrameCache(DEFAULT_CACHE_TIME_FRAME_MILLIS);
        }

        private long GetCurrentTimeInMillis()
        {
            long factor = 10000;
            return ((factor * DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond) / factor;
        }

        #endregion

        #region Public methods

        public static GazeFrameCache Instance
        {
            get { return Holder.INSTANCE; }
        }

        public virtual void Update(GazeData frame)
        {
            //only update if not contained already
            if (_Frames.Contains(frame))
                return;

            //set delta based on continuous stream and not valid frames only
            long now = GetCurrentTimeInMillis();
            _FrameDelta = now - _FrameTimeStamp;
            _FrameTimeStamp = now;

            _Frames.Enqueue(frame);

            // update gazedata based on store
            Eye right = null, left = null;

            bool userPosIsValid = false;

            Point2D gazeCoords = Point2D.Zero;
            Point2D gazeCoordsSmooth = Point2D.Zero;
            Point2D userPos = Point2D.Zero;
            double userDist = 0f;
            Point2D eyeDistVecHalf = Point2D.Zero;
            GazeData gd;

            lock (_Frames)
            {
                for (int i = _Frames.Count; --i >= 0; )
                {
                    gd = _Frames.ElementAt(i);

                    // if no tracking problems, then cache eye data
                    if ((gd.State & NO_TRACKING_MASK) == 0)
                    {
                        if (!userPosIsValid &&
                            Point2D.Zero != gd.LeftEye.PupilCenterCoordinates &&
                            Point2D.Zero != gd.RightEye.PupilCenterCoordinates)
                        {
                            userPosIsValid = true;

                            userPos = (gd.LeftEye.PupilCenterCoordinates + gd.RightEye.PupilCenterCoordinates) / 2;
                            eyeDistVecHalf = (gd.RightEye.PupilCenterCoordinates - gd.LeftEye.PupilCenterCoordinates) / 2;
                            userDist = GazeUtils.GetDistancePoint2D(gd.LeftEye.PupilCenterCoordinates, gd.RightEye.PupilCenterCoordinates);

                            left = gd.LeftEye;
                            right = gd.RightEye;
                        }
                        else if (!userPosIsValid && left == null && Point2D.Zero != gd.LeftEye.PupilCenterCoordinates)
                        {
                            left = gd.LeftEye;
                        }
                        else if (!userPosIsValid && right == null && Point2D.Zero != gd.RightEye.PupilCenterCoordinates)
                        {
                            right = gd.RightEye;
                        }

                        // if gaze coordinates available, cache both raw and smoothed
                        if (Point2D.Zero == gazeCoords && Point2D.Zero != gd.RawCoordinates)
                        {
                            gazeCoords = gd.RawCoordinates;
                            gazeCoordsSmooth = gd.SmoothedCoordinates;
                        }
                    }

                    // break loop if valid values found
                    if (userPosIsValid && Point2D.Zero != gazeCoords)
                        break;
                }

                _LastRawGazeCoords = gazeCoords;
                _LastSmoothedGazeCoords = gazeCoordsSmooth;

                if (Point2D.Zero != eyeDistVecHalf && eyeDistVecHalf != _LastEyesDistHalfVec)
                    _LastEyesDistHalfVec = eyeDistVecHalf;

                //Update user position values if needed data is valid
                if (userPosIsValid)
                {
                    _LastLeftEye = left;
                    _LastRightEye = right;

                    //update 'depth' measure
                    if (userDist < _MinimumEyesDistance)
                        _MinimumEyesDistance = (float)userDist;

                    if (userDist > _MaximumEyesDistance)
                        _MaximumEyesDistance = (float)userDist;

                    _LastEyeDistance = 1f - ((float)userDist / _MaximumEyesDistance);

                    //update user position
                    _LastUserPosition = new Point3D(userPos.X, userPos.Y, (float)_LastEyeDistance);

                    //map to normalized 3D space
                    _LastUserPosition.X = (_LastUserPosition.X * 2) - 1;
                    _LastUserPosition.Y = (_LastUserPosition.Y * 2) - 1;

                    _UserPosTimeStamp = now;

                    //update angle
                    double dy = _LastRightEye.PupilCenterCoordinates.Y - _LastLeftEye.PupilCenterCoordinates.Y;
                    double dx = _LastRightEye.PupilCenterCoordinates.X - _LastLeftEye.PupilCenterCoordinates.X;
                    _LastEyeAngle = ((180 / Math.PI * Math.Atan2(GazeManager.Instance.ScreenResolutionHeight * dy, GazeManager.Instance.ScreenResolutionWidth * dx)));
                }
                else if (null != left)
                {
                    _LastLeftEye = left;
                    _LastRightEye = null;
                    Point2D newPos = _LastLeftEye.PupilCenterCoordinates + _LastEyesDistHalfVec;
                    _LastUserPosition = new Point3D(newPos.X, newPos.Y, (float)_LastEyeDistance);

                    //map to normalized 3D space
                    _LastUserPosition.X = (_LastUserPosition.X * 2) - 1;
                    _LastUserPosition.Y = (_LastUserPosition.Y * 2) - 1;

                    _UserPosTimeStamp = now;
                }
                else if (null != right)
                {
                    _LastRightEye = right;
                    _LastLeftEye = null;
                    Point2D newPos = _LastRightEye.PupilCenterCoordinates - _LastEyesDistHalfVec;
                    _LastUserPosition = new Point3D(newPos.X, newPos.Y, (float)_LastEyeDistance);

                    //map to normalized 3D space
                    _LastUserPosition.X = (_LastUserPosition.X * 2) - 1;
                    _LastUserPosition.Y = (_LastUserPosition.Y * 2) - 1;

                    _UserPosTimeStamp = now;
                }
                else
                {
                    _LastRightEye = null;
                    _LastLeftEye = null;
                }
            }
        }

        /// <summary>
        /// Position of user in normalized right-handed 3D space with respect to device. Approximated from position of eyes.
        /// </summary>
        /// <returns>Normalized 3d position</returns>
        public Point3D GetLastUserPosition()
        {
            return _LastUserPosition;
        }

        public Eye GetLastLeftEye()
        {
            return _LastLeftEye;
        }

        public Eye GetLastRightEye()
        {
            return _LastRightEye;
        }

        public double GetLastEyesAngle()
        {
            return _LastEyeAngle;
        }

        public Point2D GetLastRawGazeCoordinates()
        {
            return _LastRawGazeCoords;
        }

        public Point2D GetLastSmoothedGazeCoordinates()
        {
            return _LastSmoothedGazeCoords;
        }

        public long GetLastDelta()
        {
            return _FrameDelta;
        }

        public long GetLastUserPosTimeStamp()
        {
            return _UserPosTimeStamp;
        }

        public long GetLastUserPosDelta()
        {
            return GetCurrentTimeInMillis() - _UserPosTimeStamp;
        }

        public Point3D GetLastGazeVectorLeft() { return Point3D.Zero; }

        public Point3D GetLastGazeVectorRight() { return Point3D.Zero; }

        public Point3D GetLastGazeVectorAvg() { return Point3D.Zero; }

        public void Clear()
        {
            lock (_Frames)
            {
                _Frames.Clear();
            }
        }



        #endregion
    }
}
