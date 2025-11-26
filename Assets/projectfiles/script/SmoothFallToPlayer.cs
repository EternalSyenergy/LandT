using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class SmoothFallToPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform pointA;          // Start position (spawn point)
    public Transform player;          // Player reference

    [Header("Settings")]
    public float heightAbovePlayer = 5f;  // Target height above player
    public float moveSpeed = 10f;         // Movement speed toward target

    [Header("Position Offset")]
    public Vector3 offset;
    private Rigidbody rb;
    private bool isFalling = false;
    private Vector3 targetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;   // We control movement manually
        player = GameManager.Instance.PlayerData.player.transform;
}

    void Update()
    {
        if (isFalling)
        {
            MoveTowardTarget();
        }
    }

    public void StartFall()
    {
        if (pointA == null || player == null)
            return;

        // Move to point A first
        transform.position = pointA.position;

        // Reset velocity so it starts fresh
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calculate target position above player
        targetPosition = player.position+offset + Vector3.up * heightAbovePlayer;

        // Start moving
        isFalling = true;
    }

    //void MoveTowardTarget()
    //{
    //    // Calculate direction toward target
    //    Vector3 direction = (targetPosition - transform.position).normalized;

    //    // Move smoothly using Rigidbody
    //    rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);

    //    // Optional: stop when close enough to target
    //    if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
    //    {
    //        isFalling = false;
    //        rb.useGravity = true; // Let gravity take over if needed
    //    }
    //}


    void MoveTowardTarget()
    {
        // Calculate direction toward target
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Smoothly rotate toward movement direction (Y-axis only if you want horizontal rotation)
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                5f * Time.deltaTime // rotation speed
            );
        }

        // Move smoothly using Rigidbody
        rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);

        // Stop when close to target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isFalling = false;
            rb.useGravity = true; // Optional: let gravity take over after reaching target
        }
    }


    void OnDrawGizmos()
    {
        if (pointA == null || player == null)
            return;

        //Vector3 targetPos = player.position + Vector3.up * heightAbovePlayer;
        Vector3 targetPos = player.position+offset + Vector3.up * heightAbovePlayer;

        // Draw line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pointA.position, targetPos);

        // Start sphere (Point A)
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(pointA.position, 0.2f);

        // Target sphere (Above player)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetPos, 0.2f);
    }
}
