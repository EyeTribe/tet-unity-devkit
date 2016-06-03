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

namespace EyeTribe.Unity.Interaction
{
    /// <summary>
    /// Handles the process of disabling a GO if initial state is colliding.
    /// <para/>
    /// Script disables itself after passing of _TimeFrame;
    /// </summary>
    public class InteractiveInitialCollisionCheck : MonoBehaviour
    {
        private float _TimeFrame = 1f;

        private bool _HasChecked = false;

        void OnCollisionEnter(Collision collision)
        {
            if (!_HasChecked)
            {
                _HasChecked = true;

                gameObject.SetActive(false);
                gameObject.transform.parent = null;
                Destroy(gameObject);
                enabled = false;
                Destroy(this);
            }
        }

        void Update() 
        {
            _TimeFrame -= Time.deltaTime;

            if (_TimeFrame < 0f) 
            {
                enabled = false;
                Destroy(this);
            }
        }
    }
}
