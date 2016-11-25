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
        [SerializeField] private float _Radius = 10f;
        [SerializeField] private float _RandomRange;
        [SerializeField] private int _NumLongitude = 12;
        [SerializeField] private int _NumLatitude = 12;
        [SerializeField] private float _DuplicateDistance = .5f;
        [SerializeField] private bool _FaceCenter = true;
        [SerializeField] private bool _RandomRotation = false;

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

        protected virtual void OnEnable() 
        {
            Initialize();
        }

        void OnDisable()
        {
            foreach (Transform child in transform)
                GameObject.Destroy(child.gameObject);
        }

        private class Holder
        {
            public Vector3 pos;
            public Quaternion rot;

            public Holder( Vector3 pos, Quaternion rot)
            {
                this.pos = pos;
                this.rot = rot;
            }
        }

        protected virtual void Initialize()
        {
            float LongStep = 360f / _NumLongitude;
            float LatStep = 360f / _NumLatitude;

            Vector3 dir;
            float range;
            Vector3 v;
            Vector3 pos;
            List<Holder> tobe = new List<Holder>();
            // Generate transforms while evaluating duplicates
            for (int i = _NumLongitude; --i >= 0; )
            {
                for (int j = _NumLatitude; --j >= 0; )
                {
                    float rangeHalf = _RandomRange * .5f;
                    range = _Radius + UnityEngine.Random.Range(-rangeHalf, rangeHalf);
                    dir = Vector3.forward;
                    v = (transform.rotation * Quaternion.Euler(LongStep * i, LatStep * j, 0f)) * dir;
                    pos = v * range;

                    /*
                    Debug.DrawLine(transform.position + transform.rotation * pos,
                        transform.position + transform.rotation * (pos + (v * -range)), Color.red, 5f);
                    */

                    Quaternion rotation = Quaternion.identity;

                    if (_FaceCenter)
                        rotation = transform.rotation * Quaternion.LookRotation(v * -1);

                    if (_RandomRotation)
                        rotation = Quaternion.Euler(UnityEngine.Random.Range(0f, 360f), 
                            UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));

                    if (CheckDuplicate(tobe, pos))
                        tobe.Add(new Holder(pos, rotation));
                }
            }

            // Generate GameObjects
            foreach (Holder h in tobe)
            {
                GameObject go = CreateObject(h.pos, h.rot);
                go.transform.parent = transform;
            }
        }

        private bool CheckDuplicate(List<Holder> tobe, Vector3 pos)
        {
            foreach (Holder h in tobe)
                if (Vector3.Distance(h.pos, pos) < _DuplicateDistance)
                    return false;

            return true;
        }

        protected virtual GameObject CreateObject(Vector3 position, Quaternion rotation)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

            go.transform.position = position;
            go.transform.rotation = rotation;

            return go;
        }
    }
}
