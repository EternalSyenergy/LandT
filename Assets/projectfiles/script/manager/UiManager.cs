using Oculus.Voice.Core.Bindings.Android.PlatformLogger;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class UiManager : MonoBehaviour
{


    public static UiManager instance;
    public LogoHandler logoHandler;
    public uiobject uiObject;
    [Header("Ui Licator and reference")]
    public LoaderUi loaderUi;
    public InsturctorUi instructorUi;
    public UiHandler uiHandler;
    public RightWrongUi RightWrongUi;
    public PlayeUiRef playeUiRef;

    public SettingUpUi settingUpUi;
    public ChapterManagementUi chapterManagementUi;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        logoupdate();
    }

    // Update is called once per frame
    void Update()
    {

        //check tipper inspection is completed




    }





    #region event handler

    

    //instructor
    public void TransformInstructorUiPos(Transform temp)
    {

        if (temp != null)
        {
            instructorUi.obj.transform.SetPositionAndRotation(temp.position, temp.rotation);

        }


        else
        {
            instructorUi.obj.transform.SetPositionAndRotation( instructorUi.instructorUiLocator.position, instructorUi.instructorUiLocator.rotation);

        }
    }

    public void TransformInstructorUiPosNew()
    {

        if (instructorUi.instructorUiType == "r")
        {
            instructorUi.obj.transform.SetPositionAndRotation(instructorUi.instructorUiLocatorR.position, instructorUi.instructorUiLocatorR.rotation);
        }
        else
        {
            instructorUi.obj.transform.SetPositionAndRotation(instructorUi.instructorUiLocator.position, instructorUi.instructorUiLocator.rotation);

        }
    }



    #endregion



    #region loader setup


    Coroutine loaderRoutine;

    public void StartLoading(Sprite image, string content)
    {
        loaderUi.updateLoader(image, content);

        if (loaderRoutine != null)
            StopCoroutine(loaderRoutine);

        loaderRoutine = StartCoroutine(fillLoader());
    }
    IEnumerator fillLoader()
    {
        float duration = 5f;
        float elapsed = 0f;

        loaderUi.LoaderSlider.value = 0f;
        loaderUi.LoaderObject.gameObject.SetActive(true);
        loaderUi.LoaderObject.updateFarUi();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            loaderUi.LoaderSlider.value = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        loaderUi.LoaderSlider.value = 1f;
        loaderUi.LoaderObject.gameObject.SetActive(false);

        loaderRoutine = null;
    }


    //public void StartLoading(Sprite Image, string content)
    //{
    //    loaderUi.updateLoader(Image, content);
    //    StartCoroutine(fillLoader());
    //}



    //IEnumerator fillLoader()
    //{
    //    float duration = 5f;
    //    float elapsed = 0f;
    //    loaderUi.LoaderSlider.value = 0f;
    //    loaderUi.LoaderObject.gameObject.SetActive(true);
    //    loaderUi.LoaderObject.updateFarUi();
    //    while (elapsed < duration)
    //    {
    //        elapsed += Time.deltaTime;
    //        loaderUi.LoaderSlider.value = Mathf.Clamp01(elapsed / duration);
    //        yield return null;
    //    }

    //    loaderUi.LoaderSlider.value = 1f;
    //    loaderUi.LoaderObject.gameObject.SetActive(false);
    //    StopCoroutine(fillLoader() );
    //}

    #endregion



    public void logoupdate() {
    
    
        for(int i = 0; i < logoHandler.logoImage.Count; i++)
        {

            logoHandler.logoImage[i].sprite=logoHandler.logos[logoHandler.logoCount];
        }
    }



    #region chapter management


    #endregion
}

[System.Serializable]
public class LogoHandler
{
    public int logoCount;
    public List<Sprite> logos = new List<Sprite>();
    public List<Image> logoImage = new List<Image>();

}

[System.Serializable]
public class uiobject
{
    public List<TMP_Text> LanguagueContentTexts;
    public void updatelanguagueText(string temp)
    {
        foreach (TMP_Text LanguagueContentText in LanguagueContentTexts)
        {
            LanguagueContentText.text = temp;
        }
    }

}

[System.Serializable]
public class LoaderUi
{

    public UiLocator LoaderObject;
    public Image loaderImage;
    public TMP_Text loaderTextTmp;
    public Slider LoaderSlider;


    public void updateLoader(Sprite ImageLoadere, string Text)
    {
        loaderImage.sprite = ImageLoadere;
        loaderTextTmp.text = Text;


    }


}

[System.Serializable]
public class InsturctorUi
{
    public TMP_Text content;
    public GameObject obj;
    public Transform instructorUiLocator;
    public Transform instructorUiLocatorR;
    public string instructorUiType="l";
}

[System.Serializable]
public class RightWrongUi
{
    public Transform obj;
    public Image right;
    public Image wrong;
    public VideoPlayer Vid;
    public UiLocator uiLocator;

}

[System.Serializable]
public class PlayeUiRef
{
    public Transform straight;
    public Transform right;
    public Transform left;
    public Transform down;


}
[System.Serializable]
public class UiHandler
{

    //group ui 

}



[System.Serializable]
public class SettingUpUi
{
    public UiLocator uiLocator;
    public Transform obj;
    public TMP_Text heading;
    public TMP_Text content;
    public Image Image;

}




[System.Serializable]
public class ChapterManagementUi
{

    public GameObject selectChapterObj;


}










