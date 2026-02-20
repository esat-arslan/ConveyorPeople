using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarSpawnConfig", menuName = "Spawning/CarSpawnConfig")]
public class CarSpawnConfig : ScriptableObject
{
    [System.Serializable]
    public class CarSpawnRule
    {
        public CarType carType;
        public int totalToSpawn;
        [HideInInspector] public int spawnedCount;
    }

    public List<CarSpawnRule> carRules = new();

    public bool HasRemaning()
    {
        foreach(var rule in carRules)
        {
            if (rule.spawnedCount < rule.totalToSpawn) return true;
        }

        return false;
    }

    public CarType? GetNextType()
    {
        int totalRemaining = 0;

        foreach(var rule in carRules)
        {
            int remaining = rule.totalToSpawn - rule.spawnedCount;
            if (remaining > 0) totalRemaining += remaining;
        }

        if (totalRemaining <= 0) return null;

        int randomPoint = Random.Range(0, totalRemaining);

        foreach(var rule in carRules)
        {
            int remaining = rule.totalToSpawn - rule.spawnedCount;
            if (remaining > 0)
            {
                if (randomPoint < remaining)
                {
                    rule.spawnedCount++;
                    return rule.carType;
                }
                randomPoint -= remaining;
            }
        }

        return null;
    }

    public void ResetRuntimeData()
    {
        foreach (var rule in carRules)
            rule.spawnedCount = 0;
    }
}
