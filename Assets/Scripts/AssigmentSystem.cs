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
    [SerializeField] private PickUpZoneSlot pickUpZoneSlot;
    [SerializeField] private List<Transform> pickupZones;
    private List<PickUpZoneSlot> pickUpZoneSlots = new();
    private IReadOnlyList<Car> ActiveCars => CarManager.Instance.ActiveCars;
    private readonly List<Person> activePeople = new();
    private readonly List<Person> waitingPeople = new();


    private void Awake()
    {
        foreach (Transform transform in conveyorPath.Waypoints)
        {
            CreateQueueSlot(transform);
        }

        foreach (WaitingSlot slot in waitingSlots)
        {
            
        }

        foreach(Transform pos in pickupZones)
        {
            CreatePickUpZoneSlot(pos);
        }
    }

    private void OnEnable()
    {
        Car.OnCarActivated += HandleCarActivated;
    }
    private void OnDisable()
    {
        Car.OnCarActivated -= HandleCarActivated;
    }

    private void HandleCarActivated(Car car)
    {
        TryMoveCarToPickUpZone(car);
        TryAssignWaitingPeople();
    }

    private void TryMoveCarToPickUpZone(Car car)
    {
        foreach(PickUpZoneSlot pickUp in pickUpZoneSlots)
        {
            if (!pickUp.IsOccupied)
            {
                car.transform.position = pickUp.Position;
                pickUp.AssignToCar(car);
                break;
            }
        }
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

    private bool isQueueEmpty()
    {
        ConveyorQueueSlot queueSlot = conveyorQueueSlots[conveyorQueueSlots.Count - 1];
        if (queueSlot.IsOccupied) return false;
        return true;
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

        if (firstSlot.IsOccupied || !firstSlot.IsAvaible) return false;
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
        activePeople.Remove(person);
    }

    private void HandlePersonReachedEnd(Person person)
    {
        int lastSlotIndex = conveyorQueueSlots.Count - 1;

        if (person.AssignedQueueIndex == lastSlotIndex)
        {
            TryMoveEndQueueToWaiting();
        }
        else
        {
            TryAdvancePerson(person);
        }
    }

    private void TryMoveEndQueueToWaiting()
    {
        int lastSlotIndex = conveyorQueueSlots.Count - 1;
        if (lastSlotIndex < 0) return;

        ConveyorQueueSlot lastSlot = conveyorQueueSlots[lastSlotIndex];
        if (!lastSlot.IsOccupied) return;

        Person person = lastSlot.Occupant;
        if (person.IsOnConveyor) return;

        WaitingSlot slot = GetNextFreeSlot();
        if (slot == null) return;

        person.CurrentQueueSlot.Clear();
        slot.Assign(person);
        person.AssignWaitingSlot(slot);
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

    private WaitingSlot GetNextFreeSlot()
    {
        foreach (WaitingSlot slot in waitingSlots)
        {
            if (!slot.IsOccupied) return slot;
        }

        return null;
    }

    private void AdvanceQueue()
    {
        for (int i = conveyorQueueSlots.Count - 2; i >= 0; i--)
        {
            ConveyorQueueSlot currentSlot = conveyorQueueSlots[i];
            ConveyorQueueSlot nextSlot = conveyorQueueSlots[i + 1];

            if (!currentSlot.IsOccupied || !nextSlot.IsAvaible) continue;

            Person personToMove = currentSlot.Occupant;
            if (personToMove.IsOnConveyor) continue;
            nextSlot.Reserve();
            currentSlot.Clear();
            personToMove.AssignQueueSlot(nextSlot);

        }
    }

    public void TryAdvancePerson(Person person)
    {
        int currentIndex = person.AssignedQueueIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= conveyorQueueSlots.Count)
            return;

        ConveyorQueueSlot nextSlot = conveyorQueueSlots[nextIndex];

        if (!nextSlot.IsAvaible)
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
            freeSlot.Reserve();
            person.StartMovementToCar(freeSlot, ()=>
            {
                car.AddPersonToCar(person);
            });
            TryMoveEndQueueToWaiting();
        }
    }

    private void HandlePersonEnteredWaiting(Person person)
    {
        if (!waitingPeople.Contains(person)) waitingPeople.Add(person);

        TryAssignPersonToCar(person);
    }
}