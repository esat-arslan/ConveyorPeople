using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] CarType personType;
    public CarType PersonType => personType;
    private ConveyorPath conveyorPath;
    private PersonPool pool;
    public bool IsOnConveyor { get; private set; }

    // Logic Variables
    private int currentWaypointIndex;
    private int maxAllowedWaypointIndex;
    bool hasStartedConveyorMovement;
    private PersonStates personState;

    private WaitingSlot assignedWaitingSlot;
    public WaitingSlot AssignedWaitingSlot => assignedWaitingSlot;
    private ConveyorQueueSlot assignedQueueSlot;

    public ConveyorQueueSlot CurrentQueueSlot { get; private set; }
    public int AssignedQueueIndex { get; private set; } = -1;

    public event Action<Person> OnEndOfThePath;
    public event Action<Person> OnEnteredWaiting;
    private Coroutine movementCoroutine;

    private ConveyorQueueSlot pendingQueueSlot;
    private bool isMoving;

    public void Initialize(ConveyorPath path, PersonPool personPool, CarType carType)
    {
        personType = carType;
        conveyorPath = path;
        pool = personPool;
        ResetState();
        currentWaypointIndex = 0;
        personState = PersonStates.OnConveyor;
    }

    public void AssignQueueSlot(ConveyorQueueSlot queueSlot)
    {
        assignedQueueSlot = queueSlot;
        CurrentQueueSlot = queueSlot;
        AssignedQueueIndex = queueSlot.QueueIndex;

        StartConveyorMovement(queueSlot.QueueIndex);
    }

    public void AssignWaitingSlot(WaitingSlot slot)
    {
        if (isMoving) return;
        StopMovement();
        isMoving = true;
        assignedWaitingSlot = slot;
        personState = PersonStates.Waiting;

        movementCoroutine = StartCoroutine(MoveToWaitingSlot());
    }

    public void StartConveyorMovement(int targetQueueIndex)
    {
        IsOnConveyor = true;
        StopMovement();

        maxAllowedWaypointIndex = targetQueueIndex;
        movementCoroutine = StartCoroutine(MasterMovementRoutine());
    }

    private IEnumerator MasterMovementRoutine()
    {
        yield return StartCoroutine(FollowPathRoutine());
        IsOnConveyor = false;
        if (CurrentQueueSlot != null) CurrentQueueSlot.AssignToQueue(this);
        OnEndOfThePath?.Invoke(this);
    }

    private IEnumerator FollowPathRoutine()
    {
        if (conveyorPath is null) yield break;

        while (currentWaypointIndex <= maxAllowedWaypointIndex)
        {
            Vector3 target = conveyorPath.GetWaypointPos(currentWaypointIndex);

            while ((transform.position - target).sqrMagnitude > 0.001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = target;

            if (currentWaypointIndex == maxAllowedWaypointIndex)
            {
                break;
            }

            currentWaypointIndex++;
        }
    }

    private void StartMoveToSlot(ConveyorQueueSlot slot)
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveDirectlyToSlot(slot));
    }


    private IEnumerator MoveDirectlyToSlot(ConveyorQueueSlot slot)
    {
        isMoving = true;

        Vector3 target = slot.Position;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;

        CurrentQueueSlot = slot;
        AssignedQueueIndex = slot.QueueIndex;
        slot.AssignToQueue(this);

        isMoving = false;

        if (pendingQueueSlot != null)
        {
            ConveyorQueueSlot next = pendingQueueSlot;
            pendingQueueSlot = null;
            StartMoveToSlot(next);
        }

    }

    private IEnumerator MoveToWaitingSlot()
    {
        if (assignedWaitingSlot == null)
        {
            isMoving = false;
            yield break;
        }

        Vector3 target = assignedWaitingSlot.Position;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
               transform.position,
               target,
               speed * Time.deltaTime
           );
            yield return null;
        }
        transform.position = target;
        isMoving = false;

        OnEnteredWaiting?.Invoke(this);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        hasStartedConveyorMovement = false;

        currentWaypointIndex = 0;
        maxAllowedWaypointIndex = 0;
        AssignedQueueIndex = -1;

        assignedQueueSlot = null;
        assignedWaitingSlot = null;
        CurrentQueueSlot = null;

    }

    public void StopMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    public void StartMovementToCar(CarPersonSlot seatSlot, Action onReached)
    {
        StopMovement();
        movementCoroutine = StartCoroutine(MoveToCarRoutine(seatSlot, onReached));
    }

    private IEnumerator MoveToCarRoutine(CarPersonSlot seatSlot, Action onReached)
    {
        Vector3 target = seatSlot.Position;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;

        seatSlot.AssignToSlot(this);
        yield return null;
        onReached?.Invoke();

    }

    public void LeaveWaitingSlot()
    {
        if (assignedWaitingSlot != null)
        {
            assignedWaitingSlot.Clear();
            assignedWaitingSlot = null;
        }
    }

    public void SetVisual(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}

public enum PersonStates { OnConveyor, Waiting }