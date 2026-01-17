using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PersonSpawner))]
public class PersonSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PersonSpawner spawner = (PersonSpawner)target;

        GUILayout.Space(10);
        
        if (GUILayout.Button("Spawn Debug Person", GUILayout.Height(30)))
        {
            spawner.SpawnPerson();
        }
    }
}