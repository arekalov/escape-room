using UnityEngine;

// Follows the XR Main Camera and renders to Display 1 so the Unity Game View shows something.
// Only active in the editor; stripped in builds automatically via UNITY_EDITOR define.
public class SpectatorCamera : MonoBehaviour
{
    Transform _target;

    void Start()
    {
        var all = Resources.FindObjectsOfTypeAll<Camera>();
        foreach (var c in all)
        {
            if (c.gameObject == gameObject) continue;
            if (c.name == "Main Camera" && c.gameObject.activeInHierarchy)
            {
                _target = c.transform;
                break;
            }
        }

        var cam = GetComponent<Camera>();
        cam.targetDisplay = 0;
        cam.depth = -10;
        cam.stereoTargetEye = StereoTargetEyeMask.None;
    }

    void LateUpdate()
    {
        if (_target == null) return;
        transform.SetPositionAndRotation(_target.position, _target.rotation);
    }
}
