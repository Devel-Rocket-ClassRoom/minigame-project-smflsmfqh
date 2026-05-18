using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Vector3 _offset = new(0f, 0.1f, 0.4f);

    [SerializeField] private float _mouseSensitivity = 3f;
    [SerializeField] private float _scrollSensitivity = 5f;
    [SerializeField] private float _smoothTime = 0.1f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 20f;
    [SerializeField] private float _minPitch = -20f;
    [SerializeField] private float _maxPitch = 80f;

    private float _yaw;
    private float _pitch;
    private float _distance;
    private Vector3 _velocity;

    private void Awake()
    {
        Vector2 horizontal = new(_offset.x, _offset.z);
        _distance = _offset.magnitude;
        _yaw = Mathf.Atan2(_offset.x, _offset.z) * Mathf.Rad2Deg;
        _pitch = Mathf.Atan2(_offset.y, horizontal.magnitude) * Mathf.Rad2Deg;

        if (_player != null)
            transform.position = _player.position + _offset;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (_player == null) return;

        _yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        _distance -= Input.GetAxis("Mouse ScrollWheel") * _scrollSensitivity;
        _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 targetPos = _player.position + rotation * new Vector3(0f, 0f, -_distance);

        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPos, 
            ref _velocity, 
            _smoothTime);

        transform.LookAt(_player.position);

        _player.rotation = Quaternion.Euler(0f, _yaw, 0f);
    }
}
