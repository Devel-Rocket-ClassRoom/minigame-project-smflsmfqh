using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement _player;

    [SerializeField]
    private float _scrollSensitivity = 5f;

    [SerializeField]
    private float _smoothTime = 1f;

    [SerializeField]
    private Vector3 _offset = new Vector3(0f, 0.05f, -0.35f);
    private Vector3 _velocity;
    private float _defaultDistance;
    private float _currentDistance;

    [SerializeField]
    private float _maxZoomOut = 3f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _defaultDistance = _offset.magnitude;
        _currentDistance = _defaultDistance;
    }

    private void LateUpdate()
    {
        if (_player == null)
            return;

        _currentDistance -= Input.GetAxis("Mouse ScrollWheel") * _scrollSensitivity;
        _currentDistance = Mathf.Clamp(
            _currentDistance,
            _defaultDistance,
            _defaultDistance + _maxZoomOut
        );

        Quaternion rotation = Quaternion.Euler(_player.Pitch, _player.Yaw, 0f);
        Vector3 targetPos =
            _player.transform.position + rotation * (_offset.normalized * _currentDistance);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref _velocity,
            _smoothTime
        );

        transform.rotation = rotation;
    }
}
