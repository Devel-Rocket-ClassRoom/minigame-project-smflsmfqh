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
            Debug.Log(
                $"[목적지 도착] 모든 미션 수행: {MissionManager.Instance.OnAllMissionCompleted}"
            );

            if (MissionManager.Instance.OnAllMissionCompleted)
            {
                GameManager.Instance.GameClear();
            }
            else
            {
                Debug.Log("[목적지 도착] 미션 수행 실패: 게임 오버!");
                CauseDeath cause = CauseDeath.Mission;
                GameManager.Instance.GameOver(cause);
            }
        }
    }
}
