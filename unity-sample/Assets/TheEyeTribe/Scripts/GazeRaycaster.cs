/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;
using EyeTribe.Unity;
using EyeTribe.ClientSdk.Data;
using System;
using EyeTribe.Unity.Calibration;

namespace EyeTribe.Unity
{
    /* Inspired by on 'VRStandardAssets.Utils.VREyeRaycaster' from 'Unity VRSamples' */

    /// <summary>
    /// Handles the UI associated to displaying eye pupils
    /// </summary>
    public class GazeRaycaster : MonoBehaviour
    {
        [SerializeField]private int _UpdatesPerSecond = 60;
        private float _UpdateDelaySeconds;

        [SerializeField]private Camera _Camera;
        [SerializeField]private LayerMask _ExclusionLayers;
        [SerializeField]private bool _ShowDebugRay;                   // Optionally show the debug ray.
        [SerializeField]private float _DebugRayLength = 5f;           // Debug ray length.
        [SerializeField]private float _DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        [SerializeField]private float _RayLength = 100f;              // How far into the scene the ray is cast.
        [SerializeField]private ReticleEyeTribe _Reticle;

        private VRInteractiveItem _CurrentInteractible;                //The current interactive item
        private VRInteractiveItem _LastInteractible;                   //The last interactive item

        private bool _UsingSmooth;

        private IEnumerator _GazeCycle;

        void Awake()
        {
            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            if (null == _Reticle)
                throw new Exception("_Reticle is not set!");

            if (_UpdatesPerSecond <= 0)
                throw new Exception("UpdatesPerSecond must be a positive number");

            _UpdateDelaySeconds = 1f / _UpdatesPerSecond;
        }

        private void OnEnable()
        {
            StartCoroutine(_GazeCycle = GazeRaycastCoroutine());

            GazeUiController.OnSmoothModeToggle += ToogleSmooth;
        }

        private void OnDisable()
        {
            if (null != _GazeCycle)
                StopCoroutine(_GazeCycle);

            GazeUiController.OnSmoothModeToggle -= ToogleSmooth;
        }

        private void DeactiveLastInteractible()
        {
            if (_LastInteractible == null)
                return;

            _LastInteractible.Out();
            _LastInteractible = null;
        }

        private void ToogleSmooth(bool isSmooth)
        {
            _UsingSmooth = isSmooth;
        }

        IEnumerator GazeRaycastCoroutine()
        {
            while (enabled)
            {
                Point3D gazeVec = GazeFrameCache.Instance.GetLastGazeVectorAvg();
                Ray ray = new Ray();

                if (Point3D.Zero != gazeVec)
                {
                    ray = new Ray(_Camera.transform.position, _Camera.transform.rotation * gazeVec.ToVec3());
                }
                else 
                {
                    // If no gaze vec, define ray based on 2D gaze coordinates
                    Point2D gaze;
                    if (!_UsingSmooth)
                        gaze = GazeFrameCache.Instance.GetLastRawGazeCoordinates();
                    else
                        gaze = GazeFrameCache.Instance.GetLastSmoothedGazeCoordinates();

                    if (Point2D.Zero != gaze)
                    { 
                        Vector3 worldGaze = _Camera.ScreenToWorldPoint(new Vector3(gaze.X, _Camera.pixelHeight - gaze.Y, 10f));
    
                        Vector3 rayVec = worldGaze - _Camera.transform.position;
                        rayVec.Normalize();

                        gazeVec = rayVec.ToPoint3D();

                        ray = new Ray(_Camera.transform.position, rayVec);
                    }
                }

                if (Point3D.Zero != gazeVec)
                {
                    Vector3 rayDirection = gazeVec.ToVec3();

                    // Show the debug ray if required
                    if (_ShowDebugRay)
                    {
                        Debug.DrawRay(_Camera.transform.position, rayDirection * _DebugRayLength, Color.blue, _DebugRayDuration);
                    }

                    RaycastHit hit;

                    // Do the raycast forweards to see if we hit an interactive item
                    if (Physics.Raycast(ray, out hit, _RayLength, ~_ExclusionLayers))
                    {
                        VRInteractiveItem interactible = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                        _CurrentInteractible = interactible;

                        // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                        if (interactible && interactible != _LastInteractible)
                            interactible.Over();

                        // Deactive the last interactive item 
                        if (interactible != _LastInteractible)
                            DeactiveLastInteractible();

                        _LastInteractible = interactible;
                    }
                    else
                    {
                        // Nothing was hit, deactive the last interactive item.
                        DeactiveLastInteractible();
                        _CurrentInteractible = null;
                    }

                    //handle reticle position
                    _Reticle.SetPosition(hit);
                    _Reticle.Show();
                }
                else
                {
                    _Reticle.Hide();
                }

                yield return new WaitForSeconds(_UpdateDelaySeconds);
            }
        }
    }
}
