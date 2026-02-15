using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private Car_SO carProperties;
    [SerializeField] private Transform exitTarget;
    [SerializeField] private float moveSpeed = 3f;
    public CarType CarType => carProperties.type;
    private Color Color => carProperties.color;
    private int Size => carProperties.size;
    private bool isActive;
    public bool IsActive => isActive;
    private List<Person> people = new();
    private List<Person> People => people;
    private List<CarPersonSlot> seatSlots = new();
    private Coroutine moveRoutine;

    public static event Action<Car> OnCarActivated;
    public static event Action<Car> OnCarDeactivated;
    public static event Action<Car> OnCarFull;

    private void Awake()
    {
        if (isActive) CarManager.Instance.Register(this);
        seatSlots.AddRange(GetComponentsInChildren<CarPersonSlot>());
    }

    private void OnEnable()
    {
        OnCarFull += HandleCarFull;
    }

    private void OnDisable()
    {
        OnCarFull -= HandleCarFull;
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
        foreach (var seat in seatSlots)
        {
            if (!seat.IsOccupied) return true;
        }
        return false;
    }

    public CarPersonSlot GetFreeSeat()
    {
        foreach (var seat in seatSlots)
        {
            if (seat.IsAvailable) return seat;
        }
        return null;
    }

    public void AddPersonToCar(Person person)
    {
        //person.StopMovement();
        people.Add(person);
        if (IsFull())
        {
            OnCarFull?.Invoke(this);
        }
    }

    private bool IsFull()
    {
        if (people.Count == seatSlots.Count)
        {
            return true;
        }
        return false;
    }

    private void HandleCarFull(Car car)
    {
        if (car != this) return;

        StartExitMovement();
    }

    private void StartExitMovement()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveToExitRoutine());
    }

    private IEnumerator MoveToExitRoutine()
    {
        Vector3 target = exitTarget.position;

        while((transform.position - target).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;

        OnReachedExit();
    }

    private void OnReachedExit()
    {
        SetActive(false);

        // TODO
        // implement return to pool when object pool gets implemented.
    }
}

