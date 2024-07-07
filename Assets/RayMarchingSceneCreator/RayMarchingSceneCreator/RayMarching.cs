using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static RayMarchingObjectManager;
using Unity.Mathematics;
using UnityEngine.Assertions.Must;
using System.Runtime.Serialization;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Raymarch (Generic)")] 
public class RayMarching : SceneViewFilter {

    public int ITERATIONS = 500;

    private float p1;
    private float p2;
    private float p3;
    private float p4;
    private float p5;

    public RayMarchingObjectManager ObjectManager;
    public RayMarchingLightManager LightManager;

    [SerializeField]
    private ComputeShader rayMarchingShader;
    private Texture depthTexture;

    private ComputeBuffer computeBuffer;

    //private int frame = 0;

    /*public Material rayMarchingMaterial
    {
        get
        {
            if (!_rayMarchingMaterial && rayMarchingShader)
            {
                _rayMarchingMaterial = new Material(rayMarchingShader);
                _rayMarchingMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return _rayMarchingMaterial;
        }
    }
    private Material _rayMarchingMaterial;*/

    public Camera CurrentCamera
    {
        get
        {
            if (!_CurrentCamera) _CurrentCamera = GetComponent<Camera>();
            return _CurrentCamera;
        }
    }
    private Camera _CurrentCamera;

    private Matrix4x4 GetFrustumCorners(Camera cam)
    {
        float camFov = cam.fieldOfView;
        //cam.ster
        float camAspect = cam.aspect;
        Matrix4x4 frustumCorners = Matrix4x4.identity;
        float fovWHalf = camFov * 0.5f;
        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
        Vector3 toRight = Vector3.right * tan_fov * camAspect;
        Vector3 toTop = Vector3.up * tan_fov;
        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);
        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);
        //Vector4 focal = (frustumCorners.GetRow(0) + frustumCorners.GetRow(1) + frustumCorners.GetRow(2) + frustumCorners.GetRow(3)) / 4.0f;
        //Debug.Log(focal.magnitude);
        return frustumCorners;
    }

    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        //RenderTexture rte = RenderTexture.GetTemporary(source.descriptor);
        Texture2D te = Texture2D.redTexture;
        //Graphics.CopyTexture(source, 0, rte, 0);
        RenderTexture.active = dest;
        fxMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        fxMaterial.SetPass(passNr);
        GL.Begin(GL.QUADS);
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        //GL.Vertex3(0.0f, 0.0f, 3.0f); // BL
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        //GL.Vertex3(1.0f, 0.0f, 2.0f); // BR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        //GL.Vertex3(1.0f, 1.0f, 1.0f); // TR
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        //GL.Vertex3(0.0f, 1.0f, 0.0f); // TL
        GL.End();
        GL.PopMatrix();
    }

