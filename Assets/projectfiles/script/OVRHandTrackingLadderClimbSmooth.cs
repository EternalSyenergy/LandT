

using UnityEngine;

public class OVRHandTrackingLadderClimbSmooth : MonoBehaviour
{
    [Header("Player Rig")]
    public Transform playerRig;
    public Transform leftHand;
    public Transform rightHand;

    public Transform leftHandIK;
    public Transform rightHandIK;


    public Transform leftHandGrab;
    public Transform rightHandGrab;
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

    [Header("Optional Targets")]
    public Transform leftTarget;
    public Transform rightTarget;

    [Header("Horizontal Limits (X/Z)")]
    [Range(-5f, 5f)] public float minX = -1f;
    [Range(-5f, 5f)] public float maxX = 1f;
    [Range(-5f, 5f)] public float minZ = -1f;
    [Range(-5f, 5f)] public float maxZ = 1f;

    public Transform ladderCenter;

    public bool leftGrabbing, rightGrabbing;
    private Vector3 leftGrabPoint, rightGrabPoint;
    private bool lastActiveLeft = true;
    public static OVRHandTrackingLadderClimbSmooth instance;
    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
      

        //bool left = HandleHand(leftOVRHand, leftHand, leftTarget, ref leftGrabbing, ref leftGrabPoint, true, leftHandIK);
        //bool right = HandleHand(rightOVRHand, rightHand, rightTarget, ref rightGrabbing, ref rightGrabPoint, false, rightHandIK);


        bool left = HandleHand(leftOVRHand, leftHand, leftTarget, ref leftGrabbing, ref leftGrabPoint, true, leftHandIK);
        bool right = HandleHand(rightOVRHand, rightHand, rightTarget, ref rightGrabbing, ref rightGrabPoint, false, rightHandIK);


