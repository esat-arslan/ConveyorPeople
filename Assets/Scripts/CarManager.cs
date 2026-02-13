using System;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager Instance { get; private set; }

    private readonly List<Car> activeCars = new();
    public IReadOnlyList<Car> ActiveCars => activeCars;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(Car car)
    {
        if (!activeCars.Contains(car)) activeCars.Add(car);
    }

    public void Unregister(Car car)
    {
        if (activeCars.Contains(car)) activeCars.Remove(car);
    }

    public void ForDebugging()
    {
        foreach (Car car in activeCars)
        {
            Debug.Log(car.gameObject.GetInstanceID());
        }
    }
}

