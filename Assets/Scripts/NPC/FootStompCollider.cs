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
            Debug.Log("[FootStomp] 플레이어 밟힘!");
            other.GetComponent<PlayerHealth>().TakeDamage(_damage, _cause);
        }
    }
}
