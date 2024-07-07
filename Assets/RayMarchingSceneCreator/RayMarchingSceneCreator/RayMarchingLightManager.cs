using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RayMarchingObjectManager;

[ExecuteInEditMode]
public class RayMarchingLightManager : MonoBehaviour
{
    public const int RayMarchingLightByteSize = sizeof(float) * (1 + 3 + 3 + 1 + 1 + 4 + 1);
    public struct RayMarchingLight
    {
        public int type;
        public Vector3 position;
        public Vector3 direction;
        public float range;
        public float spotAngle;
        public Color color;
        public float intensity;
    };

    public List<Light> Lights = new List<Light>(1);

    [Header("Shadows")]

    public float ShadowsMinDistance = 0.002f;
    public float ShadowsMaxDistance = 100.0f;
    public float ShadowsMinStep = 0.05f;
    public float ShadowsMaxStep = 0.5f;
    public float ShadowsIterations = 50.0f;
    public float ShadowsSoftness = 0.15f;

    [Header("Ambient Occlusion")]

    public float AmbientOcclusionIterations = 20.0f;
    public float AmbientOcclusionIntensity = 0.02f;
    public float AmbientOcclusionStep = 0.05f;


    [HideInInspector]
    public List<RayMarchingLight> lightBuff;
    private List<RayMarchingLight> lightBuffNew;

    [HideInInspector]
    public bool lightBuffUpdated;



    void Start()
    {
        lightBuff = new List<RayMarchingLight>(0);
        lightBuffNew = new List<RayMarchingLight>(0);
        lightBuffUpdated = false;
    }

    private void OnEnable()
    {
        lightBuff = new List<RayMarchingLight>(0);
        lightBuffNew = new List<RayMarchingLight>(0);
        lightBuffUpdated = false;
    }

    void Update()
    {
        lightBuffNew.Clear();
        foreach (Light lit in Lights)
        {
            if (lit == null) continue;
            RayMarchingLight rlit;
            rlit.type = (int)lit.type;
            rlit.position = lit.transform.position;
            rlit.direction = lit.transform.forward;
            rlit.range = lit.range;
            rlit.spotAngle = lit.spotAngle * Mathf.Deg2Rad;
            rlit.color = lit.color;
            rlit.intensity = lit.intensity;
            lightBuffNew.Add(rlit);
        }
        if (lightBuffNew.Count == 0)
        {
            RayMarchingLight rlit;
            rlit.type = (int)LightType.Directional;
            rlit.position = Vector3.zero;
            rlit.direction = Vector3.down;
            rlit.range = 0;
            rlit.spotAngle = 0;
            rlit.color = Color.white;
            rlit.intensity = 1;
            lightBuffNew.Add(rlit);
        }

        if (lightBuffNew.Count != lightBuff.Count || !Enumerable.SequenceEqual(lightBuffNew, lightBuff))
        {
            lightBuff.Clear();
            foreach (RayMarchingLight rlit in lightBuffNew)
            {
                lightBuff.Add(rlit);
            }
            lightBuffUpdated = true;
        }
    }
}
