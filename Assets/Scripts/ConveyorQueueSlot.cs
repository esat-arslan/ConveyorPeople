using System;
using UnityEngine;

public class ConveyorQueueSlot : MonoBehaviour
{
    public Person Occupant { get; private set; }
    public int QueueIndex { get; private set; }

    public Vector3 Position => transform.position;
    public bool IsReserved {get;private set;}
    public bool IsOccupied => Occupant != null;
    public bool IsAvaible => !IsOccupied && !IsReserved;

    public void AssignToQueue(Person person)
    {
        IsReserved = false;
        Occupant = person;
    }

    public void Clear()
    {
        Occupant = null;
        IsReserved = false;
    }

    public void Reserve()
    {
        IsReserved = true;
    }

    public void SetQueueIndex(int index) => QueueIndex = index;
}
