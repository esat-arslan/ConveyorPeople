using System;
using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private float speed = 5f; //Will move it to Conveyor
    private ConveyorPath conveyorPath;
    private int currentWaypointIndex;
    private Vector3 currentWaypoint;
    private PersonStates personState;
    private WaitingSlot assignedSlot;
    private ConveyorQueueSlot assignedQueueSlot;

    public event Action<Person> OnEndOfThePath;
    public event Action<Person> OnStartOfQueue;

    public void Initialize(ConveyorPath path)
    {
        conveyorPath = path;
        currentWaypointIndex = 0;
        personState = PersonStates.OnConveyor;
        Debug.Log("Initialize called!");
        //StartCoroutine(FollowPathRoutine());

    }

    public void EnterQueue()
    {
        OnStartOfQueue?.Invoke(this);
    }
    private IEnumerator FollowPathRoutine()
    {
        if (conveyorPath is null) yield break;

        while (currentWaypointIndex < conveyorPath.WaypointCount)
        {
            currentWaypoint = conveyorPath.GetWaypointPos(currentWaypointIndex);

            while ((transform.position - currentWaypoint).sqrMagnitude > 0.001f)
            {
                transform.position = Vector3.MoveTowards
                                            (transform.position, currentWaypoint, speed * Time.deltaTime);
                yield return null;
            }

            currentWaypointIndex++;
        }

        OnEndOfThePath?.Invoke(this);
    }


    private IEnumerator MoveToWaitingSlot()
    {
        Vector3 target = assignedSlot.Position;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
               transform.position,
               target,
               speed * Time.deltaTime
           );
            yield return null;
        }
    }

    private IEnumerator MoveToQueueSlot()
    {
        Vector3 target = assignedQueueSlot.Position;
        Debug.Log($"target is {target}");
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }
    }

    public void AssignWaitingSlot(WaitingSlot slot)
    {
        assignedSlot = slot;
        personState = PersonStates.Waiting;
        Debug.Log($"Assigned slot is {assignedSlot.gameObject.name}");
        StartCoroutine(MoveToWaitingSlot());
    }

    public void AssignQueueSlot(ConveyorQueueSlot queueSlot)
    {
        Debug.Log("AssignQueueSlot called");
        assignedQueueSlot = queueSlot;
        personState = PersonStates.OnConveyor;
        Debug.Log($"Queue slot is {assignedQueueSlot.gameObject.name}");
        StartCoroutine(MoveToQueueSlot());
    }

    public void PickUp()
    {
        assignedSlot.Clear();
        Destroy(gameObject);
    }
}

public enum PersonStates { OnConveyor, Waiting }
