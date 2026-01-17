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
        person.OnStartOfQueue += HandlePersonSpawned;
        Debug.Log("RegisterPerson called");
    }

    public void UnregisterPerson(Person person)
    {
        if (person is null)
            return;

        person.OnEndOfThePath -= HandlePersonReachedEnd;
        person.OnStartOfQueue -= HandlePersonSpawned;
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

    private void HandlePersonSpawned(Person person)
    {
        Debug.Log("HandlePersonSpawned called");
        ConveyorQueueSlot queueSlot = GetNextQueueSlot();
        if (queueSlot is null)
        {
            Debug.Log("No free queue slots!");
            return;
        }

        queueSlot.AssignToQueue(person);
        person.AssignQueueSlot(queueSlot);
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

    private ConveyorQueueSlot GetNextQueueSlot()
    {
        for (int i = conveyorQueueSlots.Count - 1; i >= 0; i--)
        {
            if (!conveyorQueueSlots[i].IsOccupied)
            {
                return conveyorQueueSlots[i];
            }
        }
        return null;
    }
}
