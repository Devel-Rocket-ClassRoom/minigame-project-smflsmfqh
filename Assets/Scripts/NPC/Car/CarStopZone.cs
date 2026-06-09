using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CarStopZone : MonoBehaviour
{
    private readonly List<CarMovement> _carsInZone = new();
    private readonly List<CarMovement> _stoppedCars = new();
    private bool _pedestrianGreen = false;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponent<CarMovement>();
        if (car == null)
            return;

        if (!_carsInZone.Contains(car))
            _carsInZone.Add(car);

        if (_pedestrianGreen)
            StopCar(car);
    }

    private void OnTriggerExit(Collider other)
    {
        var car = other.GetComponent<CarMovement>();
        if (car == null)
            return;

        _carsInZone.Remove(car);
        _stoppedCars.Remove(car);
    }

    // CrosswalkWaitZone이 차량 신호 녹색으로 전환 시 호출
    public void OnSignalCarGreen()
    {
        _pedestrianGreen = false;

        foreach (var car in _stoppedCars)
            if (car != null)
                car.SetSignalStop(false);

        _stoppedCars.Clear();
    }

    // CrosswalkWaitZone이 보행자 신호 녹색으로 전환 시 호출
    public void OnSignalPedestrianGreen()
    {
        _pedestrianGreen = true;

        // 현재 존 안에 있는 차량도 즉시 정지
        foreach (var car in _carsInZone)
            StopCar(car);
    }

    private void StopCar(CarMovement car)
    {
        if (car == null || _stoppedCars.Contains(car))
            return;

        car.SetSignalStop(true);
        _stoppedCars.Add(car);
    }
}
