using UnityEngine;

public class OVRHandTrackingLadderClimbSticky : MonoBehaviour
{
    [Header("Player Rig")]
    public Transform playerRig; // OVRCameraRig or XROrigin root
    public Transform leftHand;
    public Transform rightHand;

    [Header("OVR Hands")]
    public OVRHand leftOVRHand;
    public OVRHand rightOVRHand;

    [Header("Ladder Settings")]
    public LayerMask ladderMask;
    [Range(0.01f, 0.5f)] public float grabRadius = 0.1f;
    [Range(0f, 1f)] public float pinchThreshold = 0.6f;
    public float climbMultiplier = 1.0f;
    public float smoothing = 10f;
    public float stickStrength = 15f;

    [Header("Optional Target Transforms")]
    public Transform leftTarget;   // You can assign a target on the ladder
    public Transform rightTarget;  // or leave null for auto-grab point

    private bool leftGrabbing, rightGrabbing;
    private Vector3 leftGrabPoint, rightGrabPoint;
    private Vector3 lastLeftPos, lastRightPos;

    void Update()
    {
        HandleHand(leftOVRHand, leftHand, leftTarget, ref leftGrabbing, ref lastLeftPos, ref leftGrabPoint);
        HandleHand(rightOVRHand, rightHand, rightTarget, ref rightGrabbing, ref lastRightPos, ref rightGrabPoint);
    }

    void HandleHand(OVRHand ovrHand, Transform handTransform, Transform target, ref bool grabbing, ref Vector3 lastPos, ref Vector3 grabPoint)
    {
        if (ovrHand == null || handTransform == null) return;

        float pinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool isPinching = pinchStrength > pinchThreshold;
        bool nearLadder = Physics.CheckSphere(handTransform.position, grabRadius, ladderMask);

        if (isPinching && nearLadder)
        {
            if (!grabbing)
            {
                grabbing = true;
                grabPoint = target ? target.position : handTransform.position;
                lastPos = handTransform.position;
            }
            else
            {
                // Move the player rig opposite to hand movement for climbing
                Vector3 delta = lastPos - handTransform.position;
                Vector3 targetMove = delta * climbMultiplier;
                playerRig.position = Vector3.Lerp(playerRig.position, playerRig.position + targetMove, Time.deltaTime * smoothing);

                // Keep visual hand locked to the grab point
                handTransform.position = Vector3.Lerp(handTransform.position, grabPoint, Time.deltaTime * stickStrength);

                lastPos = handTransform.position;
            }
        }
        else
        {
            grabbing = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (leftHand)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(leftHand.position, grabRadius);
        }
        if (rightHand)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rightHand.position, grabRadius);
        }

        if (leftTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(leftTarget.position, 0.02f);
        }
        if (rightTarget)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(rightTarget.position, 0.02f);
        }
    }
}
