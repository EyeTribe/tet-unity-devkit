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

    public static Vector2 GetPerspectiveWorldScreenBounds(this Camera camera, float depth)
    {
        Vector3 position = camera.transform.position + camera.transform.forward * depth;

        float h = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * depth * 2f;

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
        if (routine == null)
            throw new System.ArgumentNullException("Parameter 'routine' is NULL");

        CoroutineController coroutineController = new CoroutineController(routine);
        coroutineController.StartCoroutine(monoBehaviour);
        return coroutineController;
    }

    #endregion
}