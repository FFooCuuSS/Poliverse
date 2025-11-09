using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FoodSliceGeneratorWindow : EditorWindow
{
    Texture2D sourceTexture;
    int sliceCount = 8;
    int exportSize = 512; // 생성할 텍스처 크기 (정사각)
    string exportFolder = "Assets/Resources/MinigamePrefab/CandyPlanet/TempPrefab/EJU/GeneratedSlices";
    string prefabFolder = "Assets/Resources/MinigamePrefab/CandyPlanet/TempPrefab/EJU/GeneratedSlices/Prefabs";
    float colliderRadius = 0.5f; // Sprite units (할당 후 Scale 사용 가능)
    int arcSubdivision = 8; // 호를 근사할 점 개수

    [MenuItem("Window/Slice Food Creator")]
    static void OpenWindow()
    {
        FoodSliceGeneratorWindow w = GetWindow<FoodSliceGeneratorWindow>("Slice Food Creator");
        w.minSize = new Vector2(420, 220);
    }

    void OnGUI()
    {
        GUILayout.Label("Source & Settings", EditorStyles.boldLabel);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
        sliceCount = EditorGUILayout.IntField("Slice Count", Mathf.Max(1, sliceCount));
        exportSize = EditorGUILayout.IntField("Export Size(px)", Mathf.Max(32, exportSize));
        arcSubdivision = EditorGUILayout.IntSlider("Arc Subdivision (collider)", arcSubdivision, 3, 32);

        GUILayout.Space(8);
        GUILayout.Label("Output Folders", EditorStyles.boldLabel);
        exportFolder = EditorGUILayout.TextField("Image Folder", exportFolder);
        prefabFolder = EditorGUILayout.TextField("Prefab Folder", prefabFolder);

        GUILayout.Space(8);
        if (GUILayout.Button("Generate Slices"))
        {
            if (sourceTexture == null)
            {
                EditorUtility.DisplayDialog("Error", "Source Texture is required.", "OK");
                return;
            }
            Generate();
        }
    }

    void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(exportFolder))
        {
            Directory.CreateDirectory(exportFolder);
            AssetDatabase.Refresh();
        }
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
            AssetDatabase.Refresh();
        }
    }

    void Generate()
    {
        EnsureFolders();

        // Make texture readable & set to sprite single
        string srcPath = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter ti = AssetImporter.GetAtPath(srcPath) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError("Can't get TextureImporter for source.");
            return;
        }
        bool reimportNeeded = false;
        if (!ti.isReadable) { ti.isReadable = true; reimportNeeded = true; }
        if (ti.textureType != TextureImporterType.Default && ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Default; reimportNeeded = true; }
        if (reimportNeeded) { AssetDatabase.ImportAsset(srcPath, ImportAssetOptions.ForceUpdate); }

        Color[] srcPixels = sourceTexture.GetPixels();
        int srcW = sourceTexture.width;
        int srcH = sourceTexture.height;
        // center in pixel coords
        Vector2 center = new Vector2(srcW / 2f, srcH / 2f);
        float maxRadius = Mathf.Min(srcW, srcH) * 0.5f;

        for (int i = 0; i < sliceCount; i++)
        {
            float startAngle = 360f / sliceCount * i;
            float endAngle = 360f / sliceCount * (i + 1);

            Texture2D outTex = new Texture2D(exportSize, exportSize, TextureFormat.ARGB32, false);
            outTex.wrapMode = TextureWrapMode.Clamp;

            // For mapping between outTex pixels and source pixels
            for (int y = 0; y < exportSize; y++)
            {
                for (int x = 0; x < exportSize; x++)
                {
                    // map (x,y) in outTex to src coords
                    float nx = (x + 0.5f) / exportSize; // 0..1
                    float ny = (y + 0.5f) / exportSize;
                    // map to source pixel coords centered
                    float sx = (nx - 0.5f) * srcW + center.x;
                    float sy = (ny - 0.5f) * srcH + center.y;

                    Color outC = new Color(0, 0, 0, 0);
                    // bounds
                    if (sx >= 0 && sx < srcW && sy >= 0 && sy < srcH)
                    {
                        float dx = sx - center.x;
                        float dy = sy - center.y;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist <= maxRadius)
                        {
                            float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                            if (ang < 0) ang += 360f;
                            bool inSector = AngleInRange(ang, startAngle, endAngle);
                            if (inSector)
                            {
                                // sample nearest pixel from source
                                int ix = Mathf.Clamp(Mathf.FloorToInt(sx), 0, srcW - 1);
                                int iy = Mathf.Clamp(Mathf.FloorToInt(sy), 0, srcH - 1);
                                Color sc = sourceTexture.GetPixel(ix, iy);
                                outC = sc;
                            }
                            else
                            {
                                outC = new Color(0, 0, 0, 0);
                            }
                        }
                        else
                        {
                            outC = new Color(0, 0, 0, 0);
                        }
                    }
                    outTex.SetPixel(x, y, outC);
                }
            }
            outTex.Apply();

            // Save PNG
            byte[] png = outTex.EncodeToPNG();
            string baseName = Path.GetFileNameWithoutExtension(srcPath);
            string path = $"{exportFolder}/{baseName}_slice_{i}.png";
            File.WriteAllBytes(path, png);

            // Import asset and configure as Sprite
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter ti2 = AssetImporter.GetAtPath(path) as TextureImporter;
            ti2.textureType = TextureImporterType.Sprite;
            ti2.spriteImportMode = SpriteImportMode.Single;
            ti2.isReadable = true;
            ti2.mipmapEnabled = false;
            ti2.alphaIsTransparency = true;
            ti2.spritePivot = new Vector2(0.5f, 0.5f);
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Load sprite
            Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                // create sprite asset from texture
                sprite = Sprite.Create(importedTex, new Rect(0, 0, importedTex.width, importedTex.height), new Vector2(0.5f, 0.5f), 100f);
                // Note: Sprite.Create returns a runtime sprite; to make it an asset would require different handling.
            }

            // Create GameObject and add components
            GameObject go = new GameObject($"{baseName}_slice_{i}");
            go.transform.position = Vector3.zero;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;

            // Add PolygonCollider2D approximating sector (center + arc points)
            PolygonCollider2D poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;

            List<Vector2> pts = new List<Vector2>();
            // center is 0,0 in local space because sprite pivot is center
            pts.Add(Vector2.zero);

            // compute arc points in local sprite units (normalized)
            float pixelRadius = Mathf.Min(sprite.rect.width, sprite.rect.height) * 0.5f;
            for (int s = 0; s <= arcSubdivision; s++)
            {
                float t = (float)s / arcSubdivision;
                float a = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                // convert polar to local (units in pixels)
                float px = Mathf.Cos(a) * pixelRadius;
                float py = Mathf.Sin(a) * pixelRadius;
                // convert pixel coords to sprite units (units = pixels / pixelsPerUnit)
                float ppu = sprite.pixelsPerUnit;
                pts.Add(new Vector2(px / ppu, py / ppu));
            }
            // Close back to center omitted because polygon points list should be non-duplicating
            poly.points = pts.ToArray();

            // Add Slice marker component
            var sliceScript = go.AddComponent<SliceMarker>();
            sliceScript.sliceIndex = i;

            // Save as prefab
            string prefabPath = $"{prefabFolder}/{go.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            GameObject.DestroyImmediate(go);
            Debug.Log($"Created slice prefab: {prefabPath}");
        } // end for

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Generated {sliceCount} slices in {exportFolder} and prefabs in {prefabFolder}.", "OK");
    }

    bool AngleInRange(float angle, float start, float end)
    {
        // handles wrapping
        if (start <= end) return angle >= start && angle < end;
        return angle >= start || angle < end;
    }
}

// Simple marker script for runtime behavior; can be moved to normal assets folder (not Editor).
public class SliceMarker : MonoBehaviour
{
    public int sliceIndex = 0;

    // Example runtime behavior: hide when bite zone enters
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBiteZone"))
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}
