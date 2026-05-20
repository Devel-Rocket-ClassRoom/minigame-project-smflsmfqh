using UnityEngine;

public class NPCVisualizeCustomizer : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer _bodyRenderer;

    [SerializeField]
    private SkinnedMeshRenderer _clothingRenderer;

    [SerializeField]
    private SkinnedMeshRenderer _hairRenderer;

    private readonly float[] _hairWeights =
    {
        0.5f,
        0.3f,
        0.3f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
    };
    private readonly float[] _clothingWeights =
    {
        0.3f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
    };

    public void ApplyRandomVisuals(NPCVisualData data)
    {
        if (data.SkinColors.Length > 0)
        {
            _bodyRenderer.sharedMaterial = data.SkinColors[Random.Range(0, data.SkinColors.Length)];
        }

        if (data.HairColors.Length > 0)
        {
            _hairRenderer.sharedMaterial = data.HairColors[PickWeighted(_hairWeights)];
        }

        if (data.ClothingColors.Length > 0)
        {
            _clothingRenderer.sharedMaterial = data.ClothingColors[PickWeighted(_clothingWeights)];
        }
    }

    static int PickWeighted(float[] weights)
    {
        float total = 0f;
        foreach (var w in weights)
            total += w;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return i;
        }
        return weights.Length - 1;
    }
}
