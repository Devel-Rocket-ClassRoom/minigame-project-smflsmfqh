using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CatHeadDamage : MonoBehaviour
{
    [SerializeField]
    private float _damage = 10f;

    [SerializeField]
    private float _damageCooldown = 5f;

    private float _lastDamageTime = -999f;

    private void Awake()
    {
        GetComponent<CapsuleCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastDamageTime < _damageCooldown)
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            return;

        _lastDamageTime = Time.time;
        playerHealth.TakeDamage(_damage, CauseDeath.Cat);
    }
}
