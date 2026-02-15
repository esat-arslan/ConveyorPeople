using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private ConveyorQueueSlot queueSlotPrefab;
    [SerializeField] private ConveyorPath path;
    [SerializeField] private Transform slotsParent;

    private readonly List<ConveyorQueueSlot> queueSlots = new();
    public IReadOnlyList<ConveyorQueueSlot> QueueSlots => queueSlots;

    public int SlotCount => queueSlots.Count;
    public Vector3 StartPosition => path != null ? path.StartPosition : Vector3.zero;
    public ConveyorPath Path => path;
    private bool isAdvancingQueue = false;

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (path == null) return;

        foreach (Transform waypoint in path.Waypoints)
        {
            ConveyorQueueSlot slot = Instantiate(queueSlotPrefab, waypoint.position, Quaternion.identity, slotsParent ?? transform);
            queueSlots.Add(slot);
            slot.SetQueueIndex(queueSlots.Count - 1);
        }
    }

    public ConveyorQueueSlot GetFirstSlot()
    {
        if (queueSlots.Count == 0) return null;
        return queueSlots[0];
    }

    public ConveyorQueueSlot GetLastSlot()
    {
        if (queueSlots.Count == 0) return null;
        return queueSlots[^1];
    }

    public void AdvanceQueue()
    {
        if (isAdvancingQueue) return;
        isAdvancingQueue = true;

        for (int i = queueSlots.Count - 2; i >= 0; i--)
        {
            ConveyorQueueSlot currentSlot = queueSlots[i];
            ConveyorQueueSlot nextSlot = queueSlots[i + 1];

            if (!currentSlot.IsOccupied || !nextSlot.IsAvailable) continue;

            Person personToMove = currentSlot.Occupant;
            if (personToMove.IsOnConveyor) continue;

            MovePersonToNextSlot(personToMove, currentSlot, nextSlot);
        }

        isAdvancingQueue = false;
    }

    public void TryAdvancePerson(Person person)
    {
        int currentIndex = person.AssignedQueueIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= queueSlots.Count) return;

        ConveyorQueueSlot nextSlot = queueSlots[nextIndex];
        if (!nextSlot.IsAvailable) return;

        ConveyorQueueSlot currentSlot = queueSlots[currentIndex];
        MovePersonToNextSlot(person, currentSlot, nextSlot);
        AdvanceQueue();
    }

    private void MovePersonToNextSlot(Person person, ConveyorQueueSlot current, ConveyorQueueSlot next)
    {
        next.Reserve();
        current.Clear();
        person.AssignQueueSlot(next);
    }
}
