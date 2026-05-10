using UnityEngine;
using UnityEditor;
using System.IO;

public static class ItemIconGenerator
{
    struct Entry
    {
        public string modelPath;
        public string itemAssetPath;
        public string iconName;
        public Vector3 camDir;   // normalized direction from object center to camera
        public float   fov;
        public float   distMult; // multiplier on bounds diagonal
    }

    static readonly Entry[] Entries =
    {
        new Entry {
            modelPath     = "Assets/models/bottle/bottle.fbx",
            itemAssetPath = "Assets/Items/Item_Mug.asset",
            iconName      = "icon_mug",
            camDir        = new Vector3(0.4f, 0.5f, -1f),
            fov           = 30f,
            distMult      = 1.1f
        },
        new Entry {
            modelPath     = "Assets/models/lighter/Lighter LP.fbx",
            itemAssetPath = "Assets/Items/Item_Lighter.asset",
            iconName      = "icon_lighter",
            camDir        = new Vector3(0.5f, 0.4f, -1f),
            fov           = 30f,
            distMult      = 1.1f
        },
        new Entry {
            modelPath     = "Assets/models/key/key.fbx",
            itemAssetPath = "Assets/Items/Item_Key.asset",
            iconName      = "icon_key",
            camDir        = new Vector3(0.2f, 1f, -0.1f),
            fov           = 40f,
            distMult      = 1.8f
        },
        new Entry {
            modelPath     = "Assets/models/screwdriver/mejsel.fbx",
            itemAssetPath = "Assets/Items/Item_Screwdriver.asset",
            iconName      = "icon_screwdriver",
            camDir        = new Vector3(0.3f, 0.4f, -1f),
            fov           = 28f,
            distMult      = 1.0f
        },
    };

    [MenuItem("Tools/Generate Item Icons")]
    public static void Generate()
    {
        const int size = 128;

        var rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;

        var camGO = new GameObject("__IconCam__");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        cam.orthographic    = false;
        cam.targetTexture   = rt;
        cam.cullingMask     = 1 << 30;
        cam.enabled         = false;

        // Key light  — front-left-top
        var lightGO = new GameObject("__IconLight__");
        var light   = lightGO.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.intensity = 2.2f;
        light.color     = Color.white;
        light.cullingMask = 1 << 30;
        lightGO.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        // Fill light — right, softer
        var fillGO = new GameObject("__IconFill__");
        var fill   = fillGO.AddComponent<Light>();
        fill.type      = LightType.Directional;
        fill.intensity = 0.5f;
        fill.color     = new Color(0.8f, 0.85f, 1f);
        fill.cullingMask = 1 << 30;
        fillGO.transform.rotation = Quaternion.Euler(20f, 150f, 0f);

        foreach (var e in Entries)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(e.modelPath);
            if (model == null) { Debug.LogWarning("Model not found: " + e.modelPath); continue; }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(model);
            SetLayerRecursive(inst, 30);

            var bounds  = GetBounds(inst);
            var center  = bounds.center;
            float diag  = bounds.size.magnitude;

            cam.fieldOfView = e.fov;
            var dir = e.camDir.normalized;
            camGO.transform.position = center + dir * (diag * e.distMult);
            camGO.transform.LookAt(center);

            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            var bytes = tex.EncodeToPNG();
            var path  = "Assets/Items/Icons/" + e.iconName + ".png";
            File.WriteAllBytes(Path.Combine(Application.dataPath, "../" + path), bytes);

            Object.DestroyImmediate(inst);
            Object.DestroyImmediate(tex);
        }

        Object.DestroyImmediate(camGO);
        Object.DestroyImmediate(lightGO);
        Object.DestroyImmediate(fillGO);
        Object.DestroyImmediate(rt);

        AssetDatabase.Refresh();

        foreach (var e in Entries)
        {
            var iconPath = "Assets/Items/Icons/" + e.iconName + ".png";
            var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (importer == null) continue;
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled       = false;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        foreach (var e in Entries)
        {
            var iconPath = "Assets/Items/Icons/" + e.iconName + ".png";
            var sprite   = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(e.itemAssetPath);
            if (sprite != null && itemData != null)
            {
                itemData.icon = sprite;
                EditorUtility.SetDirty(itemData);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Item icons generated and assigned.");
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    static Bounds GetBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.1f);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }
}
