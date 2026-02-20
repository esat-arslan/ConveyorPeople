using System.Collections.Generic;
using UnityEngine;

public class CarPool : MonoBehaviour
{
    public static CarPool Instance { get; private set; }

    [SerializeField] private List<Car> carPrefabs;

    private Dictionary<CarType, Queue<Car>> pool = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        foreach (var prefab in carPrefabs)
        {
            if (!pool.ContainsKey(prefab.CarType))
                pool[prefab.CarType] = new Queue<Car>();

            Car car = Instantiate(prefab, transform);
            car.gameObject.SetActive(false);
            pool[prefab.CarType].Enqueue(car);
        }
    }

    public Car Get(CarType type)
    {
        if (!pool.ContainsKey(type) || pool[type].Count == 0)
        {
            Debug.Log($"No pooled car available for {type}");
            return null;
        }

        Car car = pool[type].Dequeue();
        car.gameObject.SetActive(true);
        car.SetActive(true);
        return car;
    }

    public void Return(Car car)
    {
        car.gameObject.SetActive(false);
        pool[car.CarType].Enqueue(car);
    }
}
