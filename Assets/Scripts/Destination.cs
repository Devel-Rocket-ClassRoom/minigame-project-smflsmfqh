using UnityEngine;

public class Destination : MonoBehaviour
{
    private void Awake()
    {
        if (!TryGetComponent<MinimapMarker>(out _))
        {
            var marker = gameObject.AddComponent<MinimapMarker>();
            marker.type = MinimapMarker.MarkerType.Destination;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MissionManager.Instance.OnAllMissionCompleted)
            {
                GameManager.Instance.GameClear();
            }
            else
            {
                CauseDeath cause = CauseDeath.Mission;
                GameManager.Instance.GameOver(cause);
            }
        }
    }
}
