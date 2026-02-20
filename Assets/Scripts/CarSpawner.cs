using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private CarSpawnConfig spawnConfig;
    [SerializeField] private CarPool carPool;
    [SerializeField] private Transform spawnPoint;

    public void TrySpawn()
    {
        if (!spawnConfig.HasRemaning()) return;

        CarType? type = spawnConfig.GetNextType();
        if (type== null) return;

        Car car = carPool.Get(type.Value);
        if (car == null) return;

        car.transform.position = spawnPoint.position;
    }
}
