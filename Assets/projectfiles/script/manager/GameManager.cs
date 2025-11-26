using ItSeez3D.AvatarSdk.Oculus.HandTracking;
using Meta.WitAi;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{


    public static GameManager Instance;

    [Tooltip("English - 0, Tamil - 1 ,Hindi - 2 , Malayalam - 3 ")]
    public int Languages = 0;
    public PlayerData PlayerData;
    public GameObject instructor;
    public AudioManager audioManager;

    public SfxList sfxList;
    public bool isEvaluation;
    public ObjectManager objectManager;
    public bool vrisInput;
    public List<Transform> quizObject;

    public Image bloodOverlay;
    public void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
    }


    private void Update()
    {
        fixteleportUpdate();
    }
    private void FixedUpdate()
    {
        EyeInteraction();
    }

    void EyeInteraction()
    {
        RaycastHit hit;

        // Cast a ray and check if it hits something in the eyeInteraction layer
        if (Physics.Raycast(
            PlayerData.playerHeadUi.position,
            PlayerData.playerHeadUi.forward,
            out hit,
           Mathf.Infinity,
            PlayerData.eyeInteraction
        ))
        {
            // Trigger the interaction
            SequenceManager.instance.currentInteraction
                .GetComponent<Interaction>()
                .CompleteInteractionEvent();

            // Deactivate the hit object
            hit.collider.gameObject.SetActive(false);
        }
    }





    #region Player functionality ZOOMOUT,BACK FRONT DEATH FALL,TELEPORT ,INSTRUCTOR TELEPORT,PLAYER GRAVITY FORCE,PLAUERRIG


    [Header("Rotation Settings")]
    public float rotateSpeed = 2f;
    [Header("Camera Settings")]
    public GameObject ovrRig;
    public float zoomOutSize = 15f;
    public float zoomSpeed = 2f;
    [Header("References")]
    public OVRScreenFade screenFade;       // OVRScreenFade component on CenterEyeAnchor
    public float fadeDuration = 0.3f;
    public GameObject RigPlayer;
    public float playerMoveSpeed = 1f;

    private bool isRotating = false;
    public float gravityforce = -4.81f;


    public void PlayerFrontdown()
    {
        if (!isRotating)
            StartCoroutine(RotatePlayer(Vector3.right * 90f)); // Forward (X+)
    }

    public void PlayerBackdown()
    {
        if (!isRotating)
            StartCoroutine(RotatePlayer(Vector3.left * 90f)); // Backward (X−)
    }

    public void PlayerFallFrontdown()
    {
        if (!isRotating)
            StartCoroutine(RotateAndFall(Vector3.right * 90f));
    }

    public void ZoomOut(Transform target)
    {
        if (ovrRig != null)
            StartCoroutine(ZoomOutCoroutine(target));
    }



    private IEnumerator RotatePlayer(Vector3 eulerTarget)
    {
        isRotating = true;

        Transform player = GameManager.Instance.PlayerData.player.transform;
        Quaternion startRot = player.rotation;
        Quaternion endRot = Quaternion.Euler(eulerTarget.x, player.eulerAngles.y, player.eulerAngles.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;
            player.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        player.rotation = endRot;
        isRotating = false;
    }

    private IEnumerator RotateAndFall(Vector3 eulerTarget)
    {
        // First rotate
        yield return RotatePlayerRoutine(eulerTarget);

        // Then make player "fall" — example using Y position drop
        Transform player = PlayerData.player.transform;
        Vector3 startPos = player.position;
        Vector3 endPos = startPos - new Vector3(0, 1.5f, 0); // Drop down a bit

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;
            player.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        player.position = endPos;
    }

    private IEnumerator RotatePlayerRoutine(Vector3 eulerTarget)
    {
        isRotating = true;

        Transform player = PlayerData.player.transform;
        Quaternion startRot = player.rotation;
        Quaternion endRot = Quaternion.Euler(eulerTarget.x, player.eulerAngles.y, player.eulerAngles.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;
            player.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        player.rotation = endRot;
        isRotating = false;
    }

    private IEnumerator ZoomOutCoroutine(Transform targetPoint)
    {
        if (ovrRig == null || targetPoint == null) yield break;


        PlayerData.PlayerSkin.SetActive(false);
        Transform rigTransform = ovrRig.transform;

        Vector3 startPos = rigTransform.position;
        Vector3 endPos = targetPoint.position;

        Quaternion startRot = rigTransform.rotation;
        Quaternion endRot = Quaternion.LookRotation(
            GameManager.Instance.PlayerData.player.transform.position - endPos, // look back at player
            Vector3.up
        );

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * zoomSpeed;

            // Smooth position movement
            rigTransform.position = Vector3.Lerp(startPos, endPos, t);

            // Smooth rotation to look at player
            rigTransform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // Final snap to make sure no rounding drift
        rigTransform.position = endPos;
        rigTransform.rotation = endRot;
    }


    public void TeleportPlayerFixed(Transform target)
    {
        StartCoroutine(TeleportWithFadeRoutine(target,false,true));


    }
    public void TeleportPlayer(Transform target, bool keepYRotation)
    {
        StartCoroutine(TeleportWithFadeRoutine(target, keepYRotation, false));
    }

    private IEnumerator TeleportWithFadeRoutine(Transform target, bool keepYRotation, bool isFixxed)
    {
        if (screenFade == null)
        {
            Debug.LogWarning("No OVRScreenFade assigned. Teleporting instantly.");
            DoTeleport(target, keepYRotation);
            yield break;
        }

        // Fade out
        screenFade.fadeTime = fadeDuration;
        screenFade.FadeOut();
        yield return new WaitForSeconds(fadeDuration);

        // Teleport

        if (isFixxed)
        {
            FixToPointTeleport(target);
        }
        else
        {
            DoTeleport(target, keepYRotation);

        }

        // Fade in
        screenFade.FadeIn();
    }



    // Fix player to a point (and start following it)
    private Transform fixedTarget;
    public bool isFixed = false;

    void fixteleportUpdate()
    {
        if (isFixed && fixedTarget != null)
        {
            // Lock player position to target
            PlayerData.player.transform.position = fixedTarget.position;
            PlayerData.player.transform.rotation = fixedTarget.rotation;


            // Lock rotation
            //if (keepYRotation)
            //{
            //    Vector3 rot = PlayerData.player.transform.eulerAngles;
            //    rot.y = fixedTarget.eulerAngles.y;
            //    PlayerData.player.transform.eulerAngles = rot;
            //}
            //else
            //{
            //    PlayerData.player.transform.rotation = fixedTarget.rotation;
            //}
        }
    }
    public void FixToPointTeleport(Transform target)
    {

        Transform playerRoot = PlayerData.player.transform;
        fixedTarget = target;
        isFixed = true;
        PlayerData.PlayerSkin.SetActive(true);
        // Teleport instantly
        playerRoot.position = target.position;
        playerRoot.rotation = target.rotation;
        // Reset position and rotation
        PlayerData.ovrRig.transform.localPosition = Vector3.zero;
        PlayerData.ovrRig.transform.localRotation = Quaternion.identity;

        // Optional: reset internal camera offset
        Recenter();
    }
    private void DoTeleport(Transform target, bool keepYRotation)
    {

        isFixed = false;
        PlayerData.PlayerSkin.SetActive(true);
        Transform playerRoot = PlayerData.player.transform;
        if (playerRoot == null || target == null) return;

        playerRoot.position = target.position;

        if (keepYRotation)
        {
            Quaternion currentRot = playerRoot.rotation;
            playerRoot.rotation = Quaternion.Euler(
                currentRot.eulerAngles.x,
                target.eulerAngles.y,
                currentRot.eulerAngles.z
            );
        }
        else
        {
            playerRoot.rotation = target.rotation;
        }


        // Reset position and rotation
        PlayerData.ovrRig.transform.localPosition = Vector3.zero;
        PlayerData.ovrRig.transform.localRotation = Quaternion.identity;

        PlayerData.PlayerSkin.transform.localPosition = Vector3.zero;
        PlayerData.PlayerSkin.transform.localRotation = Quaternion.identity;


        if (OVRManager.instance != null && OVRManager.display != null)
        {
            //OVRManager.display.RecenterPose();
            Recenter();
        }




    }

    public void Recenter()
    {
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(subsystems);

        foreach (var s in subsystems)
            s.TryRecenter();

        Debug.Log("View recentered!");
    }

    public void TeleportInstructor(Transform target)
    {
        instructor.transform.SetPositionAndRotation(target.position, target.rotation);
    }

    private Coroutine moveCoroutine;

    public void PlayerMoveToTarget(Transform target)
    {
        if (target == null) return;

        // If there's already a move coroutine running, stop it first
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveCoroutine = StartCoroutine(MovePlayerSmooth(target));
    }

    public void cancelPlayerMoveTarget()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }
    private IEnumerator MovePlayerSmooth(Transform target)
    {
        Transform playerRoot = PlayerData.player.transform; // your parent player object
        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;

        // Lock Y position to the current player height
        Vector3 endPos = new Vector3(
            target.position.x,
            startPos.y,
            target.position.z
        );

        Quaternion endRot = target.rotation;

        float duration = playerMoveSpeed;    // adjust for desired speed
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            playerRoot.position = Vector3.Lerp(startPos, endPos, t);
            playerRoot.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        playerRoot.position = endPos;
        playerRoot.rotation = endRot;
    }



    public void IsPlayerGravity(float time)
    {
        StartCoroutine(ApplyTransformGravity(time));
    }
    private IEnumerator ApplyTransformGravity(float duration)
    {
        Transform playerRoot = PlayerData.player.transform;  // your player parent object
        float elapsed = 0f;
        Vector3 velocity = Vector3.zero;
        //float gravity = -9.81f;  // meters per second squared
        float gravity = gravityforce;  // meters per second squared


        PlayerData.PlayerSkin.SetActive(false); // your player parent object
        PlayerData.fallPlayerSKin.SetActive(true);
        PlayerData.fallPlayerSKin.transform.SetPositionAndRotation(PlayerData.PlayerSkin.transform.position, PlayerData.PlayerSkin.transform.rotation);
        while (elapsed < duration)
        {

            // Wait until LateUpdate has finished for this frame
            elapsed += Time.deltaTime;

            // Apply gravity to velocity (falling down)
            velocity.y += gravity * Time.deltaTime;

            // Move player downward
            playerRoot.position += velocity * Time.deltaTime;

            yield return null;
        }
        PlayerData.fallPlayerSKin.SetActive(false);


    }

    public void InitiateRigPlayer()
    {
        Transform playerRoot = PlayerData.player.transform;
        playerRoot.position = new Vector3(playerRoot.position.x, playerRoot.position.y + 0.51f, playerRoot.position.z);
        if (playerRoot == null || RigPlayer == null)
        {
            Debug.LogWarning("Player root or ragdoll prefab not assigned.");
            return;
        }

        // Get the spawn position & rotation from the current player
        Vector3 spawnPos = playerRoot.position;
        Quaternion spawnRot = playerRoot.rotation;

        // Spawn ragdoll
        GameObject ragdollInstance = Instantiate(RigPlayer, spawnPos, spawnRot);

        // Optionally copy pose if your ragdoll has matching bones
        //CopyPlayerPoseToRagdoll(ragdollInstance);
        Destroy(ragdollInstance, 10f);
    }


    #endregion


    public void PlayerSfx(AudioClip clip)
    {
        audioManager.playerSfxAudio.clip = clip;
        audioManager.playerSfxAudio.Play();

    }
    public void PlayerSecSfx(AudioClip clip)
    {
        audioManager.playerSecSfxAudio.clip = clip;
        audioManager.playerSecSfxAudio.Play();

    }
    public void ChangeScene(int temp)
    {
        SceneManager.LoadScene(temp);
    }



    #region 

    IEnumerator ToggleBloodEffect(bool enable, float intensity = 0.7f, float fadeSpeed = 3f)
    {
        if (bloodOverlay == null) yield break;

        float startAlpha = bloodOverlay.color.a;
        float targetAlpha = enable ? intensity : 0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t);
            Color c = bloodOverlay.color;
            c.a = a;
            bloodOverlay.color = c;
            yield return null;
        }

        Color final = bloodOverlay.color;
        final.a = targetAlpha;
        bloodOverlay.color = final;
    }
    public void BloodEffect(bool temp)
    {
        StartCoroutine(ToggleBloodEffect(temp));
    }
    #endregion
}

[System.Serializable]
public class PlayerData
{
    public GameObject player;
    public GameObject ovrRig;
    public LayerMask eyeInteraction;

    public Transform playerHeadUi;
    public Transform farUi;
    public PlayerHandInteraction rightHand;
    public PlayerHandInteraction leftHand;
    public GameObject PlayerSkin;
    public GameObject fallPlayerSKin;

    public Rigidbody playerRb;
}

[System.Serializable]
public class AudioManager
{
    public AudioSource mainAudio;
    public AudioSource playerSfxAudio;
    public AudioSource playerSecSfxAudio;
    public AudioSource npxSfxAudio;
    public AudioSource uiSfx;

}

[System.Serializable]
public class ObjectManager
{
    //insturctor
    public GameObject InsturctorObject;

    //handpost
    public Transform vestPos;
    public Transform helmetPos;
    public Transform shoePos;
    public string handposName;



}



[System.Serializable]
public class GrabPos
{
    public AvatarSDKHandTracking LHandSdk;
    public AvatarSDKHandTracking RHandSdk;
    public Transform BodyPos;
    public Transform LeftHandpalm;
    public Transform RightHandpalm;
}



[System.Serializable]
public class SfxList
{
    public AudioClip RightWrongUiSFx;

}



