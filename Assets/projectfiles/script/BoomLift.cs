using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomLift : MonoBehaviour
{
    public Transform ik;
    public List<Transform> uplist;
    public Transform left;
    public Transform right;

    public float moveSpeed = 2f;

    public bool isUp = false;
    public bool isDown = false;
    public bool isLeft = false;
    public bool isRight = false;

    private int currentIndex = 0;
    public AudioSource Audio;
    void Update()
    {
        if (ik == null) return;

        if (isUp)
        {
            MoveUp();
            if (!Audio.isPlaying)
            {

                Audio.Play();
            }
        }
        else if (isDown)
        {
            MoveDown();
            if (!Audio.isPlaying)
            {

                Audio.Play();
            }
        }
        else if (isLeft)
        {

            MoveSide(left);
            if (!Audio.isPlaying)
            {

                Audio.Play();
            }

        }
        else if (isRight)
        {
            MoveSide(right);

            if (!Audio.isPlaying)
            {

                Audio.Play();
            }
        }
        else
        {
            Audio.Stop();
        }
    }

    void MoveUp()
    {
        if (uplist == null || uplist.Count == 0) return;

        if (currentIndex < uplist.Count)
        {
            Transform target = uplist[currentIndex];
            ik.position = Vector3.MoveTowards(ik.position, target.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(ik.position, target.position) < 0.05f)
                currentIndex++;
        }
    }

    void MoveDown()
    {
        if (uplist == null || uplist.Count == 0) return;

        if (currentIndex > 0)
        {
            Transform target = uplist[currentIndex - 1];
            ik.position = Vector3.MoveTowards(ik.position, target.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(ik.position, target.position) < 0.05f)
                currentIndex--;
        }
    }

    void MoveSide(Transform sideTarget)
    {
        if (sideTarget == null) return;

        ik.position = Vector3.MoveTowards(ik.position, sideTarget.position, moveSpeed * Time.deltaTime);
    }



    public void boomLiftAnim(string temp)
    {
        isUp = false;
        isDown = false;
        isLeft= false;
        isRight= false;
        
        if (temp == "up")
        {
            isUp = true;
        }
        else if (temp == "down")
        {
            isDown = true;
        }
        else if (temp == "left")
        {
            isLeft = true;
        }
        else if (temp == "right")
        {
            isRight = true;
        }
     
        
    }
}
