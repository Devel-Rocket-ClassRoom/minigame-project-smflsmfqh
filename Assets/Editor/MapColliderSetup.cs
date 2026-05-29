using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MapColliderSetup : EditorWindow
{
    private const float CurbOffsetX  = 6.251141f;
    private const float CurbSizeX    = 0.21118641f;
    private const float CurbSizeY    = 0.19999999f;
    private const float CurbSizeZ    = 15f;

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

    // 연석이 필요한 road 타입 목록 (교차로·특수 형태 제외 시 이 목록에서 제거)
    private static readonly string[] RoadNames =
    {
        "road_001", "road_003", "road_009",
        "road_013", "road_019", "road_020", "road_022"
    };

    [MenuItem("Tools/Copy road_009 Curb to All")]
    public static void CopyRoad009CurbToAll()
    {
        var allGOs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var road009List = new List<GameObject>();
        foreach (var go in allGOs)
            if (go.name == "road_009") road009List.Add(go);

        if (road009List.Count == 0) { Debug.LogWarning("[RoadCurb] road_009 없음"); return; }

        // 첫 번째 road_009에서 non-trigger BoxCollider 설정 수집
        var template = road009List[0];
        var templateBoxes = new List<(Vector3 center, Vector3 size)>();
        foreach (var b in template.GetComponents<BoxCollider>())
            if (!b.isTrigger) templateBoxes.Add((b.center, b.size));

        if (templateBoxes.Count == 0)
        {
            Debug.LogWarning($"[RoadCurb] 첫 번째 road_009({template.name})에 non-trigger BoxCollider 없음");
            return;
        }

        int applied = 0;
        for (int i = 1; i < road009List.Count; i++)
        {
            var go = road009List[i];

            // 기존 non-trigger BoxCollider 전부 제거
            foreach (var b in go.GetComponents<BoxCollider>())
                if (!b.isTrigger) Undo.DestroyObjectImmediate(b);

            // 템플릿 설정 그대로 복사
            foreach (var (center, size) in templateBoxes)
            {
                Undo.RecordObject(go, "Copy road_009 Curb");
                var bc = Undo.AddComponent<BoxCollider>(go);
                bc.isTrigger = false;
                bc.center = center;
                bc.size   = size;
                applied++;
            }
        }

        Debug.Log($"[RoadCurb] road_009 {road009List.Count - 1}개에 BoxCollider {applied}개 복사 완료");
    }

    [MenuItem("Tools/Add Road Curb Colliders")]
    public static void AddRoadCurbColliders()
    {
        var allGOs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var nameSet = new System.Collections.Generic.HashSet<string>(RoadNames);
        int added = 0;

        foreach (var go in allGOs)
        {
            if (!nameSet.Contains(go.name)) continue;

            AddCurbsToRoad(go, ref added);
        }

        Debug.Log($"[RoadCurb] BoxCollider {added}개 추가 완료");
    }

    // 로드 타입별 바닥 콜라이더 설정 (center, size)
    // 없으면 바닥 콜라이더 추가 안 함
    private static readonly System.Collections.Generic.Dictionary<string, (Vector3 center, Vector3 size)> FloorColliders
        = new System.Collections.Generic.Dictionary<string, (Vector3, Vector3)>
    {
        { "road_001", (new Vector3(-0.01f, -0.10f, 0f), new Vector3(12.73f, 0.01f, 15f)) },
        { "road_009", (new Vector3(0f,    -0.09f, 0f), new Vector3( 7.76f, 0.02f, 15f)) },
    };

    private static void AddCurbsToRoad(GameObject go, ref int added)
    {
        // 메시 바운드로 타일 Z 크기 자동 감지
        var renderer = go.GetComponentInChildren<MeshRenderer>();
        float halfZ = renderer != null
            ? go.transform.InverseTransformVector(renderer.bounds.extents).z
            : CurbSizeZ * 0.5f;
        float sizeZ = Mathf.Abs(halfZ) * 2f;
        if (sizeZ < 1f) sizeZ = CurbSizeZ;

        var boxes = go.GetComponents<BoxCollider>();
        bool hasLeft  = false;
        bool hasRight = false;
        bool hasFloor = false;
        foreach (var b in boxes)
        {
            if (b.isTrigger) continue;
            if (b.center.x >  0.1f) hasLeft  = true;
            if (b.center.x < -0.1f) hasRight = true;
            if (b.size.x    >  5f)  hasFloor = true;
        }

        if (!hasLeft)
        {
            Undo.RecordObject(go, "Add Left Curb");
            var bc = Undo.AddComponent<BoxCollider>(go);
            bc.isTrigger = false;
            bc.size   = new Vector3(CurbSizeX, CurbSizeY, sizeZ);
            bc.center = new Vector3(CurbOffsetX, 0f, 0f);
            added++;
        }

        if (!hasRight)
        {
            Undo.RecordObject(go, "Add Right Curb");
            var bc = Undo.AddComponent<BoxCollider>(go);
            bc.isTrigger = false;
            bc.size   = new Vector3(CurbSizeX, CurbSizeY, sizeZ);
            bc.center = new Vector3(-CurbOffsetX, 0f, 0f);
            added++;
        }

        if (!hasFloor && FloorColliders.TryGetValue(go.name, out var floor))
        {
            Undo.RecordObject(go, "Add Floor Collider");
            var bc = Undo.AddComponent<BoxCollider>(go);
            bc.isTrigger = false;
            bc.center = floor.center;
            bc.size   = new Vector3(floor.size.x, floor.size.y, sizeZ);
            added++;
        }
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
