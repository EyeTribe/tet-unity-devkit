/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using VRStandardAssets.Utils;
using System.Collections;
using UnityEngine.Events;
using System;

namespace EyeTribe.Unity.Interaction
{

    public class SphericalCreatorInteraction : SphericalCreator
    {
        [SerializeField]private SelectionRadialEyeTribe _SelectionRadialEyeTribe;
        [SerializeField]private Transform _ReticleTransform;

        protected override void Awake()
        {
            base.Awake();

            if (null == _SelectionRadialEyeTribe)
                throw new Exception("_SelectionRadialEyeTribe is not set");

            if (null == _ReticleTransform)
                throw new Exception("_ReticleTransform is not set");
        }

        protected override GameObject CreateObject(Vector3 position, Quaternion rotation)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = position;
            go.transform.rotation = rotation;

            VRInteractiveItem vrii = go.AddComponent<VRInteractiveItem>();
            go.AddComponent<InitialCollisionCheck>();
            InteractiveColorInterpolator ici  = go.AddComponent<InteractiveColorInterpolator>();
            InteractiveShakeAndFire isaf = go.AddComponent<InteractiveShakeAndFire>();
            Rigidbody rb = go.AddComponent<Rigidbody>();

            ici.InteractiveItem = vrii;
            ici.Initialize();

            isaf.InteractiveItem = vrii;
            isaf.Selection = _SelectionRadialEyeTribe;
            isaf.ReticleTransform = _ReticleTransform;
            isaf.RigidBody = rb;
            isaf.Initialize();

            return go;
        }
    }
}
