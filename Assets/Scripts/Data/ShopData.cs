using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Scriptables/ShopData")]
public class ShopData : ScriptableObject
{
    public string shopName;
    public ItemData[] dropItems;
}
