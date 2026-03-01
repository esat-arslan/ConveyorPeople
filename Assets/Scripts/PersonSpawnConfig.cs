using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonSpawnConfig", menuName = "Spawning/PersonSpawnConfig")]
public class PersonSpawnConfig : ScriptableObject
{
    [System.Serializable]
    public class PersonSpawnRule
    {
        public CarType carType;
        public int totalToSpawn;
        public Person prefab;
        [HideInInspector] public int spawnedCount;
    }

    public List<PersonSpawnRule> spawnRules = new();
    
    public bool SpawnRemaningAvailable()
    {
        foreach (var rule in spawnRules)
        {
            if (rule.spawnedCount < rule.totalToSpawn) return true;
        }
        return false;
    }

    public PersonSpawnRule GetNextAvailableRule()
    {
        int totalRemaining = 0;
        foreach (var rule in spawnRules)
        {
            int remaining = rule.totalToSpawn - rule.spawnedCount;
            if (remaining > 0) totalRemaining += remaining;
        }

        if (totalRemaining <= 0) return null;

        int randomPoint = Random.Range(0, totalRemaining);

        foreach (var rule in spawnRules)
        {
            int remaining = rule.totalToSpawn - rule.spawnedCount;
            if (remaining > 0)
            {
                if (randomPoint < remaining)
                {
                    return rule;
                }
                randomPoint -= remaining;
            }
        }
        return null;
    }

    public void ResetRuntimeData()
    {
        foreach (var rule in spawnRules) rule.spawnedCount = 0;
    }
}
