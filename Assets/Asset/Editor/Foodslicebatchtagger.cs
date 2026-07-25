using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 이미 생성된 slice 프리팹들(slice_0 ~ slice_N)에 태그를 일괄로 지정하는 툴.
/// FoodSliceGeneratorWindow가 만든 프리팹들을 뒤늦게 태깅할 때 사용.
/// Editor 폴더 안에 넣어야 함 (FoodSliceGeneratorWindow.cs와 같은 위치).
/// </summary>
public class FoodSliceBatchTagger : EditorWindow
{
    string rootFolder = "Assets/Resources/MinigamePrefab/CandyPlanet/TempPrefab";
    string targetTag = "FoodPiece";
    bool onlySlicePrefabs = true; // 이름에 "_slice_" 포함된 것만
    List<GameObject> foundPrefabs = new List<GameObject>();

    [MenuItem("Window/Food Slice Batch Tagger")]
    static void OpenWindow()
    {
        var w = GetWindow<FoodSliceBatchTagger>("Food Slice Batch Tagger");
        w.minSize = new Vector2(480, 260);
    }

    void OnGUI()
    {
        GUILayout.Label("스캔 설정", EditorStyles.boldLabel);
        rootFolder = EditorGUILayout.TextField("검색 루트 폴더", rootFolder);
        targetTag = EditorGUILayout.TextField("지정할 Tag", targetTag);
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

            EditorGUILayout.BeginVertical(GUI.skin.box);
            int shown = 0;
            foreach (var p in foundPrefabs)
            {
                if (shown++ > 15) { GUILayout.Label("... (생략)"); break; }
                GUILayout.Label(p.name);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);
            if (GUILayout.Button($"2) 위 {foundPrefabs.Count}개 프리팹에 Tag \"{targetTag}\" 일괄 적용"))
            {
                ApplyTags();
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
        Debug.Log($"[FoodSliceBatchTagger] {foundPrefabs.Count}개 프리팹 검색됨 (루트: {rootFolder})");
    }

    void ApplyTags()
    {
        // 태그가 실제로 존재하는지 확인 (Tag Manager에 없으면 SetTag 시 에러남)
        if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(targetTag))
        {
            EditorUtility.DisplayDialog("Error",
                $"\"{targetTag}\" 태그가 프로젝트에 존재하지 않습니다.\nEdit > Project Settings > Tags and Layers 에서 먼저 추가해주세요.",
                "OK");
            return;
        }

        int changed = 0;
        int skipped = 0;

        foreach (var prefab in foundPrefabs)
        {
            string path = AssetDatabase.GetAssetPath(prefab);

            // 프리팹을 편집 모드로 로드해서 루트 오브젝트에 태그 지정
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            if (contents.tag == targetTag)
            {
                skipped++;
            }
            else
            {
                contents.tag = targetTag;
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                changed++;
            }
            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            $"태그 적용 완료.\n변경됨: {changed}개\n이미 지정되어 있어 스킵: {skipped}개",
            "OK");
        Debug.Log($"[FoodSliceBatchTagger] 완료 - 변경 {changed}개, 스킵 {skipped}개");
    }
}