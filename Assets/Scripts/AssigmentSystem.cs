using System;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentSystem : MonoBehaviour
{
    [SerializeField] private List<WaitingSlot> waitingSlots = new();
    private List<ConveyorQueueSlot> conveyorQueueSlots = new();
    [SerializeField] private ConveyorQueueSlot conveyorQueueSlot;
    [SerializeField] private ConveyorPath conveyorPath;
    [SerializeField] private Transform queueSlotsParent;
    private readonly List<Person> activePeople = new();


    private void Awake()
    {
        foreach (Transform transform in conveyorPath.Waypoints)
        {
            CreateQueueSlot(transform);
        }
    }

    private void CreateQueueSlot(Transform queuePos)
    {
        ConveyorQueueSlot queueSlot = Instantiate
                                        (conveyorQueueSlot,
                                        queuePos.position,
                                        Quaternion.identity,
                                        queueSlotsParent);
        conveyorQueueSlots.Add(queueSlot);
    }

    public void RegisterPerson(Person person)
    {
        if (person is null)
            return;

        activePeople.Add(person);
        person.OnEndOfThePath += HandlePersonReachedEnd;
    }

    public void UnregisterPerson(Person person)
    {
        if (person is null)
            return;

        person.OnEndOfThePath -= HandlePersonReachedEnd;
        activePeople.Remove(person);
    }

    private void HandlePersonReachedEnd(Person person)
    {
        WaitingSlot slot = GetNextFreeSlot();

        if (slot is null)
        {
            Debug.LogWarning("No free waiting slots!");
            return;
        }

        slot.Assign(person);
        person.AssignWaitingSlot(slot);
    }

    private WaitingSlot GetNextFreeSlot()
    {
        foreach (WaitingSlot slot in waitingSlots)
        {
            if (!slot.IsOccupied)
                return slot;
        }

        return null;
    }
}
