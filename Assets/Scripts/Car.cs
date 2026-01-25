using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private Car_SO carProperties;
    private CarType CarType => carProperties.type;
    private Color Color => carProperties.color;
    private int Size => carProperties.size;
    private bool isActive;
    private List<Person> people = new();
    private List<Person> People => people;

    private void Awake()
    {
        if (isActive) CarManager.Instance.Register(this);
    }

    private void Start()
    {
        Debug.Log("Car activated");
    }

    public void SetActive(bool value)
    {
        if (isActive == value) return;

        isActive = value;

        if (isActive) CarManager.Instance.Register(this);
        else CarManager.Instance.Unregister(this);
    }

    public bool CanAccept(Person person)
    {
        return people.Count < Size;
    }

    public void AddPersonToCar(Person person)
    {
        people.Add(person);
    }
}

