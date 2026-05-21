using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptables/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;

    //public Sprite icon;
    public GameObject dropPrefab;
    public ItemCategory category;
    public ItemEffectSO[] effects;
    public string MissionMessage;
}
