using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static RayMarchingObjectManager;

[ExecuteInEditMode]
public class RayMarchingObjectManager : MonoBehaviour
{
    public const int RayMarchingObjectByteSize = sizeof(float) * (1+1+3+3+3+12+3+3+3+3+3+1);
    public const int RayMarchingMaterialByteSize = sizeof(float) * (3+1+1+3+1+3);
    public struct RayMarchingObject
    {
        public int type;
        public int group;
        public float3 position;
        public float3 rotation;
        public float3 scale;
        public otherParameters otherParams; // 12
        public struct otherParameters
        {
            private float param0;
            private float param1;
            private float param2;
            private float param3;
            private float param4;
            private float param5;
            private float param6;
            private float param7;
            private float param8;
            private float param9;
            private float param10;
            private float param11;
            public float this[int i]
            {
                get { return GetParam(i); }
                set { SetParam(i, value); }
            }

            private float GetParam(int octantIndex)
            {
                switch (octantIndex)
                {
                    case 0: return param0;
                    case 1: return param1;
                    case 2: return param2;
                    case 3: return param3;
                    case 4: return param4;
                    case 5: return param5;
                    case 6: return param6;
                    case 7: return param7;
                    case 8: return param8;
                    case 9: return param9;
                    case 10: return param10;
                    case 11: return param11;
                    default:
                        throw new IndexOutOfRangeException($"otherParams index i must be 0 <= i < 12; got {octantIndex}");
                }
            }

            private void SetParam(int octantIndex, float newValue)
            {
                switch (octantIndex)
                {
                    case 0:
                        param0 = newValue;
                        break;
                    case 1:
                        param1 = newValue;
                        break;
                    case 2:
                        param2 = newValue;
                        break;
                    case 3:
                        param3 = newValue;
                        break;
                    case 4:
                        param4 = newValue;
                        break;
                    case 5:
                        param5 = newValue;
                        break;
                    case 6:
                        param6 = newValue;
                        break;
                    case 7:
                        param7 = newValue;
                        break;
                    case 8:
                        param8 = newValue;
                        break;
                    case 9:
                        param9 = newValue;
                        break;
                    case 10:
                        param10 = newValue;
                        break;
                    case 11:
                        param11 = newValue;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"otherParams index i must be 0 <= i < 12; got {octantIndex}");
                }
            }
        };

