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

        foreach (WaitingSlot slot in waitingSlots)
        {
            slot.Clear();
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
        int index = conveyorQueueSlots.IndexOf(queueSlot);
        queueSlot.SetQueueIndex(index);
    }


    public void RegisterPerson(Person person)
    {
        if (person is null)
            return;

        activePeople.Add(person);
        person.OnEndOfThePath += HandlePersonReachedEnd;

        ConveyorQueueSlot queueSlot = GetNextQueueSlot(person);
        if (queueSlot != null)
        {
            queueSlot.AssignToQueue(person);
            person.AssignQueueSlot(queueSlot);
            person.StartConveyorMovement(queueSlot.QueueIndex);
        }
        Debug.Log("RegisterPerson called");
    }

    public void UnregisterPerson(Person person)
    {
        if (person is null) return;

        person.OnEndOfThePath -= HandlePersonReachedEnd;
        activePeople.Remove(person);
    }

    private void HandlePersonReachedEnd(Person person)
    {
        int lastSlotIndex = conveyorQueueSlots.Count - 1;
        if (person.AssignedQueueIndex != lastSlotIndex) return;
        WaitingSlot slot = GetNextFreeSlot();
        if (slot == null)
        {
            Debug.LogWarning($"No free waiting slots!");
            return;
        }

        person.CurrentQueueSlot.Clear();

        slot.Assign(person);
        person.AssignWaitingSlot(slot);

        AdvanceQueue();
    }

    private void AdvanceQueue()
    {
        for (int i = conveyorQueueSlots.Count - 2; i >= 0; i--)
        {
            ConveyorQueueSlot currentSlot = conveyorQueueSlots[i];
            ConveyorQueueSlot nextSlot = conveyorQueueSlots[i + 1];

            if (currentSlot.IsOccupied && !nextSlot.IsOccupied)
            {
                Person personToMove = currentSlot.Occupant;

                currentSlot.Clear();
                personToMove.AdvanceToNextQueueSlot(nextSlot);
                nextSlot.AssignToQueue(personToMove);
            }
        }
    }

    private void HandlePersonSpawned(Person person)
    {
        Debug.Log("HandlePersonSpawned called");
        ConveyorQueueSlot queueSlot = GetNextQueueSlot(person);
        if (queueSlot is null)
        {
            Debug.Log($"No free queue slots! current occupant");
            return;
        }

        queueSlot.AssignToQueue(person);
        person.AssignQueueSlot(queueSlot);
    }

    private WaitingSlot GetNextFreeSlot()
    {
        foreach (WaitingSlot slot in waitingSlots)
        {
            if (!slot.IsOccupied) return slot;
        }

        return null;
    }

    private ConveyorQueueSlot GetNextQueueSlot(Person person)
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

    public bool HasFreeQueueSlot()
    {
        foreach (var slot in conveyorQueueSlots)
        {
            if (!slot.IsOccupied) return true;
        }
        return false;
    }
}
