using System;
using UnityEngine;

public class WaitingSlot : MonoBehaviour
{
    private bool isReserved;

    public Person Occupant { get; private set; }
    public bool IsOccupied => Occupant != null;

    public bool IsReserved => isReserved;
    public bool IsAvailable => !IsOccupied && !IsReserved;

    public Vector3 Position => transform.position;

    public event Action<WaitingSlot> OnSlotFreed;


    public void Assign(Person person)
    {
        Occupant = person;
        isReserved = false;
    }

    public void Reserve()
    {
        isReserved = true;
    }

    public void Clear()
    {
        Occupant = null;
        isReserved = false;

        OnSlotFreed?.Invoke(this);
    }
}
