using System.Collections.Generic;
using UnityEngine;

public class ConveyorPath : MonoBehaviour
{
    [SerializeField]
    private List<Transform> waypoints = new List<Transform>();
    public int WaypointCount => waypoints.Count;
    public List<Transform> Waypoints => waypoints;
    [SerializeField]private Vector3 startPosition;
    public Vector3 StartPosition => startPosition;

    public Vector3 GetWaypointPos(int index)
    {
        if (index < 0 || index >= waypoints.Count)
        {
            Debug.LogError($"Waypoint index {index} is out of range");
            return Vector3.zero;
        }

        return waypoints[index].position;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null)
                continue;

            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawSphere(waypoints[i].position, 0.1f);
        }

        Gizmos.DrawSphere(waypoints[^1].position, 0.1f);
    }
#endif
}
