using UnityEngine;

// --- 아이템별 이펙트가 필요한 경우 사용할 예정 ---
// 아이스크림 => 무적
// 피로회복제 => 무적
public abstract class ItemEffectSO : ScriptableObject
{
    public abstract void Apply(PlayerController player);
}
