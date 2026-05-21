using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    public enum MarkerType { Player, Destination, Shop, Item }

    [SerializeField] public MarkerType type = MarkerType.Item;

    // alpha > 0 이면 이 색으로, 아니면 MinimapUI 기본 색상 사용
    [SerializeField] public Color colorOverride = Color.clear;

    // 할당하면 해당 스프라이트로, 없으면 흰 사각형(Unity 기본)
    [SerializeField] public Sprite iconSprite;

    private void OnEnable()  => MinimapUI.Instance?.Register(this);
    private void OnDisable() => MinimapUI.Instance?.Unregister(this);
}
