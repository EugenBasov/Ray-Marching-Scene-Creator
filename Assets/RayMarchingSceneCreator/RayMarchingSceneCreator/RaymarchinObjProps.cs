using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

[SelectionBase]
[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class RaymarchinObjProps : HierarchyNavigation
{
    [HideInInspector]
    public int hierarchyHeight = 0;

    public static Dictionary<string, int> shapesIndicies = new Dictionary<string, int>
        {
            {"SPHERE", 0},
            {"SPHERE_CUT", 1},
            {"SPHERE_CUT_HOLLOW", 2},
            {"SPHERE_DEATH_STAR", 3},
            {"CUBE", 4},
            {"CUBE_ROUND", 5},
            {"CUBE_FRAME", 6},
            {"TORUS", 7},
            {"TORUS_CAPPED", 8},
            {"LINK", 9},
            {"CONE", 10},
            {"CONE_CAPPED_VERTICAL", 11},
            {"CONE_CAPPED_2DOTS", 12},
            {"CONE_ROUND_VERTICAL", 13},
            {"CONE_ROUND_2DOTS", 14},
            {"CONE_INFINITE", 15},
            {"PLANE", 16},
            {"PRISM", 17},
            {"PRISM_HEX", 18},
            {"CAPSULE_VERTICAL", 19},
            {"CAPSULE_2DOTS", 20},
            {"CYLINDER_VERTICAL", 21},
            {"CYLINDER_2DOTS", 22},
            {"CYLINDER_ROUND", 23},
            {"CYLINDER_INFINITE", 24},
            {"SOLID_ANGLE", 25},
            {"VESICA", 26},
            {"RHOMBUS", 27},
            {"OCTAHEDRON", 28},
            {"PYRAMID", 29},
            {"TRIANGLE", 30},
            {"QUAD", 31}
        };
    private string[] GetShape()
    {
        return new string[]
        {
            "SPHERE",
            "CUBE",
            //"TORUS",
            //"LINK",
            "CONE",
            "PLANE",
            //"PRISM",
            "CAPSULE",
            "CYLINDER",
            //"SOLID_ANGLE",
            //"VESICA",
            //"RHOMBUS",
            //"OCTAHEDRON",
            //"PYRAMID",
            //"TRIANGLE",
            //"QUAD",
        };
    }
    private string[] GetSubShapeSphere()
    {
        return new string[]
        {
            "SPHERE",
            "SPHERE_CUT",
            "SPHERE_CUT_HOLLOW",
            "SPHERE_DEATH_STAR"
        };
    }
    private string[] GetSubShapeCube()
    {
        return new string[]
        {
            "CUBE",
            "CUBE_ROUND",
            "CUBE_FRAME"
        };
    }
    private string[] GetSubShapeCone()
    {
        return new string[]
        {
            //"CONE",
            "CONE_CAPPED_VERTICAL",
            //"CONE_CAPPED_2DOTS",
            "CONE_ROUND_VERTICAL",
            //"CONE_ROUND_2DOTS",
            //"CONE_INFINITE"
        };
    }
    private string[] GetSubShapePlane()
    {
        return new string[]
        {
            "PLANE"
        };
    }
    private string[] GetSubShapeCapsule()
    {
        return new string[]
        {
            "CAPSULE_VERTICAL",
            "CAPSULE_2DOTS"
        };
    }
    private string[] GetSubShapeCylinder()
    {
        return new string[]
        {
            "CYLINDER_VERTICAL",
            //"CYLINDER_2DOTS",
            "CYLINDER_ROUND",
            //"CYLINDER_INFINITE",
        };
    }
    private string[] GetSubShapePyramid()
    {
        return new string[]
        {
            "PYRAMID"
        };
    }

    /*public enum SUB_SHAPE_TORUS
    {
        TORUS = 7,
        TORUS_CAPPED = 8,
    }
    public enum SUB_SHAPE_LINK
    {
        LINK = 9,
    }*/
    /*public enum SUB_SHAPE_PRISM
    {
        PRISM = 17,
        PRISM_HEX = 18,
    }*/
    /*public enum SUB_SHAPE_SOLID_ANGLE
    {
        SOLID_ANGLE = 25
    }
    public enum SUB_SHAPE_VESICA
    {
        VESICA = 26
    }
    public enum SUB_SHAPE_RHOMBUS
    {
        RHOMBUS = 27
    }
    public enum SUB_SHAPE_OCTAHEDRON
    {
        OCTAHEDRON = 28
    }*/
    /*public enum SUB_SHAPE_TRIANGLE
    {
        TRIANGLE = 30
    }
    public enum SUB_SHAPE_QUAD
    {
        QUAD = 31
    }*/


    [Dropdown(nameof(GetShape))]
    public string Shape = "SPHERE";
    public bool IsNegative;
    [HideInInspector]
    public int shapeSizeInFloats;

    //[ConditionalField(nameof(Shape), false, "SPHERE")]
    //[Dropdown(nameof(GetSubShapeSphere))]
    [HideInInspector]
    public string SubShapeSphere = "SPHERE";

    //[ConditionalField(nameof(Shape), false, "CUBE")]
    //[Dropdown(nameof(GetSubShapeCube))]
    //public string SubShapeCube = "CUBE";
    [ConditionalField(nameof(Shape), false, "CUBE")]
    [Range(0.0f, 1.0f)]
    public float CubeRounding = 0.0f;
    [ConditionalField(nameof(Shape), false, "CUBE")]
    [Range(0.0f, 1.0f)]
    public float FrameThiñkness = 1.0f;

    /*[ConditionalField(nameof(Shape), false, "TORUS")]
    public SUB_SHAPE_TORUS SubShape_Torus;
    [ConditionalField(nameof(Shape), false, "LINK")]
    public SUB_SHAPE_LINK SubShape_Link;*/

    [ConditionalField(nameof(Shape), false, "CONE")]
    [Dropdown(nameof(GetSubShapeCone))]
    public string SubShapeCone = "CONE_CAPPED_VERTICAL";

    //[ConditionalField(nameof(Shape), false, "PLANE")]
    //[Dropdown(nameof(GetSubShapePlane))]
    [HideInInspector]
    public string SubShapePlane = "PLANE";

    /*[ConditionalField(nameof(Shape), false, "PRISM")]
    public SUB_SHAPE_PRISM SubShape_Prism;*/

    //[ConditionalField(nameof(Shape), false, "CAPSULE")]
    //[Dropdown(nameof(GetSubShapeCapsule))]
    [HideInInspector]
    public string SubShapeCapsule = "CAPSULE_VERTICAL";
    [ConditionalField(nameof(Shape), false, "CYLINDER")]
    [Dropdown(nameof(GetSubShapeCylinder))]
    public string SubShapeCylinder = "CYLINDER_VERTICAL";

    /*[ConditionalField(nameof(Shape), false, "SOLID_ANGLE")]
    public SUB_SHAPE_SOLID_ANGLE SubShape_SolidAngle;
    [ConditionalField(nameof(Shape), false, "VESICA")]
    public SUB_SHAPE_VESICA SubShape_Vesica;
    [ConditionalField(nameof(Shape), false, "RHOMBUS")]
    public SUB_SHAPE_RHOMBUS SubShape_Rhombus;
    [ConditionalField(nameof(Shape), false, "OCTAHEDRON")]
    public SUB_SHAPE_OCTAHEDRON SubShape_Octahedron;*/

    //[ConditionalField(nameof(Shape), false, "PYRAMID")]
    //[Dropdown(nameof(GetSubShapePyramid))]
    //public string SubShapePyramid = "PYRAMID";

    /*[ConditionalField(nameof(Shape), false, "TRIANGLE")]
    public SUB_SHAPE_TRIANGLE SubShape_Triangle;
    [ConditionalField(nameof(Shape), false, "QUAD")]
    public SUB_SHAPE_QUAD SubShape_Quad;*/


    [ConditionalField(new[] { nameof(Shape), nameof(SubShapeSphere) }, new[] { false, false }, "SPHERE", "SPHERE_DEATH_STAR")]
    public float BigRadius = 1;

    [ConditionalField(new[] { nameof(Shape), nameof(SubShapeCapsule) }, new[] { false, false }, "CAPSULE", "CAPSULE_VERTICAL")]
    public float CapsuleRadius = 1;
    [ConditionalField(new[] { nameof(Shape), nameof(SubShapeCapsule) }, new[] { false, false }, "CAPSULE", "CAPSULE_VERTICAL")]
    public float CapsuleHeight = 1;

    private bool ConePredicate() => (Shape == "CONE" &&
       (SubShapeCone == "CONE_CAPPED_VERTICAL" ||
        SubShapeCone == "CONE_CAPPED_2DOTS" ||
        SubShapeCone == "CONE_ROUND_VERTICAL" ||
        SubShapeCone == "CONE_ROUND_2DOTS")
        );
    [ConditionalField(true, nameof(ConePredicate))] public float ConeBottomRadius = 1;
    [ConditionalField(true, nameof(ConePredicate))] public float ConeUpperRadius = 1;
    [ConditionalField(true, nameof(ConePredicate))] public float ConeHeight = 1;

    private bool CylinderPredicate() => (Shape == "CYLINDER" &&
       (SubShapeCylinder == "CYLINDER_VERTICAL" ||
        SubShapeCylinder == "CYLINDER_ROUND")
        );
    [ConditionalField(true, nameof(CylinderPredicate))] public float CylinderRadius = 1;
    [ConditionalField(true, nameof(CylinderPredicate))] public float CylinderHeight = 1;

    [ConditionalField(new[] { nameof(Shape), nameof(SubShapeCylinder) }, new[] { false, false }, "CYLINDER", "CYLINDER_ROUND")]
    public float CylinderRounding = 1;



    [Header("Material")]

    public Color DiffuseColor = Color.white;
    [Range(0.0f, 1.0f)]
    public float Roughness = 1.0f;
    [Range(0.0f, 1.0f)]
    public float Specular = 0.0f;
    public Color SpecularColor = Color.white;
    public Texture2D DiffuseTexture;
    public bool TextureFixedSize = false;
    public UnityEngine.Vector3 TextureStretch = UnityEngine.Vector3.one;

    [Header("Domain Repetition")]
    [RangeVector(new float[]{1, 1, 1}, new float[] { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity })]
    public UnityEngine.Vector3Int RepetitionCount = UnityEngine.Vector3Int.one;
    [RangeVector(new float[] { 0, 0, 0 }, new float[] { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity })]
    [HideInInspector]
    public UnityEngine.Vector3Int RepetitionCarvePoint1 = UnityEngine.Vector3Int.zero;
    [HideInInspector]
    public UnityEngine.Vector3Int RepetitionCarvePoint2 = UnityEngine.Vector3Int.zero;
    public UnityEngine.Vector3 RepetitionRotation = UnityEngine.Vector3.zero;
    public UnityEngine.Vector3 RepetitionOffset = UnityEngine.Vector3.one;
    [Tooltip("Fixes artifacts caused by object's rotation, but decreases performance. Isn't needed, if object is symmetrical.")]
    public bool FixArtifacts = false;


    /*  [ConditionalField(new[] { nameof(Shape), nameof(SubShapePlane) }, new[] { false, false }, "PLANE", "PLANE")]
        public UnityEngine.Vector3 n = new UnityEngine.Vector3(1.0f,0.0f,0.0f);
        [ConditionalField(new[] { nameof(Shape), nameof(SubShapePlane) }, new[] { false, false }, "PLANE", "PLANE")]
        public float PlaneHeight = 1;*/

    /*private void Start()
    {
        shapesIndicies = new Dictionary<string, int>
        {
            {"SPHERE", 0},
            {"SPHERE_CUT", 1},
            {"SPHERE_CUT_HOLLOW", 2},
            {"SPHERE_DEATH_STAR", 3},
            {"CUBE", 4},
            {"CUBE_ROUND", 5},
            {"CUBE_FRAME", 6},
            {"TORUS", 7},
            {"TORUS_CAPPED", 8},
            {"LINK", 9},
            {"CONE", 10},
            {"CONE_CAPPED_VERTICAL", 11},
            {"CONE_CAPPED_2DOTS", 12},
            {"CONE_ROUND_VERTICAL", 13},
            {"CONE_ROUND_2DOTS", 14},
            {"CONE_INFINITE", 15},
            {"PLANE", 16},
            {"PRISM", 17},
            {"PRISM_HEX", 18},
            {"CAPSULE_VERTICAL", 19},
            {"CAPSULE_2DOTS", 20},
            {"CYLINDER_VERTICAL", 21},
            {"CYLINDER_2DOTS", 22},
            {"CYLINDER_ROUNDED", 23},
            {"CYLINDER_INFINITE", 24},
            {"SOLID_ANGLE", 25},
            {"VESICA", 26},
            {"RHOMBUS", 27},
            {"OCTAHEDRON", 28},
            {"PYRAMID", 29},
            {"TRIANGLE", 30},
            {"QUAD", 31}
        };
    }*/


}
