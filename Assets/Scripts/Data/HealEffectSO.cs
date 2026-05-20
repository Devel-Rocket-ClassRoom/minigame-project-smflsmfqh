using UnityEngine;

[CreateAssetMenu(fileName = "HealSO", menuName = "Scriptables/HealSO")]
public class HealEffectSO : ItemEffectSO
{
    [SerializeField]
    private int _healAmount;

    public override void Apply(PlayerController player)
    {
        player.Heal(_healAmount);
    }
}
