using UnityEngine;

public class ConveyorQueueSlot : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public Person Occupant { get; private set; }
    public int QueueIndex { get; private set; }

    public Vector3 Position => transform.position;

    public void AssignToQueue(Person person)
    {
        IsOccupied = true;
        Occupant = person;
    }

    public void Clear()
    {
        IsOccupied = false;
        Occupant = null;
    }

    public void SetQueueIndex(int index) => QueueIndex = index;
}
