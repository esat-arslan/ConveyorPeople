using System.Collections.Generic;
using UnityEngine;

public class PersonSpawner : MonoBehaviour
{
    [SerializeField] private ConveyorPath conveyorPath;
    [SerializeField] private Person personPrefab;
    [SerializeField] private AssignmentSystem assigmentSystem;
    [SerializeField] private List<Color> colorPool = new List<Color>();
    [SerializeField] private PersonPool pool;

    public void SpawnPerson()
    {
        Person person = pool.Get();
        person.transform.position = conveyorPath.StartPosition;

        int randomIndex = Random.Range(0, colorPool.Count);
        Color randomColor = colorPool[randomIndex];
        person.gameObject.GetComponent<SpriteRenderer>().color = randomColor;

        person.Initialize(conveyorPath, pool);
        assigmentSystem.RegisterPerson(person);
    }


}
