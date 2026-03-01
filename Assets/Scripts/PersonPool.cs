using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonPool : MonoBehaviour
{
    [SerializeField] private int initializeSizePerPrefab = 5;
    private readonly Dictionary<Person, Queue<Person>> pools = new();



    private Person CreatePerson(Person prefab)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<Person>();

        Person person = Instantiate(prefab,transform);
        person.SetOriginPrefab(prefab);
        person.gameObject.SetActive(false);

        pools[prefab].Enqueue(person);

        return person;
    }

    public Person Get(Person prefab)
    {
        if(!pools.ContainsKey(prefab)) pools[prefab] = new Queue<Person>();
        
        
        if(pools[prefab].Count == 0)
            CreatePerson(prefab);

        Person person = pools[prefab].Dequeue();
        person.gameObject.SetActive(true);
        person.ResetState();
        //Debug.Log($"GET PERSON ID: {person.GetInstanceID()}");

        return person;
    }

    public void Return(Person person)
    {
        person.ResetState();
        person.gameObject.SetActive(false);

 
        pools[person.OriginPrefab].Enqueue(person);
    }
}
