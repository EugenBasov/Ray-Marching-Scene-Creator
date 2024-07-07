using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyNavigation : MonoBehaviour
{
    [HideInInspector]
    public int perfectSubtreeHeight;
    void Start()
    {
        perfectSubtreeHeight = 0;
    }

    void Update()
    {
        //if(name == "Main Group")
        //    Debug.Log(name + " - " + perfectSubtreeHeight);
        int maxHeight = 0;
        int maxesCount = 0;
        HierarchyNavigation chld;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).TryGetComponent<HierarchyNavigation>(out chld);
            if (chld != null && maxHeight == chld.perfectSubtreeHeight)
            {
                maxesCount += 1;
            }
            if (chld != null && maxHeight < chld.perfectSubtreeHeight)
            {
                maxHeight = chld.perfectSubtreeHeight;
                maxesCount = 1;
            }
        }
        perfectSubtreeHeight = maxHeight + (maxesCount >= 2 ? 1 : 0);
    }

}
