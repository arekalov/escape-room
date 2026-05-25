using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// На Quest (Floor tracking) пол часто ниже меша — игрок оказывается внутри дивана/пола.
/// Поднимает XR Origin после старта. В редакторе не применяется.
/// </summary>
[DefaultExecutionOrder(-100)]
public class VRQuestFloorOffset : MonoBehaviour
{
    [Tooltip("Доп. подъём рига по Y на устройстве (метры)")]
    public float deviceFloorLift = 0.35f;

    [Tooltip("Сдвиг старта: встать перед диваном, не внутри mesh")]
    public Vector3 spawnOffset = new Vector3(0f, 0.15f, -0.5f);

    void Start()
    {
        var origin = GetComponent<XROrigin>();
        if (origin == null) return;

        var t = origin.transform;
        t.position += spawnOffset;

#if UNITY_ANDROID && !UNITY_EDITOR
        t.position += Vector3.up * deviceFloorLift;
        Debug.Log($"[VRQuestFloorOffset] Quest lift +{deviceFloorLift}m, spawn offset {spawnOffset}");
#endif
    }
}
