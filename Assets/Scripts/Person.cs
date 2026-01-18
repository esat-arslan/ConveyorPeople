using System;
using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private ConveyorPath conveyorPath;
    
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

    public void Initialize(ConveyorPath path)
    {
        conveyorPath = path;
        currentWaypointIndex = 0;
        personState = PersonStates.OnConveyor;
    }

    public void AssignQueueSlot(ConveyorQueueSlot queueSlot)
    {
        Debug.Log($"AssignQueueSlot called: Index {queueSlot.QueueIndex}");
        assignedQueueSlot = queueSlot;
        CurrentQueueSlot = queueSlot;
        AssignedQueueIndex = queueSlot.QueueIndex; 
        maxAllowedWaypointIndex = queueSlot.QueueIndex;

        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MasterMovementRoutine());
    }

    public void AssignWaitingSlot(WaitingSlot slot)
    {
        assignedWaitingSlot = slot;
        personState = PersonStates.Waiting;
        
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToWaitingSlot());
    }

    private IEnumerator MasterMovementRoutine()
    {
        yield return StartCoroutine(FollowPathRoutine());

        if (CurrentQueueSlot != null)
        {
            transform.position = CurrentQueueSlot.Position; 
        }

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
        Destroy(gameObject);
    }
}

public enum PersonStates { OnConveyor, Waiting }