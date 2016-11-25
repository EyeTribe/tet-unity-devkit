/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System.Collections;
using UnityEngine;
using EyeTribe.Unity;

public class DestroyInGazeMode : MonoBehaviour {

    [SerializeField] private bool _VrMode;
    [SerializeField] private bool _RemoteMode;

    void OnEnable() 
    {
        if (_VrMode && VRMode.IsRunningInVRMode)
            Destroy(this.gameObject);

        if (_RemoteMode && !VRMode.IsRunningInVRMode)
            Destroy(this.gameObject);
    }
}
