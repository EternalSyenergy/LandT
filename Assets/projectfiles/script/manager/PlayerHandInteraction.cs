using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandInteraction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        buttonInteraction(other);
    }


    void buttonInteraction(Collider other)
    {
        if(other.gameObject.GetComponent<HandTriggerEvent>())
        {
            HandTriggerEvent interaction = other.gameObject.GetComponent<HandTriggerEvent>();
            interaction.Event.Invoke();
            if (GameManager.Instance.audioManager.uiSfx.clip != null)
            {

                if (GameManager.Instance.audioManager.uiSfx.isPlaying)
                {
                    GameManager.Instance.audioManager.uiSfx.Stop();

                }
                GameManager.Instance.audioManager.uiSfx.Play();


            }
        }
    }




}
