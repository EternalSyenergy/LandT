using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactCharacter : MonoBehaviour
{


    public Animator anim;
    public GameObject helmet;
    public GameObject shoe;
    public GameObject harness;
    public GameObject goggles;
    public GameObject mask;
    public GameObject earPlug;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAnim(string type)
    {
        switch (type)
        {
            case "h":

                helmet.SetActive(false);
                anim.SetBool("ishead", true);
                break;

            case "s":


                anim.SetBool("isshoe", true);
                break;

            case "m":

                mask.SetActive(false);
                anim.SetBool("ismask", true);
                break;

            case "g":
                //goggles.SetActive(false);

                anim.SetBool("isgoggle", true);
                break;

            case "e":
                earPlug.SetActive(false);
                anim.SetBool("isearplug", true);
                break;

            default:
                anim.SetBool(type, true);
                break;
        }
    }


}
