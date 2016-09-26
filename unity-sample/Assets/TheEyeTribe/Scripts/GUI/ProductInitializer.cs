using UnityEngine;
using System.Collections;
using System;
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
