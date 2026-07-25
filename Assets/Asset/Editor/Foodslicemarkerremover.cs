using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// slice 프리팹들에서 SliceMarker 컴포넌트를 일괄 제거하는 툴.
/// (Editor 폴더 안 스크립트에 정의된 MonoBehaviour가 런타임 프리팹에 붙어있으면
///  "Please change the script or remove it from the GameObject" 에러가 발생하므로 정리용)
/// Editor 폴더 안에 넣어야 함.
/// </summary>
public class FoodSliceMarkerRemover : EditorWindow
{
    string rootFolder = "Assets/Resources/MinigamePrefab/CandyPlanet/TempPrefab";
    bool onlySlicePrefabs = true;
    List<GameObject> foundPrefabs = new List<GameObject>();

    [MenuItem("Window/Food Slice Marker Remover")]
    static void OpenWindow()
    {
        var w = GetWindow<FoodSliceMarkerRemover>("Food Slice Marker Remover");
        w.minSize = new Vector2(480, 220);
    }

    void OnGUI()
    {
        GUILayout.Label("스캔 설정", EditorStyles.boldLabel);
        rootFolder = EditorGUILayout.TextField("검색 루트 폴더", rootFolder);
        onlySlicePrefabs = EditorGUILayout.Toggle("이름에 \"_slice_\" 포함된 것만", onlySlicePrefabs);

        GUILayout.Space(8);
        if (GUILayout.Button("1) 스캔 (미리보기)"))
        {
            Scan();
        }

        if (foundPrefabs.Count > 0)
        {
            GUILayout.Space(6);
            GUILayout.Label($"검색된 프리팹: {foundPrefabs.Count}개", EditorStyles.boldLabel);

            GUILayout.Space(8);
            if (GUILayout.Button($"2) 위 {foundPrefabs.Count}개 프리팹에서 SliceMarker 제거"))
            {
                RemoveMarkers();
            }
        }
    }

    void Scan()
    {
        foundPrefabs.Clear();

        if (!AssetDatabase.IsValidFolder(rootFolder))
        {
            EditorUtility.DisplayDialog("Error", $"폴더가 존재하지 않습니다: {rootFolder}", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (onlySlicePrefabs && !prefab.name.Contains("_slice_"))
                continue;

            foundPrefabs.Add(prefab);
        }

        foundPrefabs = foundPrefabs.OrderBy(p => p.name).ToList();
        Debug.Log($"[FoodSliceMarkerRemover] {foundPrefabs.Count}개 프리팹 검색됨 (루트: {rootFolder})");
    }

    void RemoveMarkers()
    {
        int removed = 0;
        int notFound = 0;
        int errors = 0;

        foreach (var prefab in foundPrefabs)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);

            // 컴포넌트 이름으로 찾기 (타입 참조 없이도 동작하도록)
            var comps = contents.GetComponents<MonoBehaviour>();
            bool foundOne = false;
            foreach (var c in comps)
            {
                if (c == null) continue; // missing script
                if (c.GetType().Name == "SliceMarker")
                {
                    Object.DestroyImmediate(c, true);
                    foundOne = true;
                }
            }

            if (foundOne)
            {
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    removed++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"저장 실패: {path}\n{e.Message}");
                    errors++;
                }
            }
            else
            {
                notFound++;
            }

            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            $"SliceMarker 제거 완료.\n제거됨: {removed}개\n원래 없었음: {notFound}개\n실패: {errors}개",
            "OK");
        Debug.Log($"[FoodSliceMarkerRemover] 완료 - 제거 {removed}, 없음 {notFound}, 실패 {errors}");
    }
}