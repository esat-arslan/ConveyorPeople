using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonPool : MonoBehaviour
{
    [SerializeField] private Person personPrefab;
    [SerializeField] private int initialSize = 10;

    private readonly Queue<Person> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreatePerson();
        }
    }

    private Person CreatePerson()
    {
        Person person = Instantiate(personPrefab,transform);
        person.gameObject.SetActive(false);
        pool.Enqueue(person);
        return person;
    }

    public Person Get()
    {
        if (pool.Count == 0) CreatePerson();

        Person person = pool.Dequeue();
        person.gameObject.SetActive(true);
        person.ResetState();
        Debug.Log($"GET PERSON ID: {person.GetInstanceID()}");

        return person;
    }

    public void Return(Person person)
    {
        person.ResetState();
        person.gameObject.SetActive(false);
        Debug.Log($"RETURN PERSON ID: {person.GetInstanceID()}");

        pool.Enqueue(person);
    }
}
