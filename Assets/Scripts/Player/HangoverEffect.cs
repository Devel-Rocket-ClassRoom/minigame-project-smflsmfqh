using UnityEngine;

public class HangoverEffect : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particle;

    private void Start()
    {
        if (_particle != null)
        {
            var main = _particle.main;
            main.useUnscaledTime = true;
            _particle.Play();
        }

        MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
    }

    private void HandleMissionCompleted(ItemData item)
    {
        if (item == null || item.itemName.ToUpper() != "ENERGYDRINK")
            return;

        if (_particle != null)
        {
            _particle.Stop();
            _particle.gameObject.SetActive(false);
        }

        MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
    }
}
