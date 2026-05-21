using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoostSO", menuName = "Scriptables/SpeedBoostEffectSO")]
public class SpeedBoostEffectSO : ItemEffectSO
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _duration;

    public override void Apply(PlayerController player)
    {
        player.SetSpeedBoost(_speed, _duration);
    }
}
