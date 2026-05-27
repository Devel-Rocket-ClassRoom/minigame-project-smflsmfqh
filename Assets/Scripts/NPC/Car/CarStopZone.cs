using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CarStopZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponent<CarMovement>();
        if (car != null)
            car.OnEnterStopZone(this);
    }
}
