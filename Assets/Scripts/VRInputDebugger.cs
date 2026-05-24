using UnityEngine;
using UnityEngine.InputSystem;

// Attach to any active GO. Logs whenever controller/input state changes.
public class VRInputDebugger : MonoBehaviour
{
    GameObject    _rightCtrl;
    MonoBehaviour _vrpi;
    MonoBehaviour _dmp;

    bool _prevCtrlActive;
    bool _prevVrpiEnabled;
    bool _prevDmpEnabled;
    bool _prevMoveEnabled;

    InputAction _moveAction;

    void Start()
    {
        _rightCtrl = GameObject.Find("Right Controller");
        _vrpi = FindFirstObjectByType<VRPlayerInteraction>(FindObjectsInactive.Include);
        _dmp  = (MonoBehaviour)FindAnyObjectByType(
            System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.DynamicMoveProvider, Unity.XR.Interaction.Toolkit")
            ?? typeof(MonoBehaviour));

        var mgr = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>();
        if (mgr != null && mgr.actionAssets.Count > 0)
            _moveAction = mgr.actionAssets[0].FindAction("XRI Right Locomotion/Move", true);

        Snapshot(out _prevCtrlActive, out _prevVrpiEnabled, out _prevDmpEnabled, out _prevMoveEnabled);
        Debug.Log("[VRDebug] Monitoring started. RC=" + _prevCtrlActive +
                  " VRPI=" + _prevVrpiEnabled + " DMP=" + _prevDmpEnabled +
                  " MoveEnabled=" + _prevMoveEnabled);
    }

    void Update()
    {
        Snapshot(out var ctrlActive, out var vrpiEnabled, out var dmpEnabled, out var moveEnabled);

        if (ctrlActive  != _prevCtrlActive)
            Debug.LogWarning("[VRDebug] RightController.activeInHierarchy: " + _prevCtrlActive + " -> " + ctrlActive,
                _rightCtrl);
        if (vrpiEnabled != _prevVrpiEnabled)
            Debug.LogWarning("[VRDebug] VRPlayerInteraction.enabled: " + _prevVrpiEnabled + " -> " + vrpiEnabled,
                _vrpi);
        if (dmpEnabled != _prevDmpEnabled)
            Debug.LogWarning("[VRDebug] DynamicMoveProvider.enabled: " + _prevDmpEnabled + " -> " + dmpEnabled,
                _dmp);
        if (moveEnabled != _prevMoveEnabled)
            Debug.LogWarning("[VRDebug] MoveAction.enabled: " + _prevMoveEnabled + " -> " + moveEnabled);

        _prevCtrlActive  = ctrlActive;
        _prevVrpiEnabled = vrpiEnabled;
        _prevDmpEnabled  = dmpEnabled;
        _prevMoveEnabled = moveEnabled;
    }

    void Snapshot(out bool ctrlActive, out bool vrpiEnabled, out bool dmpEnabled, out bool moveEnabled)
    {
        ctrlActive  = _rightCtrl != null && _rightCtrl.activeInHierarchy;
        vrpiEnabled = _vrpi != null && _vrpi.enabled && _vrpi.gameObject.activeInHierarchy;
        dmpEnabled  = _dmp  != null && _dmp.enabled  && _dmp.gameObject.activeInHierarchy;
        moveEnabled = _moveAction != null && _moveAction.enabled;
    }
}
