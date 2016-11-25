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

public class ProductInitializer : MonoBehaviour {

    [SerializeField]private Text _ProductName;
    [SerializeField]private Text _Version;

    void Awake()
    {
        if (null == _ProductName)
            throw new Exception("_ProductName is not set!");

        if (null == _Version)
            throw new Exception("_Version is not set!");
    }

    void OnEnable()
    {
        _ProductName.text = Application.productName;
        _Version.text = Application.version;
    }
}
