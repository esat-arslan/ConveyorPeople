using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private Car_SO carProperties;
    public CarType CarType => carProperties.type;
    private Color Color => carProperties.color;
    private int Size => carProperties.size;
    private bool isActive;
    public bool IsActive => isActive;
    private List<Person> people = new();
    private List<Person> People => people;

    public static event Action<Car> OnCarActivated;
    public static event Action<Car> OnCarDeactivated;

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

        if (isActive)
        {
            CarManager.Instance.Register(this);
            OnCarActivated?.Invoke(this);
        }
        else
        {
            CarManager.Instance.Unregister(this);
            OnCarDeactivated?.Invoke(this);
        }
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

