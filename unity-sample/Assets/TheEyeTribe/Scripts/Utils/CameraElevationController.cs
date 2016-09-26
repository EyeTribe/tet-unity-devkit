using UnityEngine;
using System.Collections;
using EyeTribe.Unity;

public class CameraElevationController : MonoBehaviour {

    [SerializeField] public float ElevationVR = 1.6f;
    [SerializeField] public float ElevationRemote = 3f;

    void Start() 
    {
        if (VRMode.IsRunningInVRMode)
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + ElevationVR,
                transform.position.z);
        else
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + ElevationRemote,
                transform.position.z);
    }
}
