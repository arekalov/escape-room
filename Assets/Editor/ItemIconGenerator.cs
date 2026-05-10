using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class ItemIconGenerator
{
    const int ICON_SIZE = 128;

    struct Job
    {
        public string modelPath;
        public string iconName;
        public string itemPath;
        public GameObject model;
    }

    static readonly (string model, string icon, string item)[] _config = {
        ("Assets/models/bottle/bottle.fbx",        "icon_bottle",       "Assets/Items/Item_Bottle.asset"),
        ("Assets/models/glass/LP_SF.fbx",            "icon_mug",          "Assets/Items/Item_Mug.asset"),
        ("Assets/models/lighter/Lighter LP.fbx",    "icon_lighter",      "Assets/Items/Item_Lighter.asset"),
        ("Assets/models/key/key.fbx",               "icon_key",          "Assets/Items/Item_Key.asset"),
        ("Assets/models/screwdriver/mejsel.fbx",    "icon_screwdriver",  "Assets/Items/Item_Screwdriver.asset"),
    };

    static List<Job> _pending = new List<Job>();
    static int       _frames;
    static bool      _busy;

    [MenuItem("Tools/Generate Item Icons")]
    public static void Generate()
    {
        if (_busy) { Debug.LogWarning("Already running — wait a moment"); return; }

        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Items/Icons"));
        AssetPreview.SetPreviewTextureCacheSize(32);

        _pending.Clear();
        foreach (var (model, icon, item) in _config)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(model);
            if (go == null) { Debug.LogWarning("Model not found: " + model); continue; }
            AssetPreview.GetAssetPreview(go); // kick off async render
            _pending.Add(new Job { modelPath = model, iconName = icon, itemPath = item, model = go });
        }

        _frames = 0;
        _busy = true;
        EditorApplication.update += Poll;
        Debug.Log("[IconGen] Waiting for previews…");
    }

    static void Poll()
    {
        _frames++;

        bool ready = true;
        foreach (var j in _pending)
            if (AssetPreview.IsLoadingAssetPreview(j.model.GetInstanceID())) { ready = false; break; }

        if (!ready && _frames < 400) return; // wait up to ~400 editor frames (~6-7 s)

        EditorApplication.update -= Poll;
        _busy = false;

        Save();
    }

    static void Save()
    {
        foreach (var j in _pending)
        {
            var preview = AssetPreview.GetAssetPreview(j.model);
            if (preview == null)
            {
                Debug.LogWarning("[IconGen] Preview still null for: " + j.modelPath);
                continue;
            }

            // Blit to a readable RGBA32 texture at ICON_SIZE
            var rt   = RenderTexture.GetTemporary(ICON_SIZE, ICON_SIZE, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(preview, rt);
            RenderTexture.active = rt;
            var copy = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, ICON_SIZE, ICON_SIZE), 0, 0);
            copy.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            var iconPath = "Assets/Items/Icons/" + j.iconName + ".png";
            File.WriteAllBytes(
                Path.Combine(Application.dataPath, "../" + iconPath),
                copy.EncodeToPNG()
            );
            Object.DestroyImmediate(copy);
            Debug.Log("[IconGen] Saved: " + iconPath);
        }

        AssetDatabase.Refresh();

        // Set import settings
        foreach (var j in _pending)
        {
            var iconPath = "Assets/Items/Icons/" + j.iconName + ".png";
            var imp = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (imp == null) continue;
            imp.textureType         = TextureImporterType.Sprite;
            imp.spriteImportMode    = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled       = false;
            imp.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        // Assign sprites to ItemData
        foreach (var j in _pending)
        {
            var sprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Items/Icons/" + j.iconName + ".png");
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(j.itemPath);
            if (sprite == null || itemData == null) continue;
            itemData.icon = sprite;
            EditorUtility.SetDirty(itemData);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[IconGen] Done — all icons generated and assigned.");
    }
}
