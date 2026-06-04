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

    public void Show()
    {
        var parent = transform.parent;
        if (parent != null)
        {
            // 부모 스케일 상쇄 + 50% 축소
            Vector3 ps = parent.lossyScale;
            transform.localScale = new Vector3(
                ps.x != 0f ? 0.5f / ps.x : 0.5f,
                ps.y != 0f ? 0.5f / ps.y : 0.5f,
                ps.z != 0f ? 0.5f / ps.z : 0.5f
            );

            var col = parent.GetComponent<Collider>();
            if (col != null)
            {
                // 건물 피벗 → 콜라이더 중심 방향 = 입구 방향 (ShopBuilding과 동일 로직)
                Vector3 center = col.bounds.center;
                Vector3 outDir = center - parent.position;
                outDir.y = 0f;
                outDir = outDir.sqrMagnitude > 0.001f ? outDir.normalized : parent.forward;

                // 콜라이더 바깥 경계(입구 앞)에 배치
                float extent = Mathf.Abs(outDir.x) * col.bounds.extents.x
                             + Mathf.Abs(outDir.z) * col.bounds.extents.z;
                transform.position = new Vector3(
                    center.x + outDir.x * extent,
                    col.bounds.center.y,
                    center.z + outDir.z * extent
                );
            }
        }

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
