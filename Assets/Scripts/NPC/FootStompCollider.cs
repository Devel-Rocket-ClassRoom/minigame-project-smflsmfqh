using System;
using UnityEngine;

public class FootStompCollider : MonoBehaviour
{
    [SerializeField]
    private CauseDeath _cause = CauseDeath.NPC;

    [SerializeField]
    private float _damage = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Foot Stomp] {_cause}가 플레이어 밟음! 데미지: {_damage}");
            other.GetComponent<PlayerHealth>().TakeDamage(_damage, _cause);
        }
    }
}
