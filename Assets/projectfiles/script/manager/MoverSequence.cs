using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverSequence : MonoBehaviour
{
    public void MoveBelowTarget(Transform targetChild)
    {
        if (targetChild == null) return;

        int targetIndex = targetChild.GetSiblingIndex();
        transform.SetSiblingIndex(targetIndex + 1);
    }



}
