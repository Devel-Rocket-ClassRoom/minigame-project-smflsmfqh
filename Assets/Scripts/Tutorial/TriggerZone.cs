using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public event System.Action OnPlayerEntered;
    public bool Triggered => _triggered;

    private bool _triggered;

    private void OnTriggerEnter(Collider other) => TryTrigger(other);

    private void OnTriggerStay(Collider other) => TryTrigger(other);

    private void TryTrigger(Collider other)
    {
        if (_triggered || !other.CompareTag("Player"))
            return;
        _triggered = true;
        OnPlayerEntered?.Invoke();
        gameObject.SetActive(false);
    }
}
