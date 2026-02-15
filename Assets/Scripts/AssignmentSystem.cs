using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssignmentSystem : MonoBehaviour
{
    [SerializeField] private List<WaitingSlot> waitingSlots = new();
    private List<ConveyorQueueSlot> conveyorQueueSlots = new();
    [SerializeField] private ConveyorQueueSlot conveyorQueueSlot;
    [SerializeField] private ConveyorPath conveyorPath;
    [SerializeField] private Transform queueSlotsParent;
    [SerializeField] private PickUpZoneSlot pickUpZoneSlot;
    [SerializeField] private List<Transform> pickupZones;
    private List<PickUpZoneSlot> pickUpZoneSlots = new();
    private IReadOnlyList<Car> ActiveCars => CarManager.Instance.ActiveCars;
    private readonly List<Person> activePeople = new();
    private readonly List<Person> waitingPeople = new();
    private bool isAdvancingQueue = false;


    private void Awake()
    {
        foreach (Transform transform in conveyorPath.Waypoints)
        {
            CreateQueueSlot(transform);
        }

        foreach (WaitingSlot slot in waitingSlots)
        {
            slot.OnSlotFreed += HandleWaitingSlotFreed;
        }

        foreach (Transform pos in pickupZones)
        {
            CreatePickUpZoneSlot(pos);
        }
    }

    private void OnEnable()
    {
        Car.OnCarActivated += HandleCarActivated;
        Car.OnCarExitStarted += HandleCarLeavingPickup;
    }
    private void OnDisable()
    {
        Car.OnCarActivated -= HandleCarActivated;
        Car.OnCarExitStarted -= HandleCarLeavingPickup;
        foreach (WaitingSlot slot in waitingSlots)
        {
            slot.OnSlotFreed -= HandleWaitingSlotFreed;
        }
    }

    private void HandleCarLeavingPickup(Car car)
    {
        car.CurrentPickupSlot?.Clear();
    }

    private void HandleCarActivated(Car car)
    {
        if (TryMoveCarToPickUpZone(car))
        {
            TryAssignWaitingPeople();
        }
        else
        {
            car.SetActive(false);
        }

    }

    private bool TryMoveCarToPickUpZone(Car car)
    {
        foreach (PickUpZoneSlot pickUp in pickUpZoneSlots)
        {
            if (!pickUp.IsOccupied)
            {
                car.transform.position = pickUp.Position;
                pickUp.AssignToCar(car);
                car.SetPickupSlot(pickUp);
                return true;
            }
        }
        return false;
    }

    private void TryAssignWaitingPeople()
    {
        for (int i = waitingPeople.Count - 1; i >= 0; i--)
        {
            Person person = waitingPeople[i];
            if (person == null) continue;
            Debug.Log("Assiging waiting people");
            TryAssignPersonToCar(person);
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

    private void CreatePickUpZoneSlot(Transform pickUpZone)
    {
        PickUpZoneSlot pickUp = Instantiate(
            pickUpZoneSlot,
            pickUpZone.position,
            Quaternion.identity
        );
        pickUpZoneSlots.Add(pickUp);
    }


    public bool TryRegisterPerson(Person person)
    {
        if (conveyorQueueSlots.Count == 0) return false;

        ConveyorQueueSlot firstSlot = conveyorQueueSlots[0];

        if (firstSlot.IsOccupied || !firstSlot.IsAvailable) return false;
        if (person == null) return false;

        activePeople.Add(person);
        person.OnEndOfThePath += HandlePersonReachedEnd;
        person.OnEnteredWaiting += HandlePersonEnteredWaiting;

        firstSlot.AssignToQueue(person);
        person.AssignQueueSlot(firstSlot);
        return true;
    }

    public void UnregisterPerson(Person person)
    {
        if (person is null) return;

        person.OnEndOfThePath -= HandlePersonReachedEnd;
        person.OnEnteredWaiting -= HandlePersonEnteredWaiting;
        activePeople.Remove(person);
    }

    private void HandlePersonReachedEnd(Person person)
    {
        int lastSlotIndex = conveyorQueueSlots.Count - 1;

        if (person.AssignedQueueIndex == lastSlotIndex)
        {
            TryMoveEndQueueToWaiting(person);
        }
        else
        {
            TryAdvancePerson(person);
        }
    }

    private void TryMoveEndQueueToWaiting(Person person)
    {
        if (person == null) return;
        if (person.IsOnConveyor) return;
        if (person.AssignedWaitingSlot != null) return;

        if (person.AssignedQueueIndex != conveyorQueueSlots.Count - 1) return;

        WaitingSlot slot = GetAndReserveNextFreeSlot();
        if (slot == null) return;

        ConveyorQueueSlot prevSlot = person.CurrentQueueSlot;

        slot.Assign(person);
        person.AssignWaitingSlot(slot);

        prevSlot.Clear();

        AdvanceQueue();
    }

    public bool TryAssignPersonToCar(Person person)
    {
        if (CarManager.Instance == null)
        {
            Debug.Log("CarManager.Instance is null");
            return false;
        }
        CarManager.Instance.ForDebugging();

        foreach (Car car in ActiveCars)
        {
            Debug.Log("Trying to assign");
            if (!car.CanAccept(person)) continue;
            if (car.CarType == person.PersonType)
            {
                AssignPersonToCar(person, car);
                return true;
            }
        }
        return false;
    }

    private WaitingSlot GetAndReserveNextFreeSlot()
    {
        foreach (WaitingSlot slot in waitingSlots)
        {
            if (slot.IsAvailable)
            {
                slot.Reserve();
                return slot;
            }
        }

        return null;
    }

    private void AdvanceQueue()
    {
        if (isAdvancingQueue) return;
        isAdvancingQueue = true;
        for (int i = conveyorQueueSlots.Count - 2; i >= 0; i--)
        {
            ConveyorQueueSlot currentSlot = conveyorQueueSlots[i];
            ConveyorQueueSlot nextSlot = conveyorQueueSlots[i + 1];

            if (!currentSlot.IsOccupied || !nextSlot.IsAvailable) continue;

            Person personToMove = currentSlot.Occupant;
            if (personToMove.IsOnConveyor) continue;
            nextSlot.Reserve();
            currentSlot.Clear();
            personToMove.AssignQueueSlot(nextSlot);

        }
        isAdvancingQueue = false;
    }

    public void TryAdvancePerson(Person person)
    {
        int currentIndex = person.AssignedQueueIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= conveyorQueueSlots.Count)
            return;

        ConveyorQueueSlot nextSlot = conveyorQueueSlots[nextIndex];

        if (!nextSlot.IsAvailable)
            return;

        ConveyorQueueSlot currentSlot = conveyorQueueSlots[currentIndex];
        nextSlot.Reserve();
        currentSlot.Clear();
        person.AssignQueueSlot(nextSlot);
        AdvanceQueue();
    }

    public void AssignPersonToCar(Person person, Car car)
    {
        if (car.CanAccept(person))
        {
            if (waitingPeople.Contains(person)) waitingPeople.Remove(person);

            person.LeaveWaitingSlot();
            CarPersonSlot freeSlot = car.GetFreeSeat();
            if (freeSlot is null) return;
            freeSlot.Reserve();
            person.StartMovementToCar(freeSlot, () =>
            {
                car.AddPersonToCar(person);
            });
        }
    }

    private void HandlePersonEnteredWaiting(Person person)
    {
        if (!waitingPeople.Contains(person)) waitingPeople.Add(person);

        TryAssignPersonToCar(person);
    }

    private void HandleWaitingSlotFreed(WaitingSlot slot)
    {
        TryMoveLastQueuePersonToWaiting();
    }

    private void TryMoveLastQueuePersonToWaiting()
    {
        int lastIndex = conveyorQueueSlots.Count - 1;
        ConveyorQueueSlot lastSlot = conveyorQueueSlots[lastIndex];

        if (!lastSlot.IsOccupied) return;

        Person person = lastSlot.Occupant;
        TryMoveEndQueueToWaiting(person);
    }
}