using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Interaction : MonoBehaviour
{


    public startEvent startEvent;
    SubSequence subSequence;




    private Coroutine languageCoroutine;
    private void Awake()
    {
        subSequence = gameObject.transform.GetComponentInParent<SubSequence>();

    }
    void Start()
    {
    }

    void Update()
    {

    }

    #region sequence intercation Handler
    public void StartInteractionEvent()
    {
        startEvent.Event.Invoke();
        LanguageHandler();
        handleContent();
        HandleTimerInteraction();





    }
    void HandleTimerInteraction()
    {
        if (startEvent.isTimeIntercation)
        {
            CancelInvoke("CompleteInteractionEvent");
            Invoke("CompleteInteractionEvent", startEvent.intercationTimer);

        }

    }
    void handleCompleteEvent()
    {
       startEvent.offEvent.Invoke();
        if (languageCoroutine != null)
        {
            StopCoroutine(languageCoroutine);

            if (GameManager.Instance.audioManager.mainAudio.isPlaying)
            {
                GameManager.Instance.audioManager.mainAudio.Stop();
            }
            languageCoroutine = null;
        }
        subSequence.currentInteractionCount++;
        subSequence.StartInteraction();
    }

    //handle event
    public void CompleteInteractionEvent()
    {

        if (!startEvent.isTimeIntercation)
        {

            CancelInvoke("handleCompleteEvent");
            Invoke("handleCompleteEvent", startEvent.intercationTimer);
        }
        else
        {
            handleCompleteEvent();
        }


    }

    #endregion

    #region handle content

    void handleContent()
    {

        if (startEvent.content.Count > 0)
        {


            UiManager.instance.uiObject.updatelanguagueText(startEvent.content[GameManager.Instance.Languages]);



        }
    }
    #endregion

    #region Language Player
    void LanguageHandler()
    {
        if (startEvent.Language.Count > 0)
        {
            AudioClip selectedClip = startEvent.Language[GameManager.Instance.Languages];
            GameManager.Instance.audioManager.mainAudio.clip = selectedClip;

            // Stop any currently running coroutine before starting a new one

            if (languageCoroutine != null)
            {
                StopCoroutine(languageCoroutine);
                languageCoroutine = null;
            }
            languageCoroutine = StartCoroutine(RepeatedSequenceAudio());
        }
        else
        {
            if (GameManager.Instance.audioManager.mainAudio.isPlaying)
            {
                GameManager.Instance.audioManager.mainAudio.Stop();

            }
        }
    }

    IEnumerator RepeatedSequenceAudio()
    {
        AudioSource mainAudio = GameManager.Instance.audioManager.mainAudio;

        while (true)
        {
            if (mainAudio.clip == null)
                yield break; // no clip to play, exit loop

            mainAudio.Play();

            // Wait for the clip to finish
            yield return new WaitForSeconds(mainAudio.clip.length + 10);

            // Wait for the extra 10 seconds gap
            yield return new WaitForSeconds(10f);
        }
    }
    #endregion

    #region PLAYER EASYACCES
    public void offFixed()
    {
        GameManager.Instance.isFixed = false;

    }
    public void PlayerFrontdown()
    {

        GameManager.Instance.PlayerFrontdown();

    }
    public void PlayerBackdown()
    {
        GameManager.Instance.PlayerBackdown();

    }

    public void PlayerFallFrontdown()
    {
        GameManager.Instance.PlayerFallFrontdown();

    }

    public void ZoomOut(Transform Temp)
    {

        GameManager.Instance.ZoomOut(Temp);

    }


    public void TeleportPlayerFixed(Transform temp)
    {
        GameManager.Instance.TeleportPlayerFixed(temp);
    }

    public void TeleportPlayer(Transform temp)
    {
        GameManager.Instance.TeleportPlayer(temp, false);
    }

    #endregion

    #region run time add interaction
    public void MoveObjects(Transform targetChild)
    {
        if (targetChild == null || startEvent.listOfInteraction.interaction == null) return;

        Transform parent = targetChild.parent;
        if (parent == null) return;

        int insertIndex = targetChild.GetSiblingIndex() + 1;

        foreach (var obj in startEvent.listOfInteraction.interaction)
        {
            if (obj == null) continue;

            // make them siblings of targetChild
            obj.transform.SetParent(parent);

            // put them just after targetChild in the hierarchy
            obj.transform.SetSiblingIndex(insertIndex);

            obj.subSequence = subSequence;

            insertIndex++; // next object stays below the previous one
        }
    }
    public void quizObjectMove(Transform targetChild)
    {
        if (targetChild == null || GameManager.Instance.quizObject.Count <= 0) return;

        // Find target's sibling index
        int targetIndex = targetChild.GetSiblingIndex();

        // Randomize the quizObject list
        System.Random rng = new System.Random();
        var shuffledList = GameManager.Instance.quizObject.OrderBy(x => rng.Next()).ToList();

        // Insert them right after the target
        for (int i = 0; i < shuffledList.Count; i++)
        {
            shuffledList[i].transform.SetSiblingIndex(targetIndex + 1 + i);
        }
    }

    #endregion

    #region ui
    public void StartLoader(Sprite LoaderImage)
    {
        //UiManager.instance.StartLoading(LoaderImage,startEvent.content[GameManager.Instance.Languages]);
        UiManager.instance.StartLoading(LoaderImage, (startEvent.content?.Count > GameManager.Instance.Languages ? startEvent.content[GameManager.Instance.Languages] : "") ?? "");


    }
    public void InstructorUiPos(Transform temp)
    {
        UiManager.instance.TransformInstructorUiPos(temp);
    }

    //right wrong ui
    public void UpdateRightWrongUi(Transform temp)
    {
        RightWrongUiActive(false);
        UiManager.instance.RightWrongUi.obj.SetPositionAndRotation(temp.position, temp.rotation);
        UiManager.instance.RightWrongUi.right.sprite = startEvent.rightUi;
        UiManager.instance.RightWrongUi.wrong.sprite = startEvent.wrongUi;
    }

    public void UpdateRightWrongUiPlayer(string temp)
    {
        RightWrongUiActive(false);
        UiManager.instance.RightWrongUi.right.sprite = startEvent.rightUi;
        UiManager.instance.RightWrongUi.right.sprite = startEvent.wrongUi;

        switch (temp)
        {
            case "s":
                UiManager.instance.RightWrongUi.obj.SetPositionAndRotation(UiManager.instance.playeUiRef.straight.position, UiManager.instance.playeUiRef.straight.rotation);
                break;
            case "r":
                UiManager.instance.RightWrongUi.obj.SetPositionAndRotation(UiManager.instance.playeUiRef.right.position, UiManager.instance.playeUiRef.right.rotation);
                break;
            case "l":
                UiManager.instance.RightWrongUi.obj.SetPositionAndRotation(UiManager.instance.playeUiRef.left.position, UiManager.instance.playeUiRef.left.rotation);
                break;
            case "d":
                UiManager.instance.RightWrongUi.obj.SetPositionAndRotation(UiManager.instance.playeUiRef.down.position, UiManager.instance.playeUiRef.down.rotation);
                break;

            default:
                break;
        }

    }

    public void RightWrongUiActive(bool temp)
    {


        GameManager.Instance.audioManager.uiSfx.clip = GameManager.Instance.sfxList.RightWrongUiSFx;
        GameManager.Instance.audioManager.uiSfx.Play();
        UiManager.instance.RightWrongUi.Vid.clip = startEvent.vidUi;
        UiManager.instance.RightWrongUi.obj.gameObject.SetActive(temp);



    }
    //end

    public void InstructorUiSetActive(bool temp)
    {
        UiManager.instance.TransformInstructorUiPos(null);

        UiManager.instance.instructorUi.obj.SetActive(temp);
    }
    #endregion

    #region SFX
    public void NpcSfxupdate(AudioClip sfx)
    {
        GameManager.Instance.audioManager.npxSfxAudio.clip = sfx;

    }
    public void NpcSfxPlay(bool isplay)
    {
        if (isplay)
        {
            GameManager.Instance.audioManager.npxSfxAudio.Play();

        }
        else
        {
            GameManager.Instance.audioManager.npxSfxAudio.Stop();

        }
    }
    public void NpcSfxSound(float temp)
    {
        GameManager.Instance.audioManager.npxSfxAudio.volume = temp;


    }
    public void PlayerSfx(AudioClip temp)
    {
        GameManager.Instance.PlayerSfx(temp);
    }
    public void PlayerSecSfx(AudioClip temp)
    {
        GameManager.Instance.PlayerSecSfx(temp);
    }

    #endregion


 ///blood effect
 ///
 public void ToggleBloodEffect(bool temp)
    {
        GameManager.Instance.BloodEffect(temp);
    }

}



[Serializable]
public class startEvent
{
    public UnityEvent Event;
    public UnityEvent offEvent;
    public List<AudioClip> Language;
    public List<String> content;
    public bool isTimeIntercation;
    public float intercationTimer;
    [Header("runtime interaction")]
    public ListOfInteraction listOfInteraction;

    [Header("RIGHT WRONG REF")]
    public Sprite rightUi;
    public Sprite wrongUi;
    public VideoClip vidUi;



}


[System.Serializable]
public class ListOfInteraction
{
    public List<Interaction> interaction;
}

