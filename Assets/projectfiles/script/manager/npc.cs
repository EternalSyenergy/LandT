using System.Collections;
using UnityEngine;

public class npc : MonoBehaviour
{
    public Animator anime;
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    public float stoppingDistance = 0.05f; // How close it should stop

    private Transform target;
    private bool isMoving = false;

    private bool isRotating = false;


    //harness
    public HarnessArmature harnessPrefab;  // Prefab to instantiate
    private HarnessArmature HarnesArmature; // The runtime instance
    public Transform chArmature;           // Character bone
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero; // in Euler angles


    public Vector3 harnessRigtTip = Vector3.zero;
    public Vector3 harnessLeftTip = Vector3.zero; // in Euler angles

    public Transform LeftEndTip;
    public Transform RightEndTip;
    public bool isHarness = false;
    public bool isHarnessOnPlayer = true;


    public NpcMover npcMover;
    private void Start()
    {
        if (isHarness)
        {
            InitHarness();
        }

        npcMover.transform = transform;

    }
    void Update()
    {
        if (isMoving && target != null)
        {
            Vector3 direction = target.position - transform.position;
            float distance = direction.magnitude;

            if (distance > stoppingDistance)
            {
                // Move towards target
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;

                // Rotate smoothly while moving (face movement direction)
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );

                // Play walking animation
                if (anime != null)
                    anime.SetBool("iswalk", true);
            }
            else
            {
                // Stop walking animation
                isMoving = false;
                if (anime != null)
                    anime.SetBool("iswalk", false);

                // Start final rotation to match target rotation
                isRotating = true;
            }
        }

        // Smoothly rotate to match target rotation after reaching
        if (isRotating && target != null)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                rotateSpeed * Time.deltaTime
            );

            // Stop rotating when very close to target rotation
            if (Quaternion.Angle(transform.rotation, target.rotation) < 0.1f)
            {
                transform.rotation = target.rotation;
                isRotating = false;
            }
        }


        npcMover.movementUpdate(anime);
    }

    void LateUpdate()
    {
        if (HarnesArmature != null && chArmature != null)
        {
            HarnesArmature.armature.transform.position = chArmature.position + positionOffset;
            HarnesArmature.armature.transform.rotation = chArmature.rotation * Quaternion.Euler(rotationOffset);
        }


        if (isHarnessOnPlayer)
        {

            if (HarnesArmature.leftHandTip != null)
            {
                HarnesArmature.leftHandTip.SetPositionAndRotation(
     LeftEndTip.position,
     LeftEndTip.rotation
 );

            }


            if (HarnesArmature.RightHandTip != null)
            {
                HarnesArmature.RightHandTip.SetPositionAndRotation(
        RightEndTip.position,
        RightEndTip.rotation
    );
            }


        }
    }


    public void movetoPoint(Transform pos)
    {
        target = pos;
        isMoving = true;
    }

    public void Playanim(string name)
    {
        anime.SetBool(name, true);
    }

    public void Stopnim(string name)
    {
        anime.SetBool(name, false);
    }

    //public void InitHarness()
    //{
    //    if (HarnesArmature == null || chArmature == null)
    //    {
    //        Debug.LogWarning("Harness or character armature not assigned!");
    //        return;
    //    }

    //    isHarness = true;
    //    // Parent the harness to the character bone (optional, can stay unparented if you prefer)
    //    HarnesArmature.transform.SetParent(chArmature, worldPositionStays: false);

    //    // Apply position and rotation offset
    //    HarnesArmature.transform.localPosition = positionOffset;
    //    HarnesArmature.transform.localRotation = Quaternion.Euler(rotationOffset);
    //}


    public void InitHarness()
    {
        if (chArmature == null)
        {
            Debug.LogWarning("Character armature not assigned!");
            return;
        }

        // Instantiate harness if it doesn't exist
        if (HarnesArmature == null && harnessPrefab != null)
        {
            HarnesArmature = Instantiate(harnessPrefab);
        }

        if (HarnesArmature == null)
        {
            Debug.LogWarning("Harness prefab not assigned or failed to instantiate!");
            return;
        }

        isHarness = true;

        // Parent harness to character bone
        HarnesArmature.transform.SetParent(gameObject.transform, worldPositionStays: false);

        // Apply offsets
        HarnesArmature.transform.localPosition = positionOffset;
        HarnesArmature.transform.localRotation = Quaternion.Euler(rotationOffset);





        //if (HarnesArmature.leftHandTip != null)
        //    HarnesArmature.leftHandTip.localPosition = harnessLeftTip;

        //if (HarnesArmature.RightHandTip != null)
        //    HarnesArmature.RightHandTip.localPosition = harnessRigtTip;


        if (isHarnessOnPlayer)
        {

            if (HarnesArmature.leftHandTip != null)
            {
                HarnesArmature.leftHandTip.SetPositionAndRotation(
     LeftEndTip.position,
     LeftEndTip.rotation
 );

            }


            if (HarnesArmature.RightHandTip != null)
            {
                HarnesArmature.RightHandTip.SetPositionAndRotation(
        RightEndTip.position,
        RightEndTip.rotation
    );
            }

        }


        //HarnesArmature.transform.localScale = Vector3.one; // optional reset
    }


    //harnes update 
    public void harnessLeftHook(Transform temp)
    {
        isHarnessOnPlayer = false;
        HarnesArmature.leftHandTip.SetPositionAndRotation(
         temp.position,
           temp.rotation
           );



    }
    public void harnessRightHook(Transform temp)
    {
        isHarnessOnPlayer = false;
        HarnesArmature.RightHandTip.SetPositionAndRotation(
          temp.position,
            temp.rotation
            );
    }
    public void harnessOnPlayer(bool temp)
    {
        isHarnessOnPlayer= temp;
    }


    ///movemnet
    public void isMove(bool temp)
    {
        npcMover.ismoving = temp;
    }
    public void updateMovePoint(Transform temp)
    {
        npcMover.currentWaypoint = temp;
    }
}

[System.Serializable]
public class NpcMover
{

    public Transform currentWaypoint;
    public float moveSpeed = 2f;
    public float rotateSpeed = 5f;
    public float reachDistance = 0.2f;
    public bool ismoving=false;
    [HideInInspector]
    public Transform transform;

public void movementUpdate(Animator anim)
    {


        if (!ismoving)
        {
            anim.SetBool("iswalk", false);
            return;

        }
          
        if (currentWaypoint == null)
        {

            anim.SetBool("iswalk", false);
             return;
        }


        if(!anim.GetBool("iswalk"))
        anim.SetBool("iswalk", true);

        // Direction to waypoint
        Vector3 direction = currentWaypoint.position - transform.position;
        direction.y = 0f; // Keep rotation flat (important)

        // Smooth rotation
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }

        // Move forward in facing direction
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, currentWaypoint.position) <= reachDistance)
        {
            Waypoint wp = currentWaypoint.GetComponent<Waypoint>();
            if (wp != null)
            {
                currentWaypoint = wp.nextWaypoint;
            }
        }
    }

}