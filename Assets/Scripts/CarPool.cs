using System.Collections.Generic;
using UnityEngine;

public class CarPool : MonoBehaviour
{
    public static CarPool Instance { get; private set; }

    [SerializeField] private List<Car> carPrefabs;

    private Dictionary<CarType, Queue<Car>> pool = new();
    private Dictionary<CarType, Car> prefabMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate CarPool found, destroying extra component.");
            Destroy(this);
            return;
        }
        Instance = this;

        foreach (var prefab in carPrefabs)
        {
            prefabMap[prefab.CarType] = prefab;
            if (!pool.ContainsKey(prefab.CarType))
                pool[prefab.CarType] = new Queue<Car>();
        }
    }

    public Car Get(CarType type)
    {
        if (!pool.ContainsKey(type))
        {
            Debug.LogError($"No prefab found for {type}");
            return null;
        }

        Car car;
        if (pool[type].Count > 0)
        {
            car = pool[type].Dequeue();
        }
        else
        {
            car = Instantiate(prefabMap[type], transform);
        }

        return car;
    }

    public void Return(Car car)
    {
        car.gameObject.SetActive(false);
        car.ResetState();
        pool[car.CarType].Enqueue(car);
    }
}
