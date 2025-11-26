using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class VRMapHead
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
}
public class IKHeaderFollow : MonoBehaviour
{

      [Range(0,1)]
    public float turnSmoothness = 0.1f;
    public VRMapHead head;


      public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void LateUpdate()
    {
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        head.Map();


    }
}
