using UnityEngine;

public class FootStompCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[FootStomp] 플레이어 밟힘!");
            other.GetComponent<PlayerHealth>().TakeDamage();
        }
    }
}
