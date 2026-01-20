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
            yield return new WaitForSeconds(1f);
        }
    }

    public void TrySpawn()
    {
        Person person = pool.Get();
        person.transform.position = conveyorPath.StartPosition;
        person.Initialize(conveyorPath, pool);
        if (!assigmentSystem.TryRegisterPerson(person))
        {
            pool.Return(person);
            return;
        }
    }

    public void SpawnPerson(PersonSpawnConfig.ColorSpawnRule rule)
    {
        Person person = pool.Get();
        person.transform.position = conveyorPath.StartPosition;
        person.GetComponent<SpriteRenderer>().color = rule.color;
        person.Initialize(conveyorPath, pool);
        assigmentSystem.TryRegisterPerson(person);
        rule.spawnedCount++;
    }


}
