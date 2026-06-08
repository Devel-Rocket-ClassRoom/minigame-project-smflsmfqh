using UnityEngine;

public class Destination : MonoBehaviour
{
    [SerializeField]
    private MissionMessageUI _messageUI;

    // 경고 메시지 중복 발화 방지 (나갔다 다시 들어와야 재발화)
    private bool _warningShown;

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
        if (!other.CompareTag("Player"))
            return;

        if (MissionManager.Instance.OnAllMissionCompleted)
        {
            GameManager.Instance.GameClear();
        }
        else if (MissionManager.Instance.HasUnassignedMissions)
        {
            // 아직 아내가 심부름을 다 시키지 않은 상태 → 경고 메시지
            if (!_warningShown)
            {
                _warningShown = true;
                _messageUI?.EnqueueTutorialMessage("WARN_EARLY_RETURN", displayDuration: 5f);
            }
        }
        else
        {
            // 미션이 모두 할당됐으나 완료 못 한 경우
            GameManager.Instance.GameOver(CauseDeath.Mission);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _warningShown = false;
    }
}
