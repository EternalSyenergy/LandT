using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }



    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation;

    /// <summary>
    /// Call this to map the VR target to the IK target.
    /// Automatically handles tracking loss by keeping the last valid pose.
    /// </summary>
    /// <param name="isTracked">Is the VR target currently tracked?</param>
    public void Map(bool isTracked,bool grab)
    {
        if (isTracked && vrTarget != null)
        {
            // Update hand/head normally
            lastValidRotation = ikTarget.rotation;

            // Store last valid pose
            lastValidPosition = ikTarget.position;
            if (!grab)
            {

                ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
                ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);

            }
        }
        else
        {
            // Tracking lost — keep hand/head at last valid pose
            ikTarget.position = lastValidPosition;
            ikTarget.rotation = lastValidRotation;
        }
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    [Range(0,1)]
    public float turnSmoothness = 0.1f;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;


    /// <summary>
    /// 
    /// 
    /// </summary>
    /// 
    public OVRHand left;
    public OVRHand right;

    // Update is called once per frame

    private void Update()
    {
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y + headBodyYawOffset;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        head.Map();


        //if(PlayerHandInteraction.instance.rightHand.IsTracked)
        //{
        //rightHand.Map();

        //}


        //if(PlayerHandInteraction.instance.leftHand.IsTracked)
        //{

        //leftHand.Map();
        //}

        // Right hand

        // Left hand

        bool rightTracked = right.IsTracked; // adjust as needed
        rightHand.Map(rightTracked, OVRHandTrackingLadderClimbSmooth.instance.rightGrabbing);

        // Map left hand with tracking check
        bool leftTracked = left.IsTracked;// adjust as needed
        leftHand.Map(leftTracked, OVRHandTrackingLadderClimbSmooth.instance.leftGrabbing);

    }

    void LateUpdate()
    {
       
    }
}
