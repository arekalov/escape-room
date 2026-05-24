using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;

/// Ensures XRBodyTransformer picks up the CharacterController on the XROrigin.
/// Needed because XRBodyTransformer.Awake() sometimes runs before the CC is ready.
[DefaultExecutionOrder(100)]
public class XRCollisionFix : MonoBehaviour
{
    void Start()
    {
        var xbt = FindAnyObjectByType<XRBodyTransformer>();
        if (xbt == null) return;

        var t = xbt.GetType();
        var manipField = t.GetField("m_ConstrainedBodyManipulator",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (manipField?.GetValue(xbt) != null) return; // already set

        var origin = FindAnyObjectByType<XROrigin>();
        var cc = origin?.GetComponent<CharacterController>();
        if (cc == null) return;

        // Find CharacterControllerBodyManipulator type in loaded assemblies
        System.Type ccbmType = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            foreach (var tp in asm.GetTypes())
                if (tp.Name == "CharacterControllerBodyManipulator") { ccbmType = tp; break; }
        if (ccbmType == null) return;

        var instance = System.Activator.CreateInstance(ccbmType);
        var ccField = ccbmType.GetField("<characterController>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        ccField?.SetValue(instance, cc);

        manipField.SetValue(xbt, instance);
        t.GetField("m_UsingDynamicConstrainedBodyManipulator",
            BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(xbt, true);

        Debug.Log("[XRCollisionFix] CharacterController connected to XRBodyTransformer.");
    }
}
