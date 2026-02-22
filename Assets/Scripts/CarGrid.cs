using System;
using System.Collections.Generic;
using UnityEngine;

public class CarGrid : MonoBehaviour
{
    [SerializeField] private Transform[] gridSlots;
    [SerializeField] private CarPool carPool;
    [SerializeField] private CarSpawnConfig spawnConfig;

    private List<Car> gridCars = new();

    private void Awake()
    {
        spawnConfig.ResetRuntimeData();
    }

    private void Start()
    {
        FillGrid();
    }

    public void FillGrid()
    {
        Debug.Log($"Filling grid with {gridSlots.Length} slots.");
        for (int i = 0; i < gridSlots.Length; i++)
        {
            SpawnCarInSlot(i);
        }
    }

    private void SpawnCarInSlot(int index)
    {
        if (!spawnConfig.HasRemaning())
        {
            Debug.LogWarning("SpawnConfig has no cars remaining to spawn.");
            return;
        }

        CarType? type = spawnConfig.GetNextType();
        if (type == null)
        {
            Debug.LogWarning("SpawnConfig returned null car type.");
            return;
        }

        CarPool pool = carPool != null ? carPool : CarPool.Instance;
        if (pool == null)
        {
            Debug.LogError("No CarPool found in scene or assigned!");
            return;
        }

        Car car = pool.Get(type.Value);
        if (car == null)
        {
            Debug.LogError($"Failed to get car of type {type.Value} from pool.");
            return;
        }

        car.transform.position = gridSlots[index].position;
        car.gameObject.SetActive(true);
        car.SetActive(false);
        gridCars.Add(car);

        car.SetSelectable(this);
        Debug.Log($"Spawned {type.Value} car in slot {index} at {gridSlots[index].position}");
    }

    public void OnCarSelected(Car car)
    {
        gridCars.Remove(car);
        ActivateCar(car);
        RefillEmptySlot();

    }

    private void ActivateCar(Car car)
    {
        car.SetActive(true);
        //move to zone anim?
    }

    private void RefillEmptySlot()
    {
        for (int i = 0; i < gridSlots.Length; i++)
        {
            bool occupied = false;

            foreach (var car in gridCars)
            {
                if (Vector3.Distance(car.transform.position, gridSlots[i].position) < 0.1f)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                SpawnCarInSlot(i);
                break;
            }
        }
    }
}
