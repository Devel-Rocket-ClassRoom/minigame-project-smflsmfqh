using UnityEngine;
using UnityEngine.UI;

public class CooldownBarUI : MonoBehaviour
{
    [SerializeField] private Image _fill;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
    }

    public void SetFill(float t) => _fill.fillAmount = Mathf.Clamp01(t);
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
