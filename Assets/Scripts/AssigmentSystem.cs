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


    public bool TryRegisterPerson(Person person)
    {
        ConveyorQueueSlot queueSlot = GetNextQueueSlot(person);
        if (queueSlot == null) return false;
        if (person == null) return false;

        activePeople.Add(person);
        person.OnEndOfThePath += HandlePersonReachedEnd;

        queueSlot.AssignToQueue(person);
        person.AssignQueueSlot(queueSlot);
        return true;
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

        if (person.AssignedQueueIndex == lastSlotIndex)
        {
            WaitingSlot slot = GetNextFreeSlot();
            if (slot == null)
                return;

            person.CurrentQueueSlot.Clear();
            slot.Assign(person);
            person.AssignWaitingSlot(slot);
        }

        TryAdvancePerson(person);
    }


    private void AdvanceQueue()
    {
        for (int i = conveyorQueueSlots.Count - 2; i >= 0; i--)
        {
            ConveyorQueueSlot currentSlot = conveyorQueueSlots[i];
            ConveyorQueueSlot nextSlot = conveyorQueueSlots[i + 1];

            if (!currentSlot.IsOccupied || nextSlot.IsOccupied) continue;

            Person personToMove = currentSlot.Occupant;
            if (personToMove.IsOnConveyor) continue;
            currentSlot.Clear();
            nextSlot.AssignToQueue(personToMove);
            personToMove.AdvanceToNextQueueSlot(nextSlot);

        }
    }

    public void TryAdvancePerson(Person person)
    {
        while (true)
        {
            int currentIndex = person.AssignedQueueIndex;
            int nextIndex = currentIndex + 1;

            if (nextIndex >= conveyorQueueSlots.Count)
                return;

            ConveyorQueueSlot nextSlot = conveyorQueueSlots[nextIndex];

            if (nextSlot.IsOccupied)
                return;

            ConveyorQueueSlot currentSlot = conveyorQueueSlots[currentIndex];

            currentSlot.Clear();
            nextSlot.AssignToQueue(person);
            person.AssignQueueSlot(nextSlot);
            person.AdvanceToNextQueueSlot(nextSlot);

        }
    }


    private WaitingSlot GetNextFreeSlot()
    {
        Debug.Log("Looking for free slot");
        foreach (WaitingSlot slot in waitingSlots)
        {
            if (!slot.IsOccupied) return slot;
        }
        Debug.Log("No free waiting slots");
        return null;
    }

    private ConveyorQueueSlot GetNextQueueSlot(Person person)
    {
        for (int i = conveyorQueueSlots.Count - 1; i >= 0; i--)
        {
            bool currentEmpty = !conveyorQueueSlots[i].IsOccupied;
            bool prevEmpty = (i>0)&&!conveyorQueueSlots[i-1].IsOccupied;

            if(currentEmpty && prevEmpty) return conveyorQueueSlots[i];
        }

        return null;
    }
}
