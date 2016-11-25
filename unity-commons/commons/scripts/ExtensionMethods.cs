/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using EyeTribe.Unity;
using System;

public static class ExtensionMethods
{
    // Even though they are used like normal methods, extension
    // methods must be declared static. Notice that the first
    // parameter has the 'this' keyword. This variable denotes
    // which class the extension method becomes a part of.

    #region RectTransform Extensions

    public static Vector2 GetLocalRectPointFromGazePoint(this RectTransform rect, Camera cam, Point2D gaze)
    {
        Vector2 g = new Vector3((float)gaze.X, (float)gaze.Y);
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, g, cam, out local);

        local -= new Vector2(rect.rect.x, rect.rect.y);

        return local;
    }

    public static Vector3 GetWorldRectPointFromGazePoint(this RectTransform rect, Camera cam, Point2D gaze)
    {
        Vector3 g = new Vector3((float)gaze.X, (float)gaze.Y, 0);
        Vector3 world;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, g, cam, out world);

        world -= new Vector3(rect.rect.x, rect.rect.y, 0);

        return world;
    }

    public static void SetRendererEnabled(this RectTransform rt, bool isEnabled)
    {
        UIBehaviour[] uis = rt.GetComponentsInChildren<UIBehaviour>();

        if (null != uis)
            foreach (UIBehaviour uib in uis)
            {
                uib.enabled = isEnabled;
            }
    }

    public static bool IsRendererEnabled(this RectTransform rt)
    {
        UIBehaviour ui = rt.GetComponent<UIBehaviour>();

        if (null != ui)
            return ui.enabled;
        return false;
    }

    #endregion

    #region Point2D Extensions

    /// <summary>
    /// Maps a GazeData gaze point (RawCoordinates or SmoothedCoordinates) to Unity screen space. 
    /// Unity screen space has origin in bottom-left corner.
    /// </summary>
    /// <param name="gp"/>gaze point to map</param>
    /// <returns>2d point mapped to unity window space or null if input null</returns>
    public static Vector3 GetGazeCoordsToUnityScreenSpace(this Point2D gp, float depth)
    {
        return new Vector3((float)gp.X, (float)((float)Camera.main.pixelHeight - gp.Y), depth);
    }

    public static Vector3 GetGazeCoordsToUnityScreenSpaceScaled(this Point2D gp, float depth)
    {
        Point2D windowCoords = ScaleGazeCoordsToUnityWindow(gp);

        return new Vector3((float)windowCoords.X, (float)((float)Camera.main.pixelHeight - windowCoords.Y), 0f);
    }

    public static Vector3 GetGazeCoordsToUnityUICoords(this Point2D gp)
    {
        return new Vector3((float)gp.X, (float)-gp.Y, 0f);
    }

    public static Vector3 GetGazeCoordsToUnityUICoordsScaled(this Point2D gp)
    {
        Point2D windowCoords = ScaleGazeCoordsToUnityWindow(gp);
        return new Vector3((float)windowCoords.X, (float)-windowCoords.Y, 0f);
    }

    private static Point2D ScaleGazeCoordsToUnityWindow(Point2D gp)
    {
        return new Point2D(
            gp.X * ((float)Camera.main.pixelWidth / GazeManager.Instance.ScreenResolutionWidth),
            gp.Y * ((float)Camera.main.pixelHeight / GazeManager.Instance.ScreenResolutionHeight)
            );
    }

    /// <summary>
    /// Convert a Point2D to Unity vector.
    /// </summary>
    /// <returns>a vector representation of point</returns>
    public static Vector2 ToVec2(this Point2D gp)
    {
        return new Vector2((float)gp.X, (float)gp.Y);
    }

    /// <summary>
    /// Convert a Point2D to Unity vector.
    /// </summary>
    /// <returns>a vector representation of point</returns>
    public static Vector3 ToVec3(this Point2D gp)
    {
        return new Vector3((float)gp.X, (float)gp.Y, 0f);
    }

    public static Vector3 GetWorldPositionFromGaze(this Point2D gp, Camera cam, float depth)
    {
        if (null != cam)
        {
            return cam.ScreenToWorldPoint(
                new Vector3(
                    (float)gp.X,
                    (float)(cam.pixelHeight - gp.Y),
                    depth)
                    );
        }

        return Vector3.zero;
    }

    #endregion

    #region Point3D Extensions

    /// <summary>
    /// Convert a Point3D to Unity Vector3.
    /// </summary>
    /// <returns>a vector representation of point</returns>
    public static Vector3 ToVec3(this Point3D gp)
    {
        return new Vector3((float)gp.X, (float)gp.Y, (float)gp.Z);
    }

    #endregion

    #region Vector3 Extensions

    /// <summary>
    /// Convert a Unity Vector3 to a double[].
    /// </summary>
    /// <returns>double array</returns>
    public static double[] ToArray(this Vector3 vec)
    {
        return new double[3] { vec.x, vec.y, vec.z };
    }

    /// <summary>
    /// Convert a double[3] to a Unity Vector3.
    /// </summary>
    /// <param name="array"/>Array to convert</param>
    /// <returns>Unity Vector3</returns>
    public static Vector3 FromArray(this Vector3 vec, double[] array)
    {
        return new Vector3((float)array[0], (float)array[1], (float)array[2]);
    }

    /// <summary>
    /// Convert a Unity Vector3 to Point3D.
    /// </summary>
    /// <returns>a vector representation of point</returns>
    public static Point3D ToPoint3D(this Vector3 vec)
    {
        return new Point3D(vec.x, vec.y, vec.z);
    }

    /// <summary>
    /// Find a position relative to another scene object.
    /// </summary>
    /// <param name="origin"/>original to calculate new point from</param>
    /// <returns>Unity Vector3</returns>
    public static Vector3 GetRelativePosition(this Vector3 position, Transform origin)
    {
        Vector3 distance = position - origin.position;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
        relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
        relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

        return relativePosition;
    }

    #endregion

    #region GameObject Extensions

    public static Renderer GetRenderer(this GameObject go)
    {
        Renderer renderer = go.GetComponent<MeshRenderer>();

        if (null != renderer)
            return renderer;

        renderer = go.GetComponent<SpriteRenderer>();

        if (null != renderer)
            return renderer;

        return null;
    }

    public static bool SetRendererEnabled(this GameObject go, bool isEnabled)
    {
        Renderer renderer = GetRenderer(go);
        if (null != renderer)
        {
            renderer.enabled = isEnabled;
            return true;
        }

        return false;
    }

    public static bool IsRendererEnabled(this GameObject go)
    {
        Renderer renderer = GetRenderer(go);

        if (null != renderer)
            return renderer.enabled;

        return false;
    }

    public static bool SetWorldPositionFromGaze(this GameObject go, Camera cam, Point2D gazeCoords, float depth)
    {
        if (null != cam)
        {
            go.transform.position = cam.ScreenToWorldPoint(
                new Vector3(
                    (float)gazeCoords.X,
                    (float)(cam.pixelHeight - gazeCoords.Y),
                    depth)
                    );
            return true;
        }

        return false;
    }

    #endregion

    #region Canvas Extensions

    public static RectTransform GetChildRectTransformFromTag(this Canvas canvas, string tag)
    {
        RectTransform[] components = canvas.GetComponentsInChildren<RectTransform>();

        foreach (RectTransform rt in components)
        {
            if (rt.tag.Equals(tag))
            {
                return rt;
            }
        }
        return null;
    }

    #endregion

    #region Camera Extensions

    /// <summary>
    /// Find the bounds of the Camera viewport in Unity units (meters) at a desired distance.
    /// </summary>
    /// <param name="depth"/>distance from camera to use</param>
    /// <returns>Unity Vector2</returns>
    public static Vector2 GetPerspectiveWorldScreenBounds(this Camera camera, float depth)
    {
        return GetPerspectiveDegreeBounds(camera, camera.fieldOfView, depth);
    }

    /// <summary>
    /// Find the bounds of custom Camera viewport in Unity units (meters) at a desired distance.
    /// </summary>
    /// <param name="degrees"/>width of custom fov in degrees</param>
    /// <param name="depth"/>distance from camera to use</param>
    /// <returns>Unity Vector2</returns>
    public static Vector2 GetPerspectiveDegreeBounds(this Camera camera, float degrees, float depth)
    {
        Vector3 position = camera.transform.position + camera.transform.forward * depth;

        float h = Mathf.Tan(degrees * Mathf.Deg2Rad * 0.5f) * depth * 2f;

        return new Vector2(h * camera.aspect, h);
    }

    public static Vector2 GetOrthographicWorldScreenBounds(this Camera camera)
    {
        float worldScreenHeight = camera.orthographicSize;
        float worldScreenWidth = worldScreenHeight / camera.pixelHeight * camera.pixelWidth;

        return new Vector2(worldScreenWidth, worldScreenHeight);
    }

    #endregion


    #region MonoBehavior Extensions

    public static CoroutineController StartCoroutineExtension(this MonoBehaviour monoBehaviour, IEnumerator routine)
    {
        return StartCoroutineExtension(monoBehaviour, routine, null);
    }

    public static CoroutineController StartCoroutineExtension(this MonoBehaviour monoBehaviour, IEnumerator routine, Action onFinish)
    {
        if (routine == null)
            throw new System.ArgumentNullException("Parameter 'routine' is NULL");

        CoroutineController coroutineController = new CoroutineController(routine);
        if (null != onFinish)
            coroutineController.OnFinish += onFinish;
        coroutineController.StartCoroutine(monoBehaviour);
        return coroutineController;
    }

    #endregion

    #region Color Extensions

    public static Color GetColorFromHex(this Color color, string hex)
    {
        hex = hex.Replace("0x", "");    //in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");     //in case the string is formatted #FFFFFF

        byte a = 255;   //assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }

    public static Color GetAlphaColor(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    #endregion
}