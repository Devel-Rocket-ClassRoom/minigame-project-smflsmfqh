using UnityEngine;

[CreateAssetMenu(fileName = "InvincibleSO", menuName = "Scriptables/InvincibleEffectSO")]
public class InvincibleEffectSO : ItemEffectSO
{
    [SerializeField]
    private float _duration;

    public override void Apply(PlayerController player)
    {
        player.SetInvincible(_duration, "item");
    }
}