/*    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, ComputeShader cshader, int passNr)
    {
        RenderTexture.active = dest;
        fxMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        fxMaterial.SetPass(passNr);
        GL.Begin(GL.QUADS);
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL
        GL.End();
        GL.PopMatrix();
    }
*/
    internal static class ExampleUtil
    {
        public static bool isPresent()
        {
            var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
            foreach (var xrDisplay in xrDisplaySubsystems)
            {
                if (xrDisplay.running)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /*    List<float> objBuff;
        private void Start()
        {
            objBuff = new List<float> { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };
        }*/

    /*    private void OnEnable()
        {
            objBuff = new List<float>();
        }*/

    [HideInInspector]
    public RenderTexture renderTex;
    private void Start()
    {
        //RenderTexture renderTex = new RenderTexture(CurrentCamera.pixelWidth, CurrentCamera.pixelHeight, 24);
        //renderTex.enableRandomWrite = true;
        //renderTex.Create();
    }
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
/*        if (ExampleUtil.isPresent())
        {

            Vector3 left = Quaternion.Inverse(InputTracking.GetLocalRotation(XRNode.LeftEye)) * InputTracking.GetLocalPosition(XRNode.LeftEye);

            Vector3 right = Quaternion.Inverse(InputTracking.GetLocalRotation(XRNode.RightEye)) * InputTracking.GetLocalPosition(XRNode.RightEye);

            Vector3 offset = (left - right) * 0.5f;

            Matrix4x4 m = Camera.main.cameraToWorldMatrix;
            //offset *= 5.0f;
            Vector3 leftWorld = m.MultiplyPoint(-offset);
            Vector3 rightWorld = m.MultiplyPoint(offset);

            //Vector3 leftWorld = m.MultiplyPoint(left);
            //Vector3 rightWorld = m.MultiplyPoint(right);

            //Debug.Log(offset);
            //Debug.Log(leftWorld);
            //Debug.Log(rightWorld);
            rayMarchingShader.SetInt("is_VR", 1);
            rayMarchingShader.SetVector("LeftEye", leftWorld);
            rayMarchingShader.SetVector("RightEye", rightWorld);
            rayMarchingShader.SetMatrix("_LeftEyeInvViewMatrix", CurrentCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse);
            rayMarchingShader.SetMatrix("_RightEyeInvViewMatrix", CurrentCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse);
            rayMarchingShader.SetMatrix("_LeftEyeInvProjectionMatrix", CurrentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left).inverse);
            rayMarchingShader.SetMatrix("_RightEyeInvProjectionMatrix", CurrentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse);
        }
        else
        {
            rayMarchingShader.SetInt("is_VR", 0);
        }*/
        rayMarchingShader.SetInt("is_VR", 0);
        float quadDist = 0.5f / Mathf.Tan((CurrentCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad);
        float hFOV = Mathf.Atan((CurrentCamera.aspect * 0.5f) / quadDist) * Mathf.Rad2Deg * 2.0f;
        rayMarchingShader.SetVector("_CameraFoward", CurrentCamera.transform.forward);
        Vector3[] frustumCorners = new Vector3[4];
        CurrentCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), CurrentCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        Matrix4x4 frustumCornersMat = new Matrix4x4();
        frustumCornersMat.SetRow(0, Vector3.Normalize(CurrentCamera.transform.TransformVector(frustumCorners[1])));
        frustumCornersMat.SetRow(1, Vector3.Normalize(CurrentCamera.transform.TransformVector(frustumCorners[0])));
        frustumCornersMat.SetRow(2, Vector3.Normalize(CurrentCamera.transform.TransformVector(frustumCorners[3])));
        frustumCornersMat.SetRow(3, Vector3.Normalize(CurrentCamera.transform.TransformVector(frustumCorners[2])));
        rayMarchingShader.SetMatrix("_FrustumCornersES", frustumCornersMat);
        //rayMarchingShader.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix);
        //rayMarchingShader.SetMatrix("_CameraInvProjectionMatrix", CurrentCamera.projectionMatrix.inverse);
        rayMarchingShader.SetVector("_CameraWS", CurrentCamera.transform.position);
        rayMarchingShader.SetInt("ITERATIONS", ITERATIONS);
        //rayMarchingShader.SetInt("TETRAHEDRON_ITERATIONS", FRACTAL_ITERATIONS);
        //Vector4[] poos = { Sierpinski.transform.position };

        bool isReloaded = false;
        try
        {
            float[] getBuff = new float[1];
            computeBuffer.GetData(getBuff);
        }
        catch(System.Exception e)
        {
            isReloaded = true;
        }
        if (ObjectManager.buffUpdated || ObjectManager.groupsTree.Count == 1 || isReloaded)
        {
            computeBuffer = new ComputeBuffer(ObjectManager.objBuff.Count, RayMarchingObjectByteSize);
            computeBuffer.SetData(ObjectManager.objBuff.ToArray());
            rayMarchingShader.SetBuffer(rayMarchingShader.FindKernel("CSMain"), "Objects", computeBuffer);

            rayMarchingShader.SetInt("nrOfObjects", ObjectManager.nonNullObj);

            rayMarchingShader.SetInt("groupsTreeSize", ObjectManager.groupsTree.Count);
            rayMarchingShader.SetVectorArray("groupsTree", ObjectManager.groupsTree.ToArray());

            ObjectManager.buffUpdated = false;
        }

        if(ObjectManager.buffMaterialUpdated || isReloaded)
        {
            computeBuffer = new ComputeBuffer(ObjectManager.matBuff.Count, RayMarchingMaterialByteSize);
            computeBuffer.SetData(ObjectManager.matBuff.ToArray());
            rayMarchingShader.SetBuffer(rayMarchingShader.FindKernel("CSMain"), "Materials", computeBuffer);

            ObjectManager.texBuff.Apply();
            Color tempc = ObjectManager.texBuff.GetPixels(0, 0)[0];
            //Debug.Log(tempc);
            rayMarchingShader.SetTexture(rayMarchingShader.FindKernel("CSMain"), "textures", ObjectManager.texBuff);


            ObjectManager.buffMaterialUpdated = false;
        }

        if (LightManager.lightBuffUpdated || isReloaded)
        {
            computeBuffer = new ComputeBuffer(LightManager.lightBuff.Count, RayMarchingLightManager.RayMarchingLightByteSize);
            computeBuffer.SetData(LightManager.lightBuff.ToArray());
            rayMarchingShader.SetBuffer(rayMarchingShader.FindKernel("CSMain"), "Lights", computeBuffer);

            rayMarchingShader.SetInt("nrOfLights", LightManager.lightBuff.Count);
            rayMarchingShader.SetInt("lightsOffset", RayMarchingLightManager.RayMarchingLightByteSize);

            LightManager.lightBuffUpdated = false;
        }

        rayMarchingShader.SetFloat("p1", p1);
        rayMarchingShader.SetFloat("p2", p2);
        rayMarchingShader.SetFloat("p3", p3);
        rayMarchingShader.SetFloat("p4", p4);
        rayMarchingShader.SetFloat("p5", p5);

        rayMarchingShader.SetFloat("_ShadowsMinDistance", LightManager.ShadowsMinDistance);
        rayMarchingShader.SetFloat("_ShadowsMaxDistance", LightManager.ShadowsMaxDistance);
        rayMarchingShader.SetFloat("_ShadowsIterations", LightManager.ShadowsIterations);
        rayMarchingShader.SetFloat("_ShadowsMinStep", LightManager.ShadowsMinStep);
        rayMarchingShader.SetFloat("_ShadowsMaxStep", LightManager.ShadowsMaxStep);
        rayMarchingShader.SetFloat("_ShadowsSoftness", LightManager.ShadowsSoftness);
                                                                      
        rayMarchingShader.SetFloat("_AmbientOcclusionIntensity", LightManager.AmbientOcclusionIntensity);
        rayMarchingShader.SetFloat("_AmbientOcclusionIterations", LightManager.AmbientOcclusionIterations);
        rayMarchingShader.SetFloat("_AmbientOcclusionStep", LightManager.AmbientOcclusionStep);

        //rayMarchingShader.SetFloats("inputObjectBuf", ObjectManager.objBuff.ToArray());
        //rayMarchingMaterial.Set
        //rayMarchingShader.SetVectorArray("_Sierpinski_p", poos);
        //rayMarchingMaterial.SetVector("_Sierpinski_p", Sierpinski.transform.position);
        //rayMarchingShader.SetVector("_Sierpinski_r", Sierpinski.transform.rotation.eulerAngles * Mathf.Deg2Rad);
        //rayMarchingShader.SetVector("_Sierpinski_s", Sierpinski.transform.localScale);
        //rayMarchingShader.SetVector("_Sierpinski_fr1", Sierpinski.GetComponent<Fractal_properties>().beforeStepRotation);
        //rayMarchingShader.SetVector("_Sierpinski_fr2", Sierpinski.GetComponent<Fractal_properties>().afterStepRotation);
        //rayMarchingShader.SetFloat("_Sierpinski_fs", Sierpinski.GetComponent<Fractal_properties>().stepScale);

        depthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
        //depthTexture.u;
        //Color tempc = depthTexture.G  GetPixels(0)[256 * 128 + 128];
        //Debug.Log(tempc);
        rayMarchingShader.SetTexture(rayMarchingShader.FindKernel("CSMain"), "DepthData", depthTexture);

        rayMarchingShader.SetTexture(rayMarchingShader.FindKernel("CSMain"), "ScreenData", source);
        //CustomGraphicsBlit(source, destination, rayMarchingShader, 0);
        if (renderTex == null || source.width != renderTex.width || source.height != renderTex.height)
        {
            renderTex = new RenderTexture(source);
            renderTex.enableRandomWrite = true;
            //renderTex.antiAliasing = 2;
            renderTex.Create();
            //renderTex.ant
        }
        rayMarchingShader.SetVector("Resolution", new Vector2(renderTex.width, renderTex.height));
        rayMarchingShader.SetTexture(rayMarchingShader.FindKernel("CSMain"), "Result", renderTex);
        rayMarchingShader.Dispatch(rayMarchingShader.FindKernel("CSMain"), renderTex.width / 8, renderTex.height / 8, 1);
        Graphics.Blit(renderTex, destination);
    }

/*    private void OnDisable()
    {
        delete objBuff;
    }*/

}