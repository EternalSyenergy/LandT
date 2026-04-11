



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    public bool isMovingForward;

    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] IKFootSolver otherFoot = default;

    [SerializeField] float speed = 4;
    [SerializeField] float stepDistance = 0.25f;
    [SerializeField] float stepLength = 0.35f;
    [SerializeField] float sideStepLength = 0.15f;

    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] Vector3 footOffset = default;

    public Vector3 footRotOffset;
    public float footYPosOffset = 0.1f;

    public float rayStartYOffset = 0.5f;
    public float rayLength = 2f;

    // NEW: step cooldown
    [SerializeField] float stepCooldown = 0.15f;
    float lastStepTime;

    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    float lerp;

    private void Start()
    {
        footSpacing = transform.localPosition.x;

        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;

        lerp = 1;
    }

    void Update()
    {
        // apply final foot transform
        transform.position = currentPosition + Vector3.up * footYPosOffset;

        // rotate foot to match ground normal + offset
        Quaternion targetRot = Quaternion.LookRotation(body.forward, currentNormal) * Quaternion.Euler(footRotOffset);
        transform.rotation = targetRot;

        // cast ray
        Vector3 rayOrigin =
            body.position +
            body.right * footSpacing +
            Vector3.up * rayStartYOffset;

        Ray ray = new Ray(rayOrigin, Vector3.down);
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.green);

        if (Physics.Raycast(ray, out RaycastHit info, rayLength, terrainLayer))
        {
            float dist = Vector3.Distance(newPosition, info.point);

            bool canStep =
                dist > stepDistance &&
                !otherFoot.IsMoving() &&
                lerp >= 1f &&
                Time.time - lastStepTime > stepCooldown;

            if (canStep)
            {
                lastStepTime = Time.time;
                lerp = 0;

                Vector3 direction = (info.point - currentPosition);
                direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

                float angle = Vector3.Angle(body.forward, direction);

                isMovingForward = angle < 75;

                if (isMovingForward)
                    newPosition = info.point + direction * stepLength + footOffset;
                else
                    newPosition = info.point + direction * sideStepLength + footOffset;

                newNormal = info.normal;
            }
        }

        if (lerp < 1)
        {
            float t = lerp;

            Vector3 target = Vector3.Lerp(oldPosition, newPosition, t);
            target.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

            currentPosition = target;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, t);

            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.06f);
    }

    public bool IsMoving()
    {
        return lerp < 1;
    }




    public void ResetFootInstant()
    {
        // stop any stepping
        lerp = 1f;
        lastStepTime = Time.time;

        // recalc spacing (safe if teleport changed scale/pose)
        footSpacing = transform.localPosition.x;

        // raycast straight down to find new ground
        Vector3 rayOrigin =
            body.position +
            body.right * footSpacing +
            Vector3.up * rayStartYOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayLength, terrainLayer))
        {
            oldPosition = currentPosition = newPosition = hit.point + footOffset;
            oldNormal = currentNormal = newNormal = hit.normal;
        }
        else
        {
            // fallback: stick foot under body if no ground hit
            Vector3 fallbackPos = body.position + body.right * footSpacing;
            oldPosition = currentPosition = newPosition = fallbackPos;
            oldNormal = currentNormal = newNormal = Vector3.up;
        }

        // apply immediately
        transform.position = currentPosition + Vector3.up * footYPosOffset;
        transform.rotation =
            Quaternion.LookRotation(body.forward, currentNormal) *
            Quaternion.Euler(footRotOffset);
    }



}
