using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{

    public static SequenceManager instance;

    public bool isEvaluation;
    public GameObject currenctSubSequence;
    public GameObject currentInteraction;
    public int currentSubseuenceCount=0;
    public int totalsubcount=0;
    private void Awake()
    {
        instance=this;
    }
    void Start()
    {
        totalsubcount = gameObject.transform.childCount;

        StartsubSequence(currentSubseuenceCount);

    }
    public void StartsubSequence(int temp)
    {

        if(temp < totalsubcount)
        {
            currenctSubSequence = gameObject.transform.GetChild(temp).gameObject;
            gameObject.transform.GetChild(temp).GetComponent<SubSequence>().StartInteraction();
        }
        else
        {
            Debug.Log("sequence completed");
        }
      
    }
    void Update()
    {
        
    }


}
