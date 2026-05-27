using UnityEngine;

public class CarDrivingZone : MonoBehaviour
{
    [Header("차선 오프셋 설정")]
    [SerializeField]
    private float _laneWidth;

    private Transform[][] _lanes;

    public int GetWaypointCount(int index) => _lanes[index].Length;

    public int Count => _lanes.Length;

    private void Awake()
    {
        _lanes = new Transform[transform.childCount][];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform lane = transform.GetChild(i);
            _lanes[i] = new Transform[lane.childCount];
            for (int j = 0; j < lane.childCount; j++)
            {
                _lanes[i][j] = lane.GetChild(j);
            }
        }
    }

    public Vector3 GetWaypointPosition(int lane, int waypointIndex)
    {
        return _lanes[lane][waypointIndex].position;
    }

    public Vector3 GetWaypointPosition(int lane, int waypointIndex, float laneOffset)
    {
        Transform wp = _lanes[lane][waypointIndex];
        return wp.position + wp.right * (laneOffset * _laneWidth);
    }

    public Transform GetWaypointTransform(int lane, int waypointIndex)
    {
        return _lanes[lane][waypointIndex];
    }
}