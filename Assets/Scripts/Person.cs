using System;
using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private ConveyorPath conveyorPath;
    private PersonPool pool;
    
    // Logic Variables
    private int currentWaypointIndex;
    private int maxAllowedWaypointIndex;
    private PersonStates personState;

    private WaitingSlot assignedWaitingSlot;
    private ConveyorQueueSlot assignedQueueSlot;

    public ConveyorQueueSlot CurrentQueueSlot { get; private set; }
    public int AssignedQueueIndex { get; private set; } = -1;

    public event Action<Person> OnEndOfThePath;

    private Coroutine movementCoroutine;

    public void Initialize(ConveyorPath path, PersonPool personPool)
    {
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
    }

    public void AssignWaitingSlot(WaitingSlot slot)
    {
        assignedWaitingSlot = slot;
        personState = PersonStates.Waiting;
        
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToWaitingSlot());
    }

    public void StartConveyorMovement(int targetQueueIndex)
    {
        maxAllowedWaypointIndex = targetQueueIndex;

        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(MasterMovementRoutine());
    }

    private IEnumerator MasterMovementRoutine()
    {
        yield return StartCoroutine(FollowPathRoutine());

        Debug.Log("Reached End of assigned path.");
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
            
            if(currentWaypointIndex == maxAllowedWaypointIndex)
            {
                break;
            }

            currentWaypointIndex++;
        }
    }

    public void AdvanceToNextQueueSlot(ConveyorQueueSlot nextSlot)
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(MoveDirectlyToSlot(nextSlot));
    }

    private IEnumerator MoveDirectlyToSlot(ConveyorQueueSlot slot)
    {
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

        OnEndOfThePath?.Invoke(this);
    }

    private IEnumerator MoveToWaitingSlot()
    {
        if (assignedWaitingSlot == null) yield break;

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
    }

    // Cleanup
    public void PickUp()
    {
        if(assignedWaitingSlot != null) assignedWaitingSlot.Clear();
        if(CurrentQueueSlot != null) CurrentQueueSlot.Clear();
        pool.Return(this);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        currentWaypointIndex = 0;
        maxAllowedWaypointIndex = 0;
        AssignedQueueIndex = -1;

        assignedQueueSlot = null;
        assignedWaitingSlot = null;

        personState = PersonStates.OnConveyor;
    }
}

public enum PersonStates { OnConveyor, Waiting }