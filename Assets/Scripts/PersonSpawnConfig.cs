using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonSpawnConfig", menuName = "Spawning/PersonSpawnConfig")]
public class PersonSpawnConfig : ScriptableObject
{
    [System.Serializable]
    public class ColorSpawnRule
    {
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
        foreach (var rule in colorRules)
        {
            if (rule.spawnedCount < rule.totalToSpawn) return rule;
        }
        return null;
    }

    public void ResetRuntimeData()
    {
        foreach (var rule in colorRules) rule.spawnedCount = 0;
    }
}
