/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace EyeTribe.Unity
{
    /* Inspired by on 'VRStandardAssets.Utils.Reticle' from 'Unity VRSamples' */

    /// <summary>
    /// Handles position of gaze driven reticle. The position of the
    /// reticle is either at a default position in space or on the
    /// surface of a VRInteractiveItem as determined by the GazeRaycaster.
    /// </summary>
    public class ReticleEyeTribe : MonoBehaviour
    {
        [SerializeField] private float _DefaultDistance = 5f;      // The default distance away from the camera the reticle is placed.
        [SerializeField] private bool _UseNormal;                  // Whether the reticle should be placed parallel to a surface.
        [SerializeField] private Image _Image;                     // Reference to the image component that represents the reticle.
        [SerializeField] private Transform _ReticleTransform;      // We need to affect the reticle's transform.
        [SerializeField] private Transform _Camera;                // The reticle is always placed relative to the camera.
       
        private Vector3 _OriginalScale;                            // Since the scale of the reticle changes, the original scale needs to be stored.
        private Quaternion _OriginalRotation;                      // Used to store the original rotation of the reticle.

        public bool UseNormal
        {
            get { return _UseNormal; }
            set { _UseNormal = value; }
        }

        public Transform ReticleTransform { get { return _ReticleTransform; } }

        private void Awake()
        {
            if (null == _Image)
                throw new Exception("_Image is not set!");

            if (null == _ReticleTransform)
                throw new Exception("_ReticleTransform is not set!");

            if (null == _Camera)
                throw new Exception("_Camera is not set!");

            // Store the original scale and rotation.
            _OriginalScale = _ReticleTransform.localScale;
            _OriginalRotation = _ReticleTransform.localRotation;
        }

        void OnEnable() 
        {
            GazeGUIController.OnIndicatorModeToggle += OnIndicatorToggle;
        }

        void OnDisable()
        {
            GazeGUIController.OnIndicatorModeToggle -= OnIndicatorToggle;
        }

        private void OnIndicatorToggle(bool isShown)
        {
            _ReticleTransform.gameObject.SetActive(isShown);
        }

        public void Hide()
        {
            _Image.enabled = false;
        }

        public void Show()
        {
            _Image.enabled = true;
        }

        public bool IsShown()
        {
            return _Image.enabled;
        }

        // This overload of SetPosition is used when the the VREyeRaycaster hasn't hit anything.
        public void SetPosition()
        {
            // Set the position of the reticle to the default distance in front of the camera.
            _ReticleTransform.position = _Camera.position + _Camera.forward * _DefaultDistance;

            // Set the scale based on the original and the distance from the camera.
            _ReticleTransform.localScale = _OriginalScale * _DefaultDistance;

            // The rotation should just be the default.
            _ReticleTransform.localRotation = _OriginalRotation;
        }

        public void SetPosition(Vector3 rayDirection)
        {
            // Set the position of the reticle to the default distance in front of the camera.
            _ReticleTransform.position = _Camera.position + rayDirection * _DefaultDistance;

            // Set the scale based on the original and the distance from the camera.
            _ReticleTransform.localScale = _OriginalScale * _DefaultDistance;

            // The rotation should just be the default.
            _ReticleTransform.localRotation = _OriginalRotation;
        }

        // This overload of SetPosition is used when the VREyeRaycaster has hit something.
        public void SetPosition(RaycastHit hit)
        {
            _ReticleTransform.position = hit.point;
            _ReticleTransform.localScale = _OriginalScale * hit.distance;

            // If the reticle should use the normal of what has been hit...
            if (_UseNormal)
                // ... set it's rotation based on it's forward vector facing along the normal.
                _ReticleTransform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            else
                // However if it isn't using the normal then it's local rotation should be as it was originally.
                _ReticleTransform.localRotation = _OriginalRotation;
        }
    }
}