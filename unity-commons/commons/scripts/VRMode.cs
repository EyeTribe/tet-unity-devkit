/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.VR;
using System.Collections;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Utility class that identifies VR modes across different VR plugins
    /// </summary>
    public class VRMode
    {
        public static bool IsRunningInVRMode()
        {
            return IsOculusVRActive() || IsSteamVRActive();
        }

        public static bool IsOculusVRActive()
        {
            return VRSettings.enabled && VRSettings.loadedDeviceName.Equals("Oculus");
        }

        public static bool IsOculusSDKGearVR()
        {
            return IsAndroidPlatform() && IsOculusVRActive();
        }

        public static bool IsOculusSDKHmdVR()
        {
            return IsStandalonePlatform() && IsOculusVRActive();
        }

        public static bool IsSteamVRActive()
        {
            return VRSettings.enabled && VRSettings.loadedDeviceName.Equals("OpenVR");
        }

        private static bool IsStandalonePlatform()
        {
            if(Application.isEditor)
                return
                    Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor;
            else
                return
                    Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.LinuxPlayer ||
                    Application.platform == RuntimePlatform.OSXPlayer;
        }

        private static bool IsAndroidPlatform()
        {
            return Application.platform == RuntimePlatform.Android;
        }
    }
}
