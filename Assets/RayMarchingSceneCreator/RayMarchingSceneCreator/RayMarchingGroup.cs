using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RaymarchinObjProps;

[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class RayMarchingGroup : HierarchyNavigation
{
    public static Dictionary<string, int> funcIndicies = new Dictionary<string, int>
        {
            { "MIN" , 1},
            { "MAX" , 2},
            { "Smooth MIN" , 3},
            { "Smooth MAX" , 4}
        };
    private string[] GetFunc()
    {
        return new string[]
        {
            "MIN",
            "MAX",
            "Smooth MIN",
            "Smooth MAX"
        };
    }
    [Dropdown(nameof(GetFunc))]
    public string Function = "MIN";
    public bool IsNegative;
    private bool GroupPredicate() => (Function == "Smooth MIN" || Function == "Smooth MAX");
    [ConditionalField(true, nameof(GroupPredicate))]
    [Range(0.0001f, 9.99f)]
    public float Factor = 0.5f;

}
