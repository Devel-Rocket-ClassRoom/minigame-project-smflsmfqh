using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    public enum MarkerType { Player, Destination, Shop, Item }

    [SerializeField] public MarkerType type = MarkerType.Item;

    [SerializeField] public Color colorOverride = Color.clear;

    [SerializeField] public Sprite iconSprite;

    private void OnEnable()  => MinimapUI.Instance?.Register(this);
    private void OnDisable() => MinimapUI.Instance?.Unregister(this);
}
