using System;
using UnityEngine;

public class WaitingSlot : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public Person Occupant { get; private set; }

    public Vector3 Position => transform.position;


    public void Assign(Person person)
    {
        IsOccupied = true;
        Occupant = person;
    }

    public void Clear()
    {
        IsOccupied = false;
        Occupant = null;
    }
}
