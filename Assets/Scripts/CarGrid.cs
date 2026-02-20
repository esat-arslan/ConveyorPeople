using System;
using System.Collections.Generic;
using UnityEngine;

public class CarGrid : MonoBehaviour
{
    [SerializeField] private Transform[] gridSlots;
    [SerializeField] private CarPool carPool;
    [SerializeField] private CarSpawnConfig spawnConfig;

    private List<Car> gridCars = new();

    private void Start()
    {
        FillGrid();
    }

    public void FillGrid()
    {
        for (int i = 0; i < gridSlots.Length; i++)
        {
            SpawnCarInSlot(i);
        }
    }

    private void SpawnCarInSlot(int index)
    {
        if (!spawnConfig.HasRemaning()) return;

        CarType? type = spawnConfig.GetNextType();
        if (type == null) return;

        Car car = carPool.Get(type.Value);
        if (car == null) return;

        car.transform.position = gridSlots[index].position;
        car.SetActive(false);
        gridCars.Add(car);

        car.SetSelectable(this);
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
