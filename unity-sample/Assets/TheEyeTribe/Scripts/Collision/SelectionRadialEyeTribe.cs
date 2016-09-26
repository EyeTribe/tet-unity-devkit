/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using VRStandardAssets.Utils;

namespace EyeTribe.Unity
{
    /* Based on 'VRStandardAssets.Utils.SelectionRadial' from 'Unity VRSamples' */

    /// <summary>
    /// Handles logic associated to a radial bar that fills up as the user holds down the space
    /// or Fire1 button. When it has finished filling it triggers an event.
    /// <para/>
    /// This is typically used in combination with ReticleEyeTribe.
    /// </summary>
    public class SelectionRadialEyeTribe : MonoBehaviour
    {
        public static event Action OnSelectionStarted;                                     // This event is triggered when the bar starts filling.
        public static event Action OnSelectionAborted;                                     // This event is triggered when the bar stop before being full.
        public static event Action OnSelectionComplete;                                    // This event is triggered when the bar has filled.

        [SerializeField] private float _SelectionDuration = 1f;                     // How long it takes for the bar to fill.
        [SerializeField] private bool _HideOnStart = true;                          // Whether or not the bar should be visible at the start.
        [SerializeField] private Image _Selection;                                  // Reference to the image who's fill amount is adjusted to display the bar.
        [SerializeField] private ReticleEyeTribe _Reticle; 

        private IEnumerator _SelectionFillRoutine;                                 // Used to start and stop the filling coroutine based on input.
        private bool _IsSelectionRadialActive;                                     // Whether or not the bar is currently useable.

        public float SelectionDuration { get { return _SelectionDuration; } }

        public bool IsFilling { get { return _SelectionFillRoutine != null; } }

        private void Awake()
        {
            if (null == _Selection)
                throw new Exception("_Selection is not set!");

            if (null == _Reticle)
                throw new Exception("_Reticle is not set!");
        }

        void OnEnable()
        {
            VRInput.OnDown += HandleDown;
            VRInput.OnUp += HandleUp;
        }

        void OnDisable()
        {
            VRInput.OnDown -= HandleDown;
            VRInput.OnUp -= HandleUp;
        }

        private void Start()
        {
            // Setup the radial to have no fill at the start and hide if necessary.
            _Selection.fillAmount = 0f;

            if (_HideOnStart)
                Hide();
        }

        public void Show()
        {
            _Selection.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _Selection.gameObject.SetActive(false);
        }

        private void Update() 
        {
            if (
                /* Handled by VRInput
                Input.GetKeyDown(KeyCode.LeftControl) ||
                    */
                Input.GetKeyDown(KeyCode.RightControl) ||
                Input.GetKeyDown(KeyCode.LeftCommand) ||
                Input.GetKeyDown(KeyCode.RightCommand)
                )
            {
                HandleDown();
            }

            if (
                /* Handled by VRInput
                Input.GetKeyUp(KeyCode.LeftControl) ||
                 */
                Input.GetKeyUp(KeyCode.RightControl) ||
                Input.GetKeyUp(KeyCode.LeftCommand) ||
                Input.GetKeyUp(KeyCode.RightCommand)
                )
            {
                HandleUp();
            }
        }

        private IEnumerator FillSelectionRadial()
        {
            // Create a timer and reset the fill amount.
            float timer = 0f;
            _Selection.fillAmount = 0f;

            if (null != OnSelectionStarted)
                OnSelectionStarted();
            
            // This loop is executed once per frame until the timer exceeds the duration.
            while (_IsSelectionRadialActive && timer < _SelectionDuration)
            {
                // The image's fill amount requires a value from 0 to 1 so we normalise the time.
                _Selection.fillAmount = timer / _SelectionDuration;

                // Increase the timer by the time between frames and wait for the next frame.
                timer += Time.deltaTime;

                yield return null;
            }

            // When the loop is finished set the fill amount to be full.
            _Selection.fillAmount = 1f;

            // Turn off the radial so it can only be used once.
            _IsSelectionRadialActive = false;

            // We reset IEnumerator since used in logic
            _SelectionFillRoutine = null;

            // If there is anything subscribed to OnSelectionComplete call it.
            if (null != OnSelectionComplete)
                OnSelectionComplete();

            // This effectively resets the radial for when it's shown again.
            _Selection.fillAmount = 0f;    

            // Hide once complete
            Hide();
        }
        
        private void HandleDown()
        {
            if (null == _SelectionFillRoutine)
            {
                // Only show if _Reticle active
                if (_Reticle.enabled)
                    Show();

                _IsSelectionRadialActive = true;
    
                // If the radial is active start filling it.
                StartCoroutine(_SelectionFillRoutine = FillSelectionRadial());
            }
        }

        private void HandleUp()
        {
            // If the radial is active stop filling it and reset it's amount.
            if (null != _SelectionFillRoutine)
            {
                StopCoroutine(_SelectionFillRoutine);
                _SelectionFillRoutine = null;

                // If there is anything subscribed to OnSelectionAborted call it.
                if (null != OnSelectionAborted)
                    OnSelectionAborted();
            }

            // Turn off the radial so it can only be used once.
            _IsSelectionRadialActive = false;

            // This effectively resets the radial for when it's shown again.
            _Selection.fillAmount = 0f;  

            Hide();
        }
    }
}