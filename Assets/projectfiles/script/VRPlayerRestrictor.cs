using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VRPlayerRestrictor : MonoBehaviour
{
    public OVRCameraRig cameraRig;       // Assign your OVRCameraRig here
    public bool restrictMovement = false;

    [Header("Boundary Limits")]
    public Vector3 minBoundary;
    public Vector3 maxBoundary;

    void Update()
    {
        if (cameraRig == null || cameraRig.trackingSpace == null) return;

        if (restrictMovement)
        {
            // Option 1: Freeze movement entirely
            // Do nothing or reset to a stored position
        }
        else
        {
            // Clamp trackingSpace inside boundaries
            Vector3 clampedPos = cameraRig.trackingSpace.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, minBoundary.x, maxBoundary.x);
            clampedPos.y = Mathf.Clamp(clampedPos.y, minBoundary.y, maxBoundary.y);
            clampedPos.z = Mathf.Clamp(clampedPos.z, minBoundary.z, maxBoundary.z);

            cameraRig.trackingSpace.position = clampedPos;
        }
    }

    public void SetRestriction(bool state)
    {
        restrictMovement = state;
    }

    void OnDrawGizmos()
    {
        if (cameraRig == null || cameraRig.trackingSpace == null) return;

        // Draw boundary
        Vector3 center = (minBoundary + maxBoundary) / 2f;
        Vector3 size = maxBoundary - minBoundary;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);

        // Draw XR Rig current position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cameraRig.trackingSpace.position, 0.2f);
    }
}
