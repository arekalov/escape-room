using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class VRUIFixer
{
    const string NoZTestMatPath =
        "Assets/Samples/XR Interaction Toolkit/3.5.0/Starter Assets/Materials/UI-NoZTest.mat";
    const string TmpOverlayShader = "TextMeshPro/Mobile/Distance Field Overlay";

    [MenuItem("Tools/VR/Apply NoZTest to All World-Space UI")]
    static void Apply()
    {
        var noZMat = AssetDatabase.LoadAssetAtPath<Material>(NoZTestMatPath);
        if (noZMat == null)
        {
            Debug.LogError("[VRUIFixer] UI-NoZTest.mat not found at: " + NoZTestMatPath);
            return;
        }

        var overlayShader = Shader.Find(TmpOverlayShader);
        if (overlayShader == null)
            Debug.LogWarning("[VRUIFixer] TMP overlay shader not found — TMP text will keep original shader.");

        int imgCount = 0, tmpCount = 0;

        var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (var canvas in canvases)
        {
            // Only World Space canvases; skip prefab assets
            if (canvas.renderMode != RenderMode.WorldSpace) continue;
            if (EditorUtility.IsPersistent(canvas)) continue;

            // ── Image / RawImage ──────────────────────────────────────────
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                Undo.RecordObject(img, "Apply NoZTest");
                img.material = noZMat;
                EditorUtility.SetDirty(img);
                imgCount++;
            }
            foreach (var raw in canvas.GetComponentsInChildren<RawImage>(true))
            {
                Undo.RecordObject(raw, "Apply NoZTest");
                raw.material = noZMat;
                EditorUtility.SetDirty(raw);
                imgCount++;
            }

            // ── TextMeshPro ───────────────────────────────────────────────
            if (overlayShader != null)
            {
                foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (tmp.fontSharedMaterial == null) continue;
                    // Create a per-object material instance so we don't alter the shared font asset
                    var inst = new Material(tmp.fontSharedMaterial) { shader = overlayShader };
                    inst.name = tmp.fontSharedMaterial.name + " (Overlay)";
                    Undo.RecordObject(tmp, "Apply TMP Overlay");
                    tmp.fontMaterial = inst;
                    EditorUtility.SetDirty(tmp);
                    tmpCount++;
                }
            }
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log(string.Format("[VRUIFixer] Done — {0} Image/RawImage, {1} TMP texts patched. Save the scene.", imgCount, tmpCount));
    }
}