        public int3 repCount;
        public int3 repCarve1;
        public int3 repCarve2;
        public float3 repRotation;
        public float3 repOffset;
        public int fixArtifacts;
    };
    
    public struct RayMarchingMaterial
    {
        public Vector3 diffuse;
        public float roughness;
        public float specular;
        public Vector3 specularColor;
        public int texFixed;
        public Vector3 texStretch;
    };

    public RayMarchingGroup GroupOfObjects;
    [HideInInspector]
    public List<Vector4> groupsTree;
    private List<Vector4> groupsTreeNew;
    [HideInInspector]
    public List<RaymarchinObjProps> RayMarchingObjects;

    [HideInInspector]
    public List<RayMarchingObject> objBuff;
    private List<RayMarchingObject> objBuffNew;
    [HideInInspector]
    public List<RayMarchingMaterial> matBuff;
    private List<RayMarchingMaterial> matBuffNew;
    [HideInInspector]
    public Texture2DArray texBuff;
    private Texture2DArray texBuffNew;
    [HideInInspector]
    public bool buffUpdated;
    [HideInInspector]
    public bool buffMaterialUpdated;
    [HideInInspector]
    public int nonNullObj;

    private void Start()
    {
        objBuff = new List<RayMarchingObject>(0);
        objBuffNew = new List<RayMarchingObject>(0);
        matBuff = new List<RayMarchingMaterial>(0);
        matBuffNew = new List<RayMarchingMaterial>(0);
        texBuff = new Texture2DArray(1,1,1,TextureFormat.RGB24,1,false, false);
        //texBuffNew = new Texture2DArray(512, 512, 1, TextureFormat.RGB24, 1, false, false);
        groupsTree = new List<Vector4>(0);
        groupsTreeNew = new List<Vector4>(0);
        buffUpdated = false;
        buffMaterialUpdated = false;
        nonNullObj = 0;
    }

    private void OnEnable()
    {
        objBuff = new List<RayMarchingObject>(0);
        objBuffNew = new List<RayMarchingObject>(0);
        matBuff = new List<RayMarchingMaterial>(0);
        matBuffNew = new List<RayMarchingMaterial>(0);
        groupsTree = new List<Vector4>(0);
        groupsTreeNew = new List<Vector4>(0);
        buffUpdated = false;
        buffMaterialUpdated = false;
        nonNullObj = 0;
    }

    Vector3 treeQueueMake(HierarchyNavigation t, int parent, ref int counter)
    {
        //Debug.Log(t.name);
        counter++;
        groupsTreeNew.Add(new Vector4(0, 0, 0, 0));
        int currIndex = counter;

        Vector4 res = new Vector4(0,0,0,0);
        res.z = parent;

        if (t.gameObject.GetComponent<RayMarchingGroup>() != null)
        {
            Vector3 meanPos = new Vector3(0.0f, 0.0f, 0.0f);
            List<HierarchyNavigation> childs = new List<HierarchyNavigation>();
            foreach (Transform child in t.transform)
            {
                if (child.gameObject.GetComponent<RaymarchinObjProps>() != null ||
                   (child.gameObject.GetComponent<RayMarchingGroup>() != null &&
                    child.childCount > 0))
                {
                    childs.Add(child.gameObject.GetComponent<HierarchyNavigation>());
                }

            }
            childs.Sort((x, y) => y.perfectSubtreeHeight - x.perfectSubtreeHeight);
            foreach (HierarchyNavigation child in childs)
            {
                res.y++;
                meanPos += treeQueueMake(child, currIndex, ref counter);
            }
        }

        res.x = counter;

        if (t.gameObject.TryGetComponent(out RaymarchinObjProps obj))
        {
            RayMarchingObjects.Add(obj);
            res.w = (RayMarchingObjects.Count - 1) + 0.1f;
            res.w *= (obj.IsNegative ? -1 : 1);
        }
        else if (t.gameObject.TryGetComponent(out RayMarchingGroup group))
        {
            if (RayMarchingGroup.funcIndicies[group.Function] <= 2) res.w = (RayMarchingGroup.funcIndicies[group.Function]);
            else res.w = (RayMarchingGroup.funcIndicies[group.Function]) + (group.Factor / 10.0f);
            res.w *= group.IsNegative ? -1 : 1;
        }
        groupsTreeNew[currIndex] = res;
        return t.transform.position;
    }
    void Update()
    {
        int counter = 0;
        RayMarchingObjects.Clear();
        groupsTreeNew.Clear();
        groupsTreeNew.Add(new Vector4(0, 0, 0, 0));
        if(GroupOfObjects != null)
            treeQueueMake(GroupOfObjects, 0, ref counter);

        objBuffNew.Clear();
        matBuffNew.Clear();
        texBuffNew = new Texture2DArray(512, 512, RayMarchingObjects.Count, TextureFormat.RGBA32, 1, false, false);
        nonNullObj = 0;
        for (int i = 0; i < RayMarchingObjects.Count; i++)
        {
            if (RayMarchingObjects[i] == null) continue;
            nonNullObj++;
            RaymarchinObjProps obj = RayMarchingObjects[i];
            RayMarchingObject robj;
            RayMarchingMaterial rmat;
            robj.group = 0;

            robj.position = obj.transform.position;
            robj.rotation = obj.transform.localEulerAngles * Mathf.Deg2Rad;
            robj.scale = obj.transform.localScale;

            robj.otherParams = new RayMarchingObject.otherParameters();

            switch (obj.Shape)
            {
                case "SPHERE":
                    robj.type = RaymarchinObjProps.shapesIndicies[obj.SubShapeSphere];
                    break;
                case "CUBE":
                    robj.type = RaymarchinObjProps.shapesIndicies["CUBE"];
                    robj.otherParams[0] = obj.CubeRounding;
                    robj.otherParams[1] = obj.FrameThiñkness;
                    break;
                case "CAPSULE":
                    robj.type = RaymarchinObjProps.shapesIndicies[obj.SubShapeCapsule];
                    switch (obj.SubShapeCapsule) {
                        case "CAPSULE_VERTICAL":
                            robj.otherParams[0] = obj.CapsuleRadius;
                            robj.otherParams[1] = obj.CapsuleHeight;
                            break;
                    }
                    break;
                case "CONE":
                    robj.type = RaymarchinObjProps.shapesIndicies[obj.SubShapeCone];
                    switch (obj.SubShapeCone)
                    {
                        case "CONE_ROUND_VERTICAL":
                            robj.otherParams[0] = obj.ConeBottomRadius;
                            robj.otherParams[1] = obj.ConeUpperRadius;
                            robj.otherParams[2] = obj.ConeHeight;
                            break;
                        case "CONE_CAPPED_VERTICAL":
                            robj.otherParams[0] = obj.ConeBottomRadius;
                            robj.otherParams[1] = obj.ConeUpperRadius;
                            robj.otherParams[2] = obj.ConeHeight;
                            break;
                    }
                    break;
                case "CYLINDER":
                    robj.type = RaymarchinObjProps.shapesIndicies[obj.SubShapeCylinder];
                    switch (obj.SubShapeCylinder)
                    {
                        case "CYLINDER_VERTICAL":
                            robj.otherParams[0] = obj.CylinderRadius;
                            robj.otherParams[1] = obj.CylinderHeight;
                            break;
                        case "CYLINDER_ROUND":
                            robj.otherParams[0] = obj.CylinderRadius;
                            robj.otherParams[1] = obj.CylinderRounding;
                            robj.otherParams[2] = obj.CylinderHeight;
                            break;
                    }
                    break;
                case "PLANE":
                    robj.type = RaymarchinObjProps.shapesIndicies[obj.SubShapePlane];
                    switch (obj.SubShapePlane)
                    {
                        case "PLANE":
/*                            robj.otherParams[0] = obj.n.x;
                            robj.otherParams[1] = obj.n.y;
                            robj.otherParams[2] = obj.n.z;
                            robj.otherParams[3] = obj.PlaneHeight;*/
                            break;
                    }
                    break;
                default:
                    robj.type = RaymarchinObjProps.shapesIndicies["SPHERE"];
                    break;
            }

            robj.repCount = new int3(obj.RepetitionCount.x, obj.RepetitionCount.y, obj.RepetitionCount.z);
            robj.repCarve1 = new int3(obj.RepetitionCarvePoint1.x, obj.RepetitionCarvePoint1.y, obj.RepetitionCarvePoint1.z);
            robj.repCarve2 = new int3(obj.RepetitionCarvePoint2.x, obj.RepetitionCarvePoint2.y, obj.RepetitionCarvePoint2.z);
            robj.repRotation = new float3((obj.RepetitionRotation.x + obj.transform.parent.eulerAngles.x) * Mathf.Deg2Rad,
                                          (obj.RepetitionRotation.y + obj.transform.parent.eulerAngles.y) * Mathf.Deg2Rad,
                                          (obj.RepetitionRotation.z + obj.transform.parent.eulerAngles.z) * Mathf.Deg2Rad);
            robj.repOffset = new float3(obj.RepetitionOffset.x, obj.RepetitionOffset.y, obj.RepetitionOffset.z);
            robj.fixArtifacts = obj.FixArtifacts ? 1 : 0;

            rmat.diffuse = new Vector3(obj.DiffuseColor.r,obj.DiffuseColor.g,obj.DiffuseColor.b);
            rmat.specular = obj.Specular;
            rmat.roughness = obj.Roughness;
            rmat.specularColor = new Vector3(obj.SpecularColor.r, obj.SpecularColor.g, obj.SpecularColor.b);
            rmat.texFixed = obj.TextureFixedSize ? 1 : 0;
            rmat.texStretch = obj.TextureStretch;

            if (obj.DiffuseTexture == null) texBuffNew.SetPixels(Enumerable.Repeat(Color.white, 512 * 512).ToArray(), matBuffNew.Count(), 0);
            else
            {
                Texture2D tempTex = new Texture2D(obj.DiffuseTexture.width, obj.DiffuseTexture.height, obj.DiffuseTexture.format, obj.DiffuseTexture.mipmapCount, false);
                Graphics.CopyTexture(obj.DiffuseTexture, tempTex);
                TextureScaler.scale(tempTex, 512, 512);
                //Color tempc = tempTex.GetPixels(0)[256 * 128 + 128];
                //Debug.Log(tempc);
                texBuffNew.SetPixelData(tempTex.GetPixelData<Color>(0), 0, matBuffNew.Count(), 0);
            }
            objBuffNew.Add(robj);

            matBuffNew.Add(rmat);

        }
        if(objBuffNew.Count == 0)
        {
            RayMarchingObject robj;
            RayMarchingMaterial rmat;
            robj.type = -1;
            robj.group = 0;
            robj.position = new float3(0.0);
            robj.rotation = new float3(0.0);
            robj.scale = new float3(0.0);
            robj.otherParams = new RayMarchingObject.otherParameters();

            robj.repCount = new int3(1);
            robj.repCarve1 = new int3(0);
            robj.repCarve2 = new int3(0);
            robj.repRotation = new int3(0);
            robj.repOffset = new int3(1);
            robj.fixArtifacts = 0;

            rmat.diffuse = Vector3.one;
            rmat.specular = 0;
            rmat.roughness = 1;
            rmat.specularColor = Vector3.one;
            rmat.texFixed = 1;
            rmat.texStretch = Vector3.one;


            //rmat.texture = null;
            objBuffNew.Add(robj);
            texBuffNew.SetPixels(Enumerable.Repeat(Color.white, 512 * 512).ToArray(), matBuffNew.Count(), 0);
            matBuffNew.Add(rmat);
        }

        if(GroupOfObjects == null || ((objBuffNew.Count != 0 && (objBuffNew.Count != objBuff.Count || !Enumerable.SequenceEqual(objBuffNew, objBuff))) ||
            (groupsTreeNew.Count != groupsTree.Count || !Enumerable.SequenceEqual(groupsTreeNew, groupsTree))))
        {
            objBuff.Clear();
            for (int i = 0; i < objBuffNew.Count; i++)
            {
                objBuff.Add(objBuffNew[i]);
            }

            groupsTree.Clear();
            for (int i = 0; i < groupsTreeNew.Count; i++)
            {
                groupsTree.Add(groupsTreeNew[i]);
            }

            buffUpdated = true;
        }

        if(matBuffNew.Count != matBuff.Count || !Enumerable.SequenceEqual(matBuffNew, matBuff) ||
            texBuffNew.imageContentsHash != texBuff.imageContentsHash)
        {
            matBuff.Clear();
            foreach(RayMarchingMaterial m in matBuffNew)
            {
                matBuff.Add(m);
            }
            texBuff = new Texture2DArray(512, 512, RayMarchingObjects.Count, TextureFormat.RGBA32, 1, false, false);
            Graphics.CopyTexture(texBuffNew, texBuff);
            buffMaterialUpdated = true;
        }

    }

}
