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
    /// <summary>
    /// Utility class the creates GameObjects in a spherical pattern around parent GO
    /// <para/>
    /// Classes wishing to create an arbitrary GameObject should extend this class and
    /// override CreateObject() method.
    /// </summary>
    public class SphericalCreator : MonoBehaviour
    {
        [SerializeField]private float _Radius = 10f;
        [SerializeField]private float _RandomRange;
        [SerializeField]private int _NumLongitude = 12;
        [SerializeField]private int _NumLatitude = 12;
        [SerializeField]private float _DuplicateDistance = .5f;
        [SerializeField]private bool _FaceCenter = true;
        [SerializeField]private bool _RandomRotation = false;

        protected virtual void Awake()
        {
            if (_Radius <= 0f)
                throw new Exception("_Radius must be a positive number");

            if (_RandomRange < 0f)
                throw new Exception("_RandomRange must be a positive number");

            if (_NumLongitude < 1f)
                throw new Exception("_NumLongitude must be a positive and non-zero number");

            if (_NumLatitude < 1f)
                throw new Exception("_NumLatitude must be a positive and non-zero number");

            if (_DuplicateDistance < 0f)
                throw new Exception("_DuplicateDistance must be a positive number");
        }

        protected virtual void Start()
        {
            float LongStep = 360f / _NumLongitude;
            float LatStep = 360f / _NumLatitude;

            _NumLatitude = _NumLatitude / 2;

            Vector3 dir;
            Vector3 v;
            Vector3 pos;
            GameObject go;
            for (int i = _NumLongitude; --i >= 0; )
            {
                for (int j = _NumLatitude; --j >= 0; )
                {
                    float rangeHalf = _RandomRange * .5f;
                    dir = transform.forward * (_Radius + UnityEngine.Random.Range(-rangeHalf, rangeHalf));
                    v = Quaternion.Euler(LongStep * i, LatStep * j, 0f) * dir;
                    pos = transform.position + v;

                    if (CheckDuplicate(pos))
                    {
                        go = CreateObject();
                        go.transform.position = transform.position + v;
                        if (_FaceCenter)
                            go.transform.rotation = Quaternion.LookRotation(v * -1);
                        if (_RandomRotation)
                            go.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));
                        go.transform.parent = this.transform;
                    }
                }
            }
        }

        private bool CheckDuplicate(Vector3 pos)
        {
            Vector3 dist;
            for (int i = transform.childCount; --i >= 0; )
            {
                Vector3 p = transform.GetChild(i).position;
                dist = pos - p;
                if (dist.magnitude < _DuplicateDistance)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual GameObject CreateObject()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

            return go;
        }
    }
}
