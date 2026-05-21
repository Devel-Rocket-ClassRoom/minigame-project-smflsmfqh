using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MapColliderSetup : EditorWindow
{
    private GameObject _targetRoot;
    private bool _skipTriggers = true;
    private bool _previewOnly = false;
    private Vector2 _scroll;
    private List<string> _previewList = new List<string>();

    [MenuItem("Tools/Map Collider Setup")]
    public static void ShowWindow()
    {
        GetWindow<MapColliderSetup>("Map Collider Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("맵 MeshCollider 일괄 추가", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _targetRoot = (GameObject)EditorGUILayout.ObjectField(
            "대상 루트 오브젝트", _targetRoot, typeof(GameObject), true);

        _skipTriggers = EditorGUILayout.Toggle("기존 Trigger는 건드리지 않음", _skipTriggers);
        _previewOnly = EditorGUILayout.Toggle("미리보기만 (실제 추가 안 함)", _previewOnly);

        EditorGUILayout.Space();

        if (_targetRoot == null)
        {
            EditorGUILayout.HelpBox("씬에서 PostProcessingVolume(맵 루트)을 드래그하세요.", MessageType.Info);
            return;
        }

        if (GUILayout.Button(_previewOnly ? "미리보기 실행" : "MeshCollider 추가 실행"))
            Run();

        if (_previewList.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label($"대상 오브젝트 {_previewList.Count}개:", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(300));
            foreach (var s in _previewList)
                EditorGUILayout.LabelField(s, EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }
    }

    private void Run()
    {
        _previewList.Clear();
        var renderers = _targetRoot.GetComponentsInChildren<MeshRenderer>(true);
        int addedCount = 0;

        foreach (var r in renderers)
        {
            var go = r.gameObject;

            // 이미 Collider 있으면 건너뜀 (Trigger 포함 여부에 따라)
            var existing = go.GetComponent<Collider>();
            if (existing != null)
            {
                if (_skipTriggers && existing.isTrigger)
                    continue; // trigger는 건드리지 않음
                if (!existing.isTrigger)
                    continue; // 실제 콜라이더가 이미 있으면 건너뜀
            }

            var mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                continue;

            _previewList.Add(GetPath(go));

            if (!_previewOnly)
            {
                Undo.RecordObject(go, "Add MeshCollider");
                var mc = Undo.AddComponent<MeshCollider>(go);
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false;
                addedCount++;
            }
        }

        if (!_previewOnly)
        {
            EditorUtility.SetDirty(_targetRoot);
            Debug.Log($"[MapColliderSetup] MeshCollider {addedCount}개 추가 완료");
        }
        else
        {
            Debug.Log($"[MapColliderSetup] 미리보기: 추가될 오브젝트 {_previewList.Count}개");
        }
    }

    private static string GetPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        int d = 0;
        while (t != null && d < 4)
        {
            path = t.name + "/" + path;
            t = t.parent;
            d++;
        }
        return path;
    }
}