        // Determine active hand
        if (left && !right) lastActiveLeft = true;
        else if (right && !left) lastActiveLeft = false;
    }

  


    Vector3 smoothedLeft, smoothedRight;
    public float slowAmount = 6f; // Lower = heavier/slower feel



    bool HandleHandOld(
    OVRHand ovrHand,
    Transform hand,
    Transform target,
    ref bool grabbing,
    ref Vector3 grabPoint,
    bool isLeft,
    Transform ikTarget // the IK hand target (like leftHandIK),
    

)
    {
        if (ovrHand == null || hand == null) return false;

        // --- Adaptive smoothing setup ---
        ref Vector3 smoothedHand = ref (isLeft ? ref smoothedLeft : ref smoothedRight);

        if (smoothedHand == Vector3.zero)
            smoothedHand = hand.position;

        // Calculate hand velocity to adapt smoothing
        Vector3 rawHand = hand.position;
        Vector3 handVelocity = (rawHand - smoothedHand) / Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = handVelocity.magnitude;

        // Adaptive smoothing: less lag for fast hand movement
        float adaptiveSmooth = Mathf.Lerp(slowAmount, slowAmount * 6f, Mathf.Clamp01(speed / 1.5f));

        // Blend hand position
        smoothedHand = Vector3.Lerp(smoothedHand, rawHand, Time.deltaTime * adaptiveSmooth);
        // --- End smoothing ---

        float pinch = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool pinching = pinch > pinchThreshold;
        bool nearLadder = Physics.CheckSphere(smoothedHand, grabRadius, ladderMask);

   
    

        if (pinching && nearLadder)
        {
            if (!grabbing)
            {
                grabbing = true;
                grabPoint = target ? target.position : smoothedHand;

                // Mark latest grabbing hand
                lastActiveLeft = isLeft;

                // Lock the IK target at the grab point immediately
                if (ikTarget)
                    ikTarget.position = grabPoint;
            }

            Vector3 handDelta = grabPoint - smoothedHand; // Use smoothed movement
            Vector3 moveTarget = playerRig.position + handDelta * climbMultiplier;

            // Move the rig only for the most recently active hand
            if ((isLeft && lastActiveLeft) || (!isLeft && !lastActiveLeft))
            {
                if (ladderCenter)
                {
                    moveTarget.x = Mathf.Clamp(moveTarget.x, ladderCenter.position.x + minX, ladderCenter.position.x + maxX);
                    moveTarget.z = Mathf.Clamp(moveTarget.z, ladderCenter.position.z + minZ, ladderCenter.position.z + maxZ);
                }

                // Rig movement with consistent damping
                playerRig.position = Vector3.Lerp(playerRig.position, moveTarget, 0.25f);
            }

            // Keep the IK hand visually anchored
            if (ikTarget)
                ikTarget.position = Vector3.Lerp(ikTarget.position, grabPoint, Time.deltaTime * stickStrength);

            return true;
        }
        else
        {
            if (grabbing)
            {
                grabbing = false;

                // Release hand — allow IK target to follow tracking again
                if (ikTarget)
                    ikTarget.position = smoothedHand;
            }

            return false;
        }
    }



    bool HandleHand(
    OVRHand ovrHand,
    Transform hand,
    Transform target,
    ref bool grabbing,
    ref Vector3 grabPoint,
    bool isLeft,
    Transform ikTarget // the IK hand target (like leftHandIK)
)
    {
        if (ovrHand == null || hand == null) return false;

        // --- Adaptive smoothing setup ---
        ref Vector3 smoothedHand = ref (isLeft ? ref smoothedLeft : ref smoothedRight);

        if (smoothedHand == Vector3.zero)
            smoothedHand = hand.position;

        Vector3 rawHand = hand.position;
        Vector3 handVelocity = (rawHand - smoothedHand) / Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = handVelocity.magnitude;

        float adaptiveSmooth = Mathf.Lerp(slowAmount, slowAmount * 6f, Mathf.Clamp01(speed / 1.5f));
        smoothedHand = Vector3.Lerp(smoothedHand, rawHand, Time.deltaTime * adaptiveSmooth);
        // --- End smoothing ---

        float pinch = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool pinching = pinch > pinchThreshold;

        // --- Detect nearest LadderPoint ---
        LadderPoint nearestPoint = null;
        Collider[] hits = Physics.OverlapSphere(smoothedHand, grabRadius, ladderMask);
        float closest = float.MaxValue;

        foreach (var hit in hits)
        {
            LadderPoint lp = hit.GetComponent<LadderPoint>();
            if (lp)
            {
                float dist = Vector3.Distance(smoothedHand, lp.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    nearestPoint = lp;
                }
            }
        }

        bool nearLadder = nearestPoint != null;

        if (pinching && nearLadder)
        {
            if (!grabbing)
            {
                grabbing = true;

                // Use the LadderPoint's Y position only; keep player’s XZ
                Vector3 anchor = nearestPoint.handPoint.position;
                //grabPoint = new Vector3(smoothedHand.x, anchor.y, smoothedHand.z);
                grabPoint = new Vector3(anchor.x, anchor.y, smoothedHand.z);


                // Mark latest grabbing hand
                lastActiveLeft = isLeft;

                // Place IK hand immediately on grab point
                if (ikTarget)
                    ikTarget.position = grabPoint;
            }

            Vector3 handDelta = grabPoint - smoothedHand;
            Vector3 moveTarget = playerRig.position + handDelta * climbMultiplier;

            if ((isLeft && lastActiveLeft) || (!isLeft && !lastActiveLeft))
            {
                if (ladderCenter)
                {
                    moveTarget.x = Mathf.Clamp(moveTarget.x, ladderCenter.position.x + minX, ladderCenter.position.x + maxX);
                    moveTarget.z = Mathf.Clamp(moveTarget.z, ladderCenter.position.z + minZ, ladderCenter.position.z + maxZ);
                }

                playerRig.position = Vector3.Lerp(playerRig.position, moveTarget, 0.25f);
            }

            // Keep the IK hand anchored at grabPoint
            if (ikTarget)
                ikTarget.position = Vector3.Lerp(ikTarget.position, grabPoint, Time.deltaTime * stickStrength);

            return true;
        }
        else
        {
            if (grabbing)
            {
                grabbing = false;

                if (ikTarget)
                    ikTarget.position = smoothedHand;
            }

            return false;
        }
    }






    private void OnDisable()
    {
        leftGrabbing = rightGrabbing = false;
    }

    void OnDrawGizmosSelected()
    {
        if (leftHand)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(leftHandGrab.position, grabRadius);
        }
        if (rightHand)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rightHandGrab.position, grabRadius);
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
