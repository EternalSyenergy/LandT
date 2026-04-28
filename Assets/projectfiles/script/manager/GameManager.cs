using ItSeez3D.AvatarSdk.Oculus.HandTracking;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public Bulp bulp;
   
    public GameObject directionalLight;


    public SequenceHandler sequenceHandler;
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
      

        PlayerData.trackHarness();
        PlayerData.TrackWelding();
        PlayerData.TrackLockout();

    }
    private void FixedUpdate()
    {
        EyeInteraction();
    }

    private void LateUpdate()
    {
        UpdateLeftHandHookTip();
        UpdateRightHandHookTip();

        UpdateLeftHandHookWallTip();
        UpdateRightHandHookWallTip();

        fixteleportUpdate();
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
    public GameObject impactPlayer;
    public ImpactCharacter impactPlayerInstance;
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

    public void updateLanguague(int temp)
    {
        Languages = temp;
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

        PlayerData.leftLegSolver.ResetFootInstant();
        PlayerData.rightLegSolver.ResetFootInstant();


    }



  

    // Fix player to a point (and start following it)
    private Transform fixedTarget;
    public bool isFixed = false;

    void fixteleportUpdate()
    {
        //if (isFixed && fixedTarget != null)
        //{
        //    // Lock player position to target
        //    PlayerData.player.transform.position = fixedTarget.position;
        //    PlayerData.player.transform.rotation = fixedTarget.rotation;



        //}


        if (isFixed && fixedTarget != null)
        {
            // This is the "Easy Way":
            // Make the player a child of the target. Unity now handles movement perfectly.
            PlayerData.player.transform.SetParent(fixedTarget);

            // Reset local position/rotation to align perfectly with the target
            PlayerData.player.transform.localPosition = Vector3.zero;
            PlayerData.player.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Unparent the player when they are free to move again
            PlayerData.player.transform.SetParent(null);
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

        Transform child = instructor.transform.GetChild(3);
        child.localRotation = Quaternion.identity;

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

    public void InitiateRigPlayer(bool rig=true)
    {

        if (rig)
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

            impactPlayerInstance = ragdollInstance.GetComponent<ImpactCharacter>();
            // Optionally copy pose if your ragdoll has matching bones
            //CopyPlayerPoseToRagdoll(ragdollInstance);
            Destroy(ragdollInstance, 10f);

        }
        else
        {



            Transform playerRoot = PlayerData.player.transform;
            playerRoot.position = new Vector3(playerRoot.position.x, playerRoot.position.y + 0f, playerRoot.position.z);
            if (playerRoot == null || impactPlayer == null)
            {
                Debug.LogWarning("Player root or ragdoll prefab not assigned.");
                return;
            }

            // Get the spawn position & rotation from the current player
            Vector3 spawnPos = playerRoot.position;
            Quaternion spawnRot = playerRoot.rotation;

            // Spawn ragdoll
            GameObject ragdollInstance = Instantiate(impactPlayer, spawnPos, spawnRot);

            impactPlayerInstance= ragdollInstance.GetComponent<ImpactCharacter>();
            // Optionally copy pose if your ragdoll has matching bones
            //CopyPlayerPoseToRagdoll(ragdollInstance);
            Destroy(ragdollInstance, 10f);

        }

    }


    public void impactPlayerType(string type)
    {

        if (impactPlayerInstance == null)
        {
            return;
        }

     


        switch (type)
        {
            case "h":

                impactPlayerInstance. helmet.SetActive(false);
                impactPlayerInstance.goggles.SetActive(false);
                break;

            case "s":

                impactPlayerInstance.helmet.SetActive(true);
                impactPlayerInstance.goggles.SetActive(false);

                break;

            case "m":
                impactPlayerInstance.helmet.SetActive(true);
                impactPlayerInstance.shoe.SetActive(true);
                impactPlayerInstance.mask.SetActive(false);
                break;

            case "g":
                //goggles.SetActive(false);
                impactPlayerInstance.helmet.SetActive(true);
                impactPlayerInstance.shoe.SetActive(true); 
                impactPlayerInstance.mask.SetActive(true);
                impactPlayerInstance.goggles.SetActive(false);

                break;

            case "e":
                impactPlayerInstance.helmet.SetActive(true);
                impactPlayerInstance.shoe.SetActive(true);
                impactPlayerInstance.mask.SetActive(true);
                impactPlayerInstance.goggles.SetActive(true);
                impactPlayerInstance.earPlug.SetActive(false);

                break;


            case "ha":
                //impactPlayerInstance.helmet.SetActive(true);
                //impactPlayerInstance.shoe.SetActive(true);
                //impactPlayerInstance.mask.SetActive(true);
                //impactPlayerInstance.goggles.SetActive(true);
                //impactPlayerInstance.earPlug.SetActive(false);
                impactPlayerInstance.harness.SetActive(false);

                break;
            default:

                break;
        }


    }

    public void InitiateRigPlayerAnim(string type) 
    {



        if (impactPlayerInstance==null)
        {
            return;
        }


        impactPlayerInstance.PlayAnim(type);




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


    #region Harness

    // =========================
    // Harness Enable / Disable
    // =========================
    public void EnableHarness(bool value)
    {
        if (PlayerData. playerHarness != null)
            PlayerData.playerHarness.SetActive(value);
    }

    // =========================
    // LEFT HAND
    // =========================

    // Call when left hook starts (button press / hit / attach)
    //public void HarnessLeftHookTipMove(Transform target)
    //{
    //    PlayerData.leftTarget = target;
    //    PlayerData.isHarnessLeft = true;
    //}

    // Called every LateUpdate
    private void UpdateLeftHandHookTip()
    {
        if (!PlayerData.isHarnessLeft || PlayerData.leftTarget == null || PlayerData.harnessLeftTip == null)
            return;

        PlayerData.harnessLeftTip.position = PlayerData.leftTarget.position;
        PlayerData.harnessLeftTip.rotation = PlayerData.leftTarget.rotation;
    }

    private void UpdateLeftHandHookWallTip()
    {
        if (PlayerData.leftWallTarget == null || PlayerData.harnessLeftTip == null)
            return;

        PlayerData.harnessLeftTip.position = PlayerData.leftWallTarget.position;
        PlayerData.harnessLeftTip.rotation = PlayerData.leftWallTarget.rotation;
    }

    // Call to release / cancel left hook
    public void UpdateLeftHandHookPosition(Transform returnTarget)
    {
        PlayerData.isHarnessLeft = false;
        PlayerData.leftWallTarget = returnTarget;

        if (returnTarget != null && PlayerData.harnessLeftTip != null)
        {
            PlayerData.harnessLeftTip.position = returnTarget.position;
            PlayerData.harnessLeftTip.rotation = returnTarget.rotation;
        }
    }

    // =========================
    // RIGHT HAND
    // =========================

 
    // Called every LateUpdate
    private void UpdateRightHandHookTip()
    {
        if (!PlayerData.isHarnessRight || PlayerData.rightTarget == null || PlayerData.harnessRightTip == null)
            return;

        PlayerData.harnessRightTip.position = PlayerData.rightTarget.position;
        PlayerData.harnessRightTip.rotation = PlayerData.rightTarget.rotation;
    }

    private void UpdateRightHandHookWallTip()
    {
        if (PlayerData.rightWallTarget == null || PlayerData.harnessRightTip == null)
            return;

        PlayerData.harnessRightTip.position = PlayerData.rightWallTarget.position;
        PlayerData.harnessRightTip.rotation = PlayerData.rightWallTarget.rotation;
    }
    // Call to release / cancel right hook
    public void UpdateRightHandHookPosition(Transform returnTarget)
    {
        PlayerData.isHarnessRight = false;

        PlayerData.rightWallTarget= returnTarget;

        if (returnTarget != null && PlayerData.harnessRightTip != null)
        {
            PlayerData.harnessRightTip.position = returnTarget.position;
            PlayerData.harnessRightTip.rotation = returnTarget.rotation;
        }
    }


    public void UpdateLeftHookInHand(bool temp )
    {
        PlayerData.isHarnessLeft = temp;
        PlayerData.leftWallTarget= null;
    }

    public void UpdateRightHookInHand(bool temp)
    {
        PlayerData.isHarnessRight = temp;
        PlayerData.rightWallTarget = null;

    }
    #endregion


    #region welding 


    public void updateIsWeldinginHand(bool temp)
    {

        PlayerData.isWeldingOnhand = temp;
    }
    public void updateWeldingHandlerObj(Transform temp)
    {

        PlayerData.weldingObject = temp;
    }



    #endregion


    #region lockoutTagout 


    public void updatelockoutOnhand(bool temp)
    {

        PlayerData.isLockoutOnhand = temp;
    }
    public void updatelockoutObject(Transform temp)
    {

        PlayerData.lockoutObject = temp;
    }



    #endregion

    #region


    public void isBulpMove(bool temp)
    {
        bulp.isbulpMove = temp;

    }

    public void updatebulpObject(Transform temp)
    {

        bulp.bulpObj = temp;
    }

    public void upateBulpEndpos(Transform temp)
    {
        bulp.bulpEndPos = temp;
    }

    public void bulpMOvetopoint()
    {
        if (bulp.isbulpMove)
        {
            if (bulp.bulpObj != null && bulp.bulpEndPos != null)
            {
                StartCoroutine(MoveWithShake(bulp.bulpObj.transform, bulp.bulpEndPos.position));
            }
        }
    }

    public void updateBulpshake(float temp)
    {
        bulp.shakeAmount = temp;
    }

    IEnumerator MoveWithShake1(Transform obj, Vector3 targetPos)
    {
        float duration = 1.5f;          // total movement time
      //  float shakeAmount = 0.2f;       // how far left-right
        //float shakeSpeed = 20f;         // how fast shaking

        float shakeAmount = .5f;       // how far left-right
        float shakeSpeed = 5f;         // how fast shaking


        Vector3 startPos = obj.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Smooth movement toward target
            Vector3 movePos = Vector3.Lerp(startPos, targetPos, t);

            // Left-right shake (X axis)
            float shakeOffset = Mathf.Sin(time * shakeSpeed) * shakeAmount * (1 - t);
            movePos.x += shakeOffset;

            obj.position = movePos;

            yield return null;
        }

        obj.position = targetPos;
    }


    IEnumerator MoveWithShake(Transform obj, Vector3 targetPos)
    {
        float moveDuration = bulp.bulpmoveDuration;
       
        float shakeSpeed = bulp.shakeSpeed;

        Vector3 startPos = obj.position;
        float time = 0f;

        // Phase 1: Move to Target while shaking
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = time / moveDuration;

            Vector3 movePos = Vector3.Lerp(startPos, targetPos, t);

            // Removed the (1-t) so the shake doesn't die out
            float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * bulp.shakeAmount;
            movePos.x += shakeOffset;

            obj.position = movePos;
            yield return null;
        }

        // Phase 2: Stay at target but keep swinging
        while (true)
        {
            Vector3 idlePos = targetPos;
            idlePos.z += Mathf.Sin(Time.time * shakeSpeed) *bulp.shakeAmount;
            obj.position = idlePos;
            yield return null;
        }
    }

    #endregion


    ///dark abient
    public void updateDarkAmbientLight(bool temp)
    {
        if (temp)
        {
            directionalLight.SetActive(false);
            RenderSettings.ambientIntensity = 0.5f;
            RenderSettings.ambientIntensity = 0.4f;
        }
        else
        {
            directionalLight.SetActive(true);
            RenderSettings.ambientIntensity = 1.2f;
            RenderSettings.ambientIntensity = 0.9f;

        }
    }


    public void CameraDistaance(float temp)
    {
        PlayerData.playerCam.farClipPlane    = temp;
    }

    #region Chapter Management

    public void updateChapter(string temp)
    {
        GameManager.Instance.sequenceHandler.updateCurrentChapter(temp);
    }
    public void updateChapterIndex(int temp)
    {
        GameManager.Instance.sequenceHandler.updateCurrentChapterIndex(temp);
        updateChapter(GameManager.Instance.sequenceHandler.chapterGroups[temp-1].chapterName);
    }
    public void MoveCurrentChapterAsNextSibling()
    {
        GameManager.Instance.sequenceHandler.MoveCurrentChaptersAsNextSiblings();
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

    public IKFootSolver leftLegSolver;
    public IKFootSolver rightLegSolver;


    [Header("Harness Mechanism")]
    public GameObject playerHarness;
    public Transform playerHarnessTractPos;
    [Header("Hook Tips")]
    public Transform harnessLeftTip;
    public Transform harnessRightTip;
    [Header("State")]
    public bool isHarnessLeft;
    public bool isHarnessRight;
    // Internal targets
    public Transform leftTarget;
    public Transform rightTarget;

    // Internal targets
    public Transform leftWallTarget;
    public Transform rightWallTarget;



    [Header("Welding ")]
    public Transform weldingHandPos;
    public Transform weldingObject;
    public bool isWeldingOnhand;

    public Camera playerCam;
    public void TrackWelding()
    {

        if (isWeldingOnhand)
        {

            if (weldingObject != null && weldingObject != null)
            {

                weldingObject.transform.SetPositionAndRotation(weldingHandPos.position, weldingHandPos.rotation);

            }

        }

    }

    [Header("lockout ")]
    public Transform lockoutHandPos;
    public Transform lockoutObject;
    public bool isLockoutOnhand;

    public void TrackLockout()
    {

        if (isLockoutOnhand)
        {

            if (lockoutObject != null)
            {

                lockoutObject.transform.SetPositionAndRotation(lockoutHandPos.position, lockoutHandPos.rotation);

            }

        }

    }


    public void trackHarness()
    {
        playerHarness.transform.SetPositionAndRotation(playerHarnessTractPos.position, playerHarnessTractPos.rotation);
    }





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


[System.Serializable]
public class Bulp
{
    public Transform bulpObj;
    public Transform bulpEndPos;
    public bool isbulpMove;
    public float shakeAmount = 0.5f;
    public float shakeSpeed = 0.5f;
    public float bulpmoveDuration=1.5f;


}



// 
[System.Serializable]
public class SequenceHandler
{

    public GameObject parent;
    public GameObject targetchild;
    public GameObject finalSubsequence;
    public List<ChapterGroup> chapterGroups;

    public string currentChapter;
    public int currentChapterIndex;
   

    public void updateCurrentChapter(string temp)
    {
        currentChapter = temp;
    }

    public void MoveCurrentChaptersAsNextSiblings()
    {
        // Safety check for parent and references
        if (parent == null || targetchild == null || chapterGroups.Count == 0) return;

        // 1. Find where the targetchild is in the hierarchy
        int targetIndex = targetchild.transform.GetSiblingIndex();

        // 2. Get the list of Chapters for the selected category (PPE, LOTO, etc.)
        ChapterGroup activeGroup = chapterGroups[currentChapterIndex-1];

        // 3. Move each chapter object to be the NEXT sibling
        for (int i = 0; i < activeGroup.Chapters.Count; i++)
        {
            GameObject chapterObj = activeGroup.Chapters[i];

            if (chapterObj != null)
            {
                // Ensure it is under the same parent
                chapterObj.transform.SetParent(parent.transform);

                // Set index to (targetIndex + 1 + i) to stack them right below target
                chapterObj.transform.SetSiblingIndex(targetIndex + 1 + i);

                // Basic transform reset for UI alignment
                //chapterObj.transform.localPosition = Vector3.zero;
                //chapterObj.transform.localRotation = Quaternion.identity;
                //chapterObj.transform.localScale = Vector3.one;

                chapterObj.SetActive(true);
            }
        }
    }

    public void updateCurrentChapterIndex(int temp)
    {
        if (temp >= 0 && temp < chapterGroups.Count)
        {
            currentChapterIndex = temp;
        }
    }   



}
[System.Serializable]
public class ChapterGroup
{
    public List<GameObject> Chapters;
    public string chapterName;
    public int chapterIndex;


}
