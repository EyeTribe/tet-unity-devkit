/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

namespace EyeTribe.Unity.Interaction
{

    public class SphericalCreatorInteraction : SphericalCreator
    {
        [SerializeField] private SelectionRadialEyeTribe _SelectionRadialEyeTribe;
        [SerializeField] private Transform _ReticleTransform;
        [SerializeField] private StateNotifyer _StateNotifyer;

        protected override void Awake()
        {
            base.Awake();

            if (null == _SelectionRadialEyeTribe)
                throw new Exception("_SelectionRadialEyeTribe is not set");

            if (null == _ReticleTransform)
                throw new Exception("_ReticleTransform is not set");

            if (null == _StateNotifyer)
                throw new Exception("_StateNotifyer is not set");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Invoke("IntroText", 1f);
        }

        private void IntroText()
        { 
            _StateNotifyer.ShowState(.5f, 2f, "<b>GAZE</b> and press <b>FIRE1*</b> to interact");
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
