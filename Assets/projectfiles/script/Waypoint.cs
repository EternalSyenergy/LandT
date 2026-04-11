using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Transform nextWaypoint;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (nextWaypoint != null)
        {
            Gizmos.DrawLine(transform.position, nextWaypoint.position);
        }
    }
}
