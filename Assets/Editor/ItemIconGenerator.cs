using UnityEngine;
using UnityEditor;
using System.IO;

public static class ItemIconGenerator
{
    [MenuItem("Tools/Generate Item Icons")]
    public static void Generate()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Items/Icons"));

        GenerateColorIcon("icon_bottle",      new Color(0.10f, 0.65f, 0.05f, 1f), "Assets/Items/Item_Bottle.asset");
        GenerateColorIcon("icon_mug",         new Color(0.55f, 0.52f, 0.48f, 1f), "Assets/Items/Item_Mug.asset");
        GenerateTextureIcon("icon_lighter",   "Assets/models/lighter/Lighter LP Col+Alpha.png", "Assets/Items/Item_Lighter.asset");
        GenerateTextureIcon("icon_key",       "Assets/models/key/metal11_diffuse.jpg",           "Assets/Items/Item_Key.asset");
        GenerateTextureIcon("icon_screwdriver","Assets/models/screwdriver/mejsel_Screwdriver_BaseColor.png", "Assets/Items/Item_Screwdriver.asset");

        AssetDatabase.Refresh();

        // Import as Sprite
        string[] icons = { "icon_bottle", "icon_mug", "icon_lighter", "icon_key", "icon_screwdriver" };
        foreach (var name in icons)
        {
            var iconPath = "Assets/Items/Icons/" + name + ".png";
            var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (importer == null) continue;
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled       = false;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        // Assign to ItemData
        string[] itemPaths = {
            "Assets/Items/Item_Bottle.asset",
            "Assets/Items/Item_Mug.asset",
            "Assets/Items/Item_Lighter.asset",
            "Assets/Items/Item_Key.asset",
            "Assets/Items/Item_Screwdriver.asset"
        };
        string[] iconNames = { "icon_bottle", "icon_mug", "icon_lighter", "icon_key", "icon_screwdriver" };

        for (int i = 0; i < itemPaths.Length; i++)
        {
            var sprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Items/Icons/" + iconNames[i] + ".png");
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemPaths[i]);
            if (sprite != null && itemData != null)
            {
                itemData.icon = sprite;
                EditorUtility.SetDirty(itemData);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Icons generated.");
    }

    static void GenerateColorIcon(string iconName, Color baseColor, string itemPath)
    {
        const int SIZE = 128;
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        float half = SIZE * 0.5f;

        for (int y = 0; y < SIZE; y++)
        for (int x = 0; x < SIZE; x++)
        {
            float nx = (x - half) / half;
            float ny = (y - half) / half;
            float dist     = Mathf.Sqrt(nx * nx + ny * ny);
            float vignette  = 1f - Mathf.Clamp01(dist * 0.5f);
            float highlight = Mathf.Clamp01(1f - (nx * 0.3f + ny * 0.3f + 0.25f));

            var c = new Color(
                Mathf.Clamp01(baseColor.r * (vignette * 0.65f + 0.35f) + highlight * 0.22f),
                Mathf.Clamp01(baseColor.g * (vignette * 0.65f + 0.35f) + highlight * 0.22f),
                Mathf.Clamp01(baseColor.b * (vignette * 0.65f + 0.35f) + highlight * 0.22f),
                1f
            );
            tex.SetPixel(x, y, c);
        }

        tex.Apply();
        SaveIcon(tex, iconName);
        Object.DestroyImmediate(tex);
    }

    static void GenerateTextureIcon(string iconName, string texturePath, string itemPath)
    {
        var srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (srcTex == null)
        {
            Debug.LogWarning("Texture not found: " + texturePath);
            GenerateColorIcon(iconName, new Color(0.5f, 0.5f, 0.5f), itemPath);
            return;
        }

        var rt = RenderTexture.GetTemporary(128, 128, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(srcTex, rt);
        RenderTexture.active = rt;
        var copy = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        copy.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        SaveIcon(copy, iconName);
        Object.DestroyImmediate(copy);
    }

    static void SaveIcon(Texture2D tex, string iconName)
    {
        var path  = "Assets/Items/Icons/" + iconName + ".png";
        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(Application.dataPath, "../" + path), bytes);
    }
}
