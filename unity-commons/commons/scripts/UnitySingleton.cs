/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EyeTribe.Unity
{
    /// <summary>
    /// A singleton pattern that makes sure that a GameObject with the implementing UnitySingleton script
    /// is always present in a secene.
    /// <para/>
    /// Based on http://wiki.unity3d.com/index.php/Singleton
    /// </summary>
    public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _Instance;

        private static object _Lock = new object();

        private static bool _ApplicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_ApplicationIsQuitting)
                {
                    Debug.LogWarning("[UnitySingleton] Instance '" + typeof(T) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                lock (_Lock)
                {
                    if (_Instance == null)
                    {
                        _Instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[UnitySingleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " Reopenning the scene might fix it.");
                            return _Instance;
                        }

                        if (_Instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _Instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();

                            DontDestroyOnLoad(singleton);

                            Debug.Log("[UnitySingleton] An instance of " + typeof(T) +
                                " is needed in the scene, so '" + singleton +
                                "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log("[UnitySingleton] Using instance already created: " +
                                _Instance.gameObject.name);
                        }
                    }

                    return _Instance;
                }
            }
        }

        public void OnDestroy()
        {
            _ApplicationIsQuitting = true;
        }
    }
}
