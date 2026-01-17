using UnityEngine;

public class PersonSpawner : MonoBehaviour
{
    [SerializeField] private ConveyorPath conveyorPath;
    [SerializeField] private Person personPrefab;
    [SerializeField] private AssignmentSystem assigmentSystem;

    public void SpawnPerson()
    {
        Person person = Instantiate(personPrefab, transform.position, Quaternion.identity);
        assigmentSystem.RegisterPerson(person);
        person.Initialize(conveyorPath);
        person.EnterQueue();
    }


}
