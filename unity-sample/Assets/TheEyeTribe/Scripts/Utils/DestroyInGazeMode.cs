using UnityEngine;
using System.Collections;
using EyeTribe.Unity;

public class DestroyInGazeMode : MonoBehaviour {

    [SerializeField] private bool _VrMode;
    [SerializeField] private bool _RemoteMode;

    void OnEnable() 
    {
        if (_VrMode && VRMode.IsRunningInVRMode)
            Destroy(this.gameObject);

        if (_RemoteMode && !VRMode.IsRunningInVRMode)
            Destroy(this.gameObject);
    }
}
