using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonSpawnConfig", menuName = "Spawning/PersonSpawnConfig")]
public class PersonSpawnConfig : ScriptableObject
{
    [System.Serializable]
    public class ColorSpawnRule
    {
        public CarType carType;
        public Color color;
        public int totalToSpawn;
        [HideInInspector] public int spawnedCount;
    }

    public List<ColorSpawnRule> colorRules = new();
    
    public bool SpawnRemaningAvaible()
    {
        foreach (var rule in colorRules)
        {
            if (rule.spawnedCount < rule.totalToSpawn) return true;
        }
        return false;
    }

    public ColorSpawnRule GetNextAvaibleRule()
    {
        int totalRemaining = 0;
        foreach (var rule in colorRules)
        {
            int remaining = rule.totalToSpawn - rule.spawnedCount;
            if (remaining > 0) totalRemaining += remaining;
        }

        if (totalRemaining <= 0) return null;

        int randomPoint = Random.Range(0, totalRemaining);

        foreach (var rule in colorRules)
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
        foreach (var rule in colorRules) rule.spawnedCount = 0;
    }
}
