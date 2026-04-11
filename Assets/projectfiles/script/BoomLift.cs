//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class BoomLift : MonoBehaviour
//{
//    public Transform ik;
//    public List<Transform> uplist;
//    public Transform left;
//    public Transform right;

//    public float moveSpeed = 2f;
//    public float BoomliftMoveSpeed = 1f;
//    public bool isUp = false;
//    public bool isDown = false;
//    public bool isLeft = false;
//    public bool isRight = false;

//    private int currentIndex = 0;
//    public AudioSource Audio;
//    private Coroutine moveCoroutine;
//    void Update()
//    {
//        if (ik == null) return;

//        if (isUp)
//        {
//            MoveUp();
//            if (!Audio.isPlaying)
//            {

//                Audio.Play();
//            }
//        }
//        else if (isDown)
//        {
//            MoveDown();
//            if (!Audio.isPlaying)
//            {

//                Audio.Play();
//            }
//        }
//        else if (isLeft)
//        {

//            MoveSide(left);
//            if (!Audio.isPlaying)
//            {

//                Audio.Play();
//            }

//        }
//        else if (isRight)
//        {
//            MoveSide(right);

//            if (!Audio.isPlaying)
//            {

//                Audio.Play();
//            }
//        }
//        else
//        {
//            Audio.Stop();
//        }
//    }

//    void MoveUp()
//    {
//        if (uplist == null || uplist.Count == 0) return;

//        if (currentIndex < uplist.Count)
//        {
//            Transform target = uplist[currentIndex];
//            ik.position = Vector3.MoveTowards(ik.position, target.position, moveSpeed * Time.deltaTime);

//            if (Vector3.Distance(ik.position, target.position) < 0.05f)
//                currentIndex++;
//        }
//    }

//    void MoveDown()
//    {
//        if (uplist == null || uplist.Count == 0) return;

//        if (currentIndex > 0)
//        {
//            Transform target = uplist[currentIndex - 1];
//            ik.position = Vector3.MoveTowards(ik.position, target.position, moveSpeed * Time.deltaTime);

//            if (Vector3.Distance(ik.position, target.position) < 0.05f)
//                currentIndex--;
//        }
//    }

//    void MoveSide(Transform sideTarget)
//    {
//        if (sideTarget == null) return;

//        ik.position = Vector3.MoveTowards(ik.position, sideTarget.position, moveSpeed * Time.deltaTime);
//    }



//    public void boomLiftMOve(Transform target)
//    {
//        if (target == null) return;

//        // If there's already a move coroutine running, stop it first
//        if (moveCoroutine != null)
//        {
//            StopCoroutine(moveCoroutine);
//            moveCoroutine = null;
//        }

//        moveCoroutine = StartCoroutine(MovePlayerSmooth(target));
//    }

//    private IEnumerator MovePlayerSmooth(Transform target)
//    {
//        Transform playerRoot = gameObject.transform; // your parent player object
//        Vector3 startPos = playerRoot.position;
//        Quaternion startRot = playerRoot.rotation;

//        // Lock Y position to the current player height
//        Vector3 endPos = new Vector3(
//            target.position.x,
//            startPos.y,
//            target.position.z
//        );

//        Quaternion endRot = target.rotation;

//        float duration = BoomliftMoveSpeed;    // adjust for desired speed
//        float t = 0f;

//        while (t < 1f)
//        {
//            t += Time.deltaTime / duration;
//            playerRoot.position = Vector3.Lerp(startPos, endPos, t);
//            playerRoot.rotation = Quaternion.Slerp(startRot, endRot, t);
//            yield return null;
//        }

//        playerRoot.position = endPos;
//        playerRoot.rotation = endRot;
//    }

//    public void boomLiftAnim(string temp)
//    {
//        isUp = false;
//        isDown = false;
//        isLeft= false;
//        isRight= false;

//        if (temp == "up")
//        {
//            isUp = true;
//        }
//        else if (temp == "down")
//        {
//            isDown = true;
//        }
//        else if (temp == "left")
//        {
//            isLeft = true;
//        }
//        else if (temp == "right")
//        {
//            isRight = true;
//        }


//    }
//}





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
    public float BoomliftMoveSpeed = 1f;
    public bool isUp = false;
    public bool isDown = false;
    public bool isLeft = false;
    public bool isRight = false;

    private int currentIndex = 0;
    public AudioSource Audio;
    private Coroutine moveCoroutine;

    void Update()
    {
        if (ik == null) return;

        if (isUp)
        {
            MoveUp();
            PlayAudio();
        }
        else if (isDown)
        {
            MoveDown();
            PlayAudio();
        }
        else if (isLeft)
        {
            MoveSide(left);
            PlayAudio();
        }
        else if (isRight)
        {
            MoveSide(right);
            PlayAudio();
        }
        else
        {
            StopAudio();
        }
    }

    void MoveUp()
    {
        if (uplist == null || uplist.Count == 0) return;

        if (currentIndex < uplist.Count)
        {
            Transform target = uplist[currentIndex];

            ik.position = Vector3.MoveTowards(
                ik.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(ik.position, target.position) <= 0.02f)
            {
                currentIndex++;
            }
        }
    }

    void MoveDown()
    {
        if (uplist == null || uplist.Count == 0) return;

        if (currentIndex > 0)
        {
            Transform target = uplist[currentIndex - 1];

            ik.position = Vector3.MoveTowards(
                ik.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(ik.position, target.position) <= 0.02f)
            {
                currentIndex--;
            }
        }
    }

    void MoveSide(Transform sideTarget)
    {
        if (sideTarget == null) return;

        ik.position = Vector3.MoveTowards(
            ik.position,
            sideTarget.position,
            moveSpeed * Time.deltaTime
        );
    }

    public void boomLiftMOve(Transform target)
    {
        if (target == null) return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MovePlayerSmooth(target));
    }

    IEnumerator MovePlayerSmooth(Transform target)
    {
        Transform playerRoot = transform;

        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;

        Vector3 endPos = new Vector3(
            target.position.x,
            startPos.y,
            target.position.z
        );

        Quaternion endRot = target.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / BoomliftMoveSpeed;

            playerRoot.position = Vector3.Lerp(startPos, endPos, t);
            playerRoot.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        playerRoot.position = endPos;
        playerRoot.rotation = endRot;
    }

    void PlayAudio()
    {
        if (!Audio.isPlaying)
            Audio.Play();
    }

    void StopAudio()
    {
        if (Audio.isPlaying)
            Audio.Stop();
    }

    public void boomLiftAnim(string temp)
    {
        isUp = isDown = isLeft = isRight = false;

        if (temp == "up") isUp = true;
        else if (temp == "down") isDown = true;
        else if (temp == "left") isLeft = true;
        else if (temp == "right") isRight = true;
    }
}
