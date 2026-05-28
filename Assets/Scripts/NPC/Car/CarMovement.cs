using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private enum CarState
    {
        Running,
        Stop,
    }

    [Header("경로점 (Inspector 직접 지정 시)")]
    [SerializeField]
    private CarDrivingZone _path;

    [SerializeField]
    private int _laneIndex = 0;

    [SerializeField]
    private float _laneOffset = 0f;
    private int _waypointIndex = 0;

    [Header("이동 설정")]
    [SerializeField]
    private float _carSpeed = 5f;

    [SerializeField]
    private float _rotationSpeed = 8f;

    [SerializeField]
    private float _waypointReachDistance = 0.5f;

    [Header("정지 설정")]
    [SerializeField]
    private float _waitTime = 7f;

    private CarState _state = CarState.Running;
    private float _waitTimer;
    private CarStopZone _lastStopZone;
    private Vector3 _currentDestination;
    private bool _initialized = false;

    private void Start()
    {
        if (_path != null && !_initialized)
            InitializeMovement();
    }

    public void Initialize(
        CarDrivingZone path,
        int laneIndex,
        int startWaypointIndex,
        float laneOffset = 0f
    )
    {
        _path = path;
        _laneIndex = laneIndex;
        _laneOffset = laneOffset;
        _waypointIndex = startWaypointIndex;
        InitializeMovement();
    }

    private void InitializeMovement()
    {
        _currentDestination = GetWaypointPosition();
        AlignToNextWaypoint();
        _initialized = true;
    }

    private void AlignToNextWaypoint()
    {
        Vector3 dir = _currentDestination - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private void Update()
    {
        if (!_initialized)
            return;

        switch (_state)
        {
            case CarState.Running:
                DriveCar();
                break;
            case CarState.Stop:
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    _waitTimer = 0f;
                    _state = CarState.Running;
                    _lastStopZone = null;
                }
                break;
        }
    }

    private void DriveCar()
    {
        if (_path == null || _path.Count == 0)
            return;

        Vector3 flatDest = new(_currentDestination.x, transform.position.y, _currentDestination.z);

        Vector3 dir = flatDest - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * _rotationSpeed
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            _currentDestination,
            _carSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _currentDestination) <= _waypointReachDistance)
        {
            _waypointIndex = (_waypointIndex + 1) % _path.GetWaypointCount(_laneIndex);
            _currentDestination = GetWaypointPosition();
        }
    }

    public void OnEnterStopZone(CarStopZone zone)
    {
        if (_lastStopZone == zone)
            return;

        _lastStopZone = zone;
        _state = CarState.Stop;
        _waitTimer = _waitTime;
    }

    private Vector3 GetWaypointPosition() =>
        _path.GetWaypointPosition(_laneIndex, _waypointIndex, _laneOffset);
}
