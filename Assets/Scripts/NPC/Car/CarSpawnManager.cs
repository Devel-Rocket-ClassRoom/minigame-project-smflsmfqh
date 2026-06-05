using System.Collections.Generic;
using UnityEngine;

public class CarSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class LaneSpawnConfig
    {
        public CarDrivingZone drivingZone;
        public int laneIndex = 0;
        public float laneOffset = 0f;
        [Min(1)] public int spawnCount = 1;
    }

    [Header("차량 프리팹")]
    [SerializeField] private CarMovement[] _carPrefabs;

    [Header("경적 사운드")]
    [SerializeField] private AudioClip _hornClip;

    [Header("차선별 스폰 설정")]
    [SerializeField] private LaneSpawnConfig[] _laneConfigs;

    [Header("스폰 간격")]
    [Min(1)] [SerializeField] private int _minWaypointGap = 2;

    private void Start()
    {
        SpawnAllCars();
    }

    private void SpawnAllCars()
    {
        if (_carPrefabs == null || _carPrefabs.Length == 0)
        {
            Debug.LogWarning("[CarSpawnManager] Car 프리팹이 지정되지 않았습니다.");
            return;
        }

        foreach (LaneSpawnConfig config in _laneConfigs)
        {
            if (config.drivingZone == null)
            {
                Debug.LogWarning("[CarSpawnManager] drivingZone이 비어 있는 차선 설정이 있습니다.");
                continue;
            }

            int totalWaypoints = config.drivingZone.GetWaypointCount(config.laneIndex);
            if (totalWaypoints == 0)
                continue;

            SpawnCarsInLane(config, totalWaypoints);
        }
    }

    private void SpawnCarsInLane(LaneSpawnConfig config, int totalWaypoints)
    {
        int maxSpawnable = totalWaypoints / Mathf.Max(_minWaypointGap, 1);
        int count = Mathf.Clamp(config.spawnCount, 1, Mathf.Max(maxSpawnable, 1));

        List<int> usedIndices = new();

        for (int i = 0; i < count; i++)
        {
            int startIndex = PickRandomIndex(totalWaypoints, usedIndices);
            if (startIndex < 0)
            {
                Debug.LogWarning($"[CarSpawnManager] '{config.drivingZone.name}' lane{config.laneIndex}: " +
                                 $"웨이포인트 간격 부족으로 {i + 1}번째 차량 스폰 생략");
                break;
            }

            CarMovement prefab = _carPrefabs[Random.Range(0, _carPrefabs.Length)];
            Vector3 spawnPos = config.drivingZone.GetWaypointPosition(config.laneIndex, startIndex, config.laneOffset);
            CarMovement car = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
            if (_hornClip != null)
                car.SetHornClip(_hornClip);
            car.Initialize(config.drivingZone, config.laneIndex, startIndex, config.laneOffset);
        }
    }

    private int PickRandomIndex(int total, List<int> usedIndices)
    {
        List<int> candidates = new(total);
        for (int i = 0; i < total; i++)
            candidates.Add(i);

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        foreach (int candidate in candidates)
        {
            if (!IsTooClose(candidate, usedIndices, total))
            {
                usedIndices.Add(candidate);
                return candidate;
            }
        }

        return -1;
    }

    private bool IsTooClose(int candidate, List<int> usedIndices, int total)
    {
        foreach (int used in usedIndices)
        {
            int dist = Mathf.Abs(candidate - used);
            int circularDist = Mathf.Min(dist, total - dist);
            if (circularDist < _minWaypointGap)
                return true;
        }
        return false;
    }
}
