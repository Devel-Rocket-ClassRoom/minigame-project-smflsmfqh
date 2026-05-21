using UnityEngine;

public class Destination : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Game Clear] 목적지 도착!");
            GameManager.Instance.GameClear();
        }
    }
}
