/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;

namespace EyeTribe.Unity
{
    public class EyeTribeSDK : MonoBehaviour, IGazeListener, IConnectionStateListener, ITrackerStateListener, ICalibrationResultListener
    {
        public static event Action<bool> OnConnectionStateChange;
        public static event Action<GazeManager.TrackerState> OnTrackerStateChange;
        public static event Action<bool, CalibrationResult> OnCalibration;

        [SerializeField]private UnityDispatcher _Dispatcher;

        private const string DEFAULT_CFG = "network.cfg";

        void Awake() 
        {
            if (null == _Dispatcher)
                Debug.LogError("_Dispatcher is not set!");
        }

        void OnEnable()
        {
            //register listeners
            GazeManager.Instance.AddGazeListener(this);
            GazeManager.Instance.AddTrackerStateListener(this);
            GazeManager.Instance.AddCalibrationResultListener(this);
            GazeManager.Instance.AddConnectionStateListener(this);

            //activate EyeTribe C# SDK, default port
            if (!GazeManager.Instance.IsActivated)
                if (Application.platform != RuntimePlatform.Android)
                    GazeManager.Instance.ActivateAsync();
                else
                {
                    // Create cfg file in external storage if not already present
                    String path = GetExternalFilePath( DEFAULT_CFG);

                    if (null != path)
                    { 
                        // Read IP from cfg file
                        String ip = GetIpFromJsonFile(path);

                        Debug.Log("Connecting to IP ADDRESS: " + ip);

                        GazeManager.Instance.ActivateAsync(GazeManager.ApiVersion.VERSION_1_0, ip, 6555);
                    }
                }
        }

        void OnDisable()
        {
            //deregister listeners
            GazeManager.Instance.RemoveGazeListener(this);
            GazeManager.Instance.RemoveTrackerStateListener(this);
            GazeManager.Instance.RemoveCalibrationResultListener(this);
            GazeManager.Instance.RemoveConnectionStateListener(this);
        }

        public void OnGazeUpdate(GazeData gazeData)
        {
            //Add frame to GazeData cache handler
            GazeFrameCache.Instance.Update(gazeData);
        }

        void OnApplicationQuit()
        {
            GazeManager.Instance.Deactivate();
        }

        public void OnConnectionStateChanged(bool isConnected) 
        {
            _Dispatcher.Dispatch(() => 
            {
                if (OnConnectionStateChange != null)
                    OnConnectionStateChange(isConnected);
            });
        }

        public void OnTrackerStateChanged(GazeManager.TrackerState trackerState) 
        {
            _Dispatcher.Dispatch(() =>
            {
                if (OnTrackerStateChange != null)
                    OnTrackerStateChange(trackerState);
            });
        }

        public void OnCalibrationChanged(bool isCalibrated, CalibrationResult calibResult)
        {
            _Dispatcher.Dispatch(() =>
            {
                if (OnCalibration != null)
                    OnCalibration(isCalibrated, calibResult);
            });
        }

        public String GetExternalFilePath(string fileName)
        {
            String extPath = null;

            if (Application.platform == RuntimePlatform.Android)
            {
                extPath = GetAndroidContextExternalFilesDir() + "/" + DEFAULT_CFG;

                // We check if the config file exists, else create it
                if(!File.Exists(extPath))
                {
                    string cfg = "{\"ip\": \"192.168.43.15\"}";

                    FileStream fs = null;

                    try
                    {
                        //copy custom config file to default config file path
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(cfg);
                        fs = File.Open(extPath, FileMode.OpenOrCreate);
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Unable to write EyeTribe ip config file: " + e.Message);
                    }
                    finally
                    {
                        try {fs.Close();}catch{}
                    }
                }
            }

            return extPath;
        }

        private String GetIpFromJsonFile(string filePath)
        {
            String ip = "";
        
            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                JObject jo = JObject.Parse(json);
                ip = (string)jo["ip"];

                if(ip.CompareTo("")!= 0)
                {
                    IPAddress address;
                    if (!IPAddress.TryParse(ip, out address))
                        throw new Exception("Ip address not valid");
                }
                else
                    throw new Exception("No valid Ip address found in CFG file");

            }
            catch(Exception e)
            {
                Debug.LogWarning("Error reading IP address from JSON file: " + e.Message);
            } 
        
            return ip;
        }

        private String GetAndroidContextExternalFilesDir()
        {
            string path = "";

            if (Application.platform == RuntimePlatform.Android)
            {
                //if Android, we call native methods to find sdcard context storage path

                try
                {
                    using (AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        using (AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            path = ajo.Call<AndroidJavaObject>("getExternalFilesDir", null).Call<string>("getAbsolutePath");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error fetching native Android external storage dir: " + e.Message);
                }
            }

            return path;
        }
    }
}
