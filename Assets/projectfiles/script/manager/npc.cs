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
}
