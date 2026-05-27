using UnityEngine;
using UnityEngine.AI;

public class CatSpawnManager : MonoBehaviour
{
    [SerializeField]
    private CatMovement _catPrefab;

    [SerializeField]
    private NPCSpawnZone[] _spawnZones;

    [SerializeField]
    private PlayerHealth _player;
    private CatMovement[] _cats;

    private void Awake()
    {
        if (_catPrefab == null)
            return;
    }

    private void Start()
    {
        _cats = new CatMovement[_spawnZones.Length];
        int walkable = 1 << NavMesh.GetAreaFromName("Walkable");

        for (int i = 0; i < _spawnZones.Length; i++)
        {
            if (_spawnZones[i].TryGetSpawnPoint(walkable, out Vector3 pos))
            {
                var cat = Instantiate(_catPrefab, pos, Quaternion.identity);
                cat.SetPlayer(_player);
                _cats[i] = cat;
                Debug.Log($"[고양이 스폰] {pos}에 {i}번째 고양이 스폰 완료");
            }
        }
    }
}
