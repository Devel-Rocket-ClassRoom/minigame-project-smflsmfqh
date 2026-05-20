using UnityEngine;

[CreateAssetMenu(fileName = "NPCVisualData", menuName = "Scriptables/NPCVisualData")]
public class NPCVisualData : ScriptableObject
{
    public Material[] SkinColors;
    public Material[] HairColors;
    public Material[] ClothingColors;
}
