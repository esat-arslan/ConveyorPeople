using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PersonSpawner : MonoBehaviour
{
    [SerializeField] private ConveyorPath conveyorPath;
    [SerializeField] private AssignmentSystem assigmentSystem;
    [SerializeField] private PersonPool pool;
    [SerializeField] private PersonSpawnConfig spawnConfig;

    private void Awake()
    {
        spawnConfig.ResetRuntimeData();
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            TrySpawn();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void TrySpawn()
    {
        if (spawnConfig == null) return;
        
        var rule = spawnConfig.GetNextAvaibleRule();
        if (rule == null) return;

        SpawnPerson(rule);
    }

    public void SpawnPerson(PersonSpawnConfig.ColorSpawnRule rule)
    {
        Person person = pool.Get();
        person.transform.position = conveyorPath.StartPosition;
        person.Initialize(conveyorPath, pool, rule.carType);
        person.SetVisual(rule.color);
        
        if (assigmentSystem.TryRegisterPerson(person))
        {
            rule.spawnedCount++;
        }
        else
        {
            pool.Return(person);
        }
    }


}
