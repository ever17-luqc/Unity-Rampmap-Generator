using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class RampmapGenerator : EditorWindow
{
    [MenuItem("CelTools/Character/RampmapGenerator", false, 1000)]
    public static void window()
    {
        Rect rect = new Rect(Screen.width / 2, Screen.height / 2, 500, 600);
        RampmapGenerator window = (RampmapGenerator)EditorWindow.GetWindow(typeof(RampmapGenerator), true, "RampmapGenerator");
        window.position = rect;

    }
    enum TexResolutions
    {
        [InspectorName("128")]
        Tex_128 = 0,
        [InspectorName("256")]
        Tex_256 = 1,
        [InspectorName("512")]
        Tex_512 = 2,
        [InspectorName("1024")]
        Tex_1024 = 3,
        [InspectorName("2048")]
        Tex_2048 = 4,


    }

    List<Vector4> m_ColorAndPos = new List<Vector4>();
    string savePath = "";
    string savePath_Preset = "";
    string saveName = "";
    Gradient m_Gradient = new Gradient();
    AnimationCurve m_CurveXR = new AnimationCurve();
    AnimationCurve m_CurveYR = new AnimationCurve();
    AnimationCurve m_CurveNSR = new AnimationCurve();
    AnimationCurve m_CurveXG = new AnimationCurve();
    AnimationCurve m_CurveYG = new AnimationCurve();
    AnimationCurve m_CurveNSG = new AnimationCurve();
    AnimationCurve m_CurveXB = new AnimationCurve();
    AnimationCurve m_CurveYB = new AnimationCurve();
    AnimationCurve m_CurveNSB = new AnimationCurve();
    AnimationCurve m_CurveXA = new AnimationCurve();
    AnimationCurve m_CurveYA = new AnimationCurve();
    AnimationCurve m_CurveNSA = new AnimationCurve();
    TexResolutions m_Resolution = 0;

    public ComputeShader RampmapCompute;
    public RenderTexture m_RenderTexture;
    public RampPreset RampPreset;
    private void OnDestroy()
    {
        if (null != m_RenderTexture)
        {
            m_RenderTexture.Release();



        }
        if(m_Curve1Tex!=null)
        {
            DestroyImmediate(m_Curve1Tex);
        }

        if (m_Curve2Tex != null)
        {
            DestroyImmediate(m_Curve2Tex);
        }
        if(RampTex!=null)
        {
            DestroyImmediate(RampTex);
        }

    }
    void DrawRamp1D(Gradient gradient, TexResolutions resolution,bool sRGB=false)
    {
        int _resolution = 128 * (int)Mathf.Pow(2, (int)resolution);
       


        m_ColorAndPos.Clear();
        for (int index = 0; index < gradient.colorKeys.Length; index++)
        {
            Color evaluateCol = gradient.colorKeys[index].color;

            m_ColorAndPos.Add(new Vector4(evaluateCol.r, evaluateCol.g, evaluateCol.b, gradient.colorKeys[index].time));
          
        }

        int compute1DIndex = RampmapCompute.FindKernel("RampGen1D");
        RampmapCompute.SetVectorArray("_ColorAndPosArr", m_ColorAndPos.ToArray());
        RampmapCompute.SetFloat("_ColorAndPosArrLen", m_ColorAndPos.Count);
        RampmapCompute.SetFloat("_Resolution", _resolution);
        if(PlayerSettings.colorSpace==ColorSpace.Linear)
        {
            if (sRGB)
            {
                RampmapCompute.EnableKeyword("_SRGB");
            }
            else
            {
                RampmapCompute.DisableKeyword("_SRGB");
            }
        }
        else
        {
            RampmapCompute.EnableKeyword("_SRGB");
        }
       
        RampmapCompute.SetTexture(compute1DIndex,"Result", m_RenderTexture);
        RampmapCompute.Dispatch(compute1DIndex, _resolution / 8, _resolution / 8, 1);

        



    }
    Texture2D GetCurveTex(Texture2D tex,AnimationCurve curve,int Mode)
    {
        for(int i=0;i<512;++i)
        {
            float val = curve.Evaluate(i / 512.0f);
            switch(Mode)
            {
                case 0:
                    {
                       
                        Color oc = tex.GetPixel(i, 0, 0);
                        Color c = new Color(val, oc.g, oc.b, oc.a);
                        tex.SetPixel(i, 0, c);
                        break;
                    }
                case 1:
                    {
                        Color oc = tex.GetPixel(i, 0, 0);
                        Color c = new Color(oc.r, val, oc.b, oc.a);
                        tex.SetPixel(i, 0, c);
                        break;
                    }
                case 2:
                    {
                        Color oc = tex.GetPixel(i, 0, 0);
                        Color c = new Color(oc.r, oc.g, val, oc.a);
                        tex.SetPixel(i, 0, c);
                        break;
                    }
                case 3:
                    {
                        Color oc = tex.GetPixel(i, 0, 0);
                        Color c = new Color(oc.r, oc.g, oc.b, val);
                        tex.SetPixel(i, 0, c);
                        break;
                    }
            }
           
        }
        tex.Apply();
        return tex;
    }
    void DrawRamp2D(TexResolutions resolution,int index,bool sRGB=false)
    {
        int _resolution = 128 * (int)Mathf.Pow(2, (int)resolution);


        int compute2DIndex = RampmapCompute.FindKernel("RampGen2D");
        RampmapCompute.SetFloat("_Resolution", _resolution);
        RampmapCompute.SetTexture(compute2DIndex, "Result", m_RenderTexture);
        RampmapCompute.SetTexture(compute2DIndex, "_Curve1", m_Curve1Tex); 
        RampmapCompute.SetTexture(compute2DIndex, "_Curve2", m_Curve2Tex);
        RampmapCompute.SetTexture(compute2DIndex, "_CurveNS", m_CurveNSTex);
        //RampmapCompute.SetVector("_NegativeShape", NegativeShapeFin);
        RampmapCompute.SetFloat("_Moede", index);

        if (sRGB)
        {
            RampmapCompute.EnableKeyword("_SRGB");
        }
        else
        {
            RampmapCompute.DisableKeyword("_SRGB");
        }
       
        RampmapCompute.Dispatch(compute2DIndex, _resolution / 8, _resolution / 8, 1);


      


    }
    private string[] buttonNames = new string[] { "一维", "二维" };
    private string[] buttonRGBNames = new string[] { "R", "G","B","A","Result" };
    public int index;
    public int rgbIndex;
    private Texture2D m_Curve1Tex;
    private Texture2D m_Curve2Tex;
    private Texture2D m_CurveNSTex;


    //public float NegativeShapeR = 0.5f;
    //public float NegativeShapeG = 0.5f;
    //public float NegativeShapeB = 0.5f;
    //public float NegativeShapeA = 0.5f;
    //public Vector4 NegativeShapeFin = new Vector4(0,0,0,0);
    public Texture2D RampTex;
    //void DrawNegShapeSlider(int index)
    //{
    //    switch (index)
    //    {
    //        case 0:
    //            {
                    

    //                NegativeShapeR = EditorGUILayout.Slider("Negative Shape", NegativeShapeR, 0, 1, new GUILayoutOption[] { GUILayout.Width(300) });
                   
    //                break;
    //            }
    //        case 1:
    //            {
    //                NegativeShapeG = EditorGUILayout.Slider("Negative Shape", NegativeShapeG, 0, 1, new GUILayoutOption[] { GUILayout.Width(300) });
    //                break;
    //            }
    //        case 2:
    //            {
    //                NegativeShapeB = EditorGUILayout.Slider("Negative Shape", NegativeShapeB, 0, 1, new GUILayoutOption[] { GUILayout.Width(300) });
    //                break;
    //            }
    //        case 3:
    //            {
    //                NegativeShapeA = EditorGUILayout.Slider("Negative Shape", NegativeShapeA, 0, 1, new GUILayoutOption[] { GUILayout.Width(300) });
    //                break;
    //            }
    //    }
    //}
    //float GetNegShape(int index)
    //{
    //    switch (index)
    //    {
    //        case 0:
    //            {

    //                return NegativeShapeR;


                   
    //            }
    //        case 1:
    //            {
    //                return NegativeShapeG ;
                   
    //            }
    //        case 2:
    //            {
    //                return NegativeShapeB;
                   
    //            }
    //        case 3:
    //            {
    //                return NegativeShapeA;
                    
    //            }
    //        default:
    //            {
    //                return 0;
    //            }
    //    }

    //}

    void SaveTex()
    {
        int res = 128 * (int)Mathf.Pow(2, (int)m_Resolution);
        if(index==1)
        {
            DrawRamp2D(m_Resolution, 4,true); 
        }
        else
        {
            DrawRamp1D(m_Gradient, m_Resolution, true);
        }
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = m_RenderTexture;
        if (null == RampTex)
        {
            RampTex = new Texture2D(res, res, TextureFormat.RGBAFloat, true);
        }
        else
        {
            DestroyImmediate(RampTex);
            RampTex = new Texture2D(res, res, TextureFormat.RGBAFloat, true);
        }
        RampTex.ReadPixels(new Rect(0, 0, res, res), 0, 0);
        RampTex.Apply();
        RenderTexture.active = old;
      
        byte[] bytes = RampTex.EncodeToTGA();
        string defaultPath = Application.dataPath;
        savePath = EditorUtility.SaveFilePanel("保存到", defaultPath, saveName, "tga");
        //Debug.Log(savePath);
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.Refresh();
        string savePath_Rel = savePath.Substring(savePath.IndexOf("Assets"));
        //Debug.Log(str);
        TextureImporter TI = (TextureImporter)TextureImporter.GetAtPath(savePath_Rel);
        //贴图设置往下面加==========================
        TI.wrapMode = TextureWrapMode.Clamp;
        //===========================================

        RampTex.Apply();



        AssetDatabase.Refresh();
    }
    private void OnGUI()
    {
        if(null== m_Curve1Tex)
        {
            m_Curve1Tex = new Texture2D(512, 1, TextureFormat.ARGB32, false,true);

        }
        if (null == m_Curve2Tex)
        {
            m_Curve2Tex  = new Texture2D(512, 1, TextureFormat.ARGB32, false, true);

        }
        if(null==m_CurveNSTex)
        {
            m_CurveNSTex = new Texture2D(512, 1, TextureFormat.ARGB32, false, true);
        }

        
        if (null == RampmapCompute)
        {
            RampmapCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Editor/RampmapGenerator/RampmapCompute.compute");
            if (null == RampmapCompute)
            {
                Debug.Log("ComputeShader未配置正确");
                return;
            }

        }
        //RT =====================
        if(null== m_RenderTexture)
        {
            m_RenderTexture = RenderTexture.GetTemporary(128 * (int)Mathf.Pow(2, (int)m_Resolution), 128 * (int)Mathf.Pow(2, (int)m_Resolution), 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
            m_RenderTexture.enableRandomWrite = true;

            m_RenderTexture.Create();
           
        }

        // ======================

        index = GUILayout.Toolbar(index, buttonNames);

        m_Resolution = (TexResolutions)EditorGUILayout.EnumPopup("RampMap分辨率", m_Resolution);
        saveName = EditorGUILayout.TextField("输入保存贴图的名字", saveName);
 
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存贴图"))
            {
                SaveTex();
            }
            GUILayout.Space(40);
            if (GUILayout.Button("保存预设"))
            {
                SavePrset();
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUI.BeginChangeCheck();
        RampPreset = (RampPreset)EditorGUILayout.ObjectField("ramp预设[可选]", RampPreset, typeof(RampPreset), false);
        if (EditorGUI.EndChangeCheck()&& null!=RampPreset)
        {
            SetPreset();
        }
        GUI.color = new Color(0.5f, 0.5f, 0.5f);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        GUI.color = Color.white;
        GUILayout.Space(20);
        if (index==0)
        {
            GUILayoutOption[] gradientOptions = new GUILayoutOption[] { GUILayout.Width(400), GUILayout.Height(50) };
            m_Gradient = EditorGUILayout.GradientField("选择颜色映射", m_Gradient, gradientOptions);
            
            DrawRamp1D(m_Gradient, m_Resolution);
            
            GUILayout.Label( "Ramp Preview:");
            Rect rect = EditorGUILayout.GetControlRect();
         
            rect = new Rect(rect.x+50, rect.y+10, 400, 200);
   
            EditorGUI.DrawPreviewTexture(rect,m_RenderTexture);
            

        }
        else
        {
            if (m_CurveXR.keys.Length == 0)
            {

                m_CurveXR = AnimationCurve.Linear(0, 0, 1, 1);
                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXR, rgbIndex);
            }
            if (m_CurveXG.keys.Length == 0)
            {

                m_CurveXG = AnimationCurve.Linear(0, 0, 1, 1);
                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXG, rgbIndex);
            }
            if (m_CurveXB.keys.Length == 0)
            {

                m_CurveXB = AnimationCurve.Linear(0, 0, 1, 1);
                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXB, rgbIndex);
            }
            if (m_CurveXA.keys.Length == 0)
            {

                m_CurveXA = AnimationCurve.Linear(0, 0, 1, 1);
                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXA, rgbIndex);
            }
            if (m_CurveYR.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 1);
                Keyframe key2 = new Keyframe(1, 1);
                keys = new Keyframe[] { key1, key2 };
                m_CurveYR = new AnimationCurve(keys);
                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYR, rgbIndex);
            }
            if (m_CurveYG.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 1);
                Keyframe key2 = new Keyframe(1, 1);
                keys = new Keyframe[] { key1, key2 };
                m_CurveYG = new AnimationCurve(keys);
                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYG, rgbIndex);
            }
            if (m_CurveYB.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 1);
                Keyframe key2 = new Keyframe(1, 1);
                keys = new Keyframe[] { key1, key2 };
                m_CurveYB = new AnimationCurve(keys);
                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYB, rgbIndex);
            }
            if (m_CurveYA.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 1);
                Keyframe key2 = new Keyframe(1, 1);
                keys = new Keyframe[] { key1, key2 };
                m_CurveYA = new AnimationCurve(keys);
                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYA, rgbIndex);
            }
            if (m_CurveNSR.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 0.5f);
                Keyframe key2 = new Keyframe(1, 0.5f);
                keys = new Keyframe[] { key1, key2 };
                m_CurveNSR = new AnimationCurve(keys);
                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSR, rgbIndex);
            }
            if (m_CurveNSG.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 0.5f);
                Keyframe key2 = new Keyframe(1, 0.5f);
                keys = new Keyframe[] { key1, key2 };
                m_CurveNSG = new AnimationCurve(keys);
                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSG, rgbIndex);
            }
            if (m_CurveNSB.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 0.5f);
                Keyframe key2 = new Keyframe(1, 0.5f);
                keys = new Keyframe[] { key1, key2 };
                m_CurveNSB = new AnimationCurve(keys);
                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSB, rgbIndex);
            }
            if (m_CurveNSA.keys.Length == 0)
            {
                Keyframe[] keys;
                Keyframe key1 = new Keyframe(0, 0.5f);
                Keyframe key2 = new Keyframe(1, 0.5f);
                keys = new Keyframe[] { key1, key2 };
                m_CurveNSA = new AnimationCurve(keys);
                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSA, rgbIndex);
            }

            GUILayout.Space(20);
            EditorGUI.BeginChangeCheck();
            rgbIndex = GUILayout.Toolbar(rgbIndex, buttonRGBNames);
            if (EditorGUI.EndChangeCheck())
            {
                switch(rgbIndex)
                {
                    case 0:
                        {
                            m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXR, rgbIndex);
                            m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYR, rgbIndex);
                            m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSR, rgbIndex);
                            break;
                        }
                    case 1:
                        {
                            m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXG, rgbIndex);
                            m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYG, rgbIndex);
                            m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSG, rgbIndex);
                            break;
                        }
                    case 2:
                        {
                            m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXB, rgbIndex);
                            m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYB, rgbIndex);
                            m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSB, rgbIndex);
                            break;
                        }
                    case 3:
                        {
                            m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXA, rgbIndex);
                            m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYA, rgbIndex);
                            m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSA, rgbIndex);
                            break;
                        }
                }
                
            }


                GUILayoutOption[] curveOptions = new GUILayoutOption[] { GUILayout.Width(400), GUILayout.Height(50) };
           
            if(rgbIndex!=4)
            {
                EditorGUI.BeginChangeCheck();
                switch (rgbIndex)
                {
                    case 0:
                        {
                            m_CurveXR = EditorGUILayout.CurveField("基础映射曲线", m_CurveXR, curveOptions);
                            break;
                        }
                    case 1:
                        {
                            m_CurveXG = EditorGUILayout.CurveField("基础映射曲线", m_CurveXG, curveOptions);
                            break;
                        }
                    case 2:
                        {
                            m_CurveXB = EditorGUILayout.CurveField("基础映射曲线", m_CurveXB, curveOptions);
                            break;
                        }
                    case 3:
                        {
                            m_CurveXA = EditorGUILayout.CurveField("基础映射曲线", m_CurveXA, curveOptions);
                            break;
                        }
                }
               
                if (EditorGUI.EndChangeCheck())
                {
                    switch (rgbIndex)
                    {
                        case 0:
                            {
                                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXR, rgbIndex);
                                break;
                            }
                        case 1:
                            {
                                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXG, rgbIndex);
                                break;
                            }
                        case 2:
                            {
                                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXB, rgbIndex);
                                break;
                            }
                        case 3:
                            {
                                m_Curve1Tex = GetCurveTex(m_Curve1Tex, m_CurveXA, rgbIndex);
                                break;
                            }
                    }
                   
                }
                EditorGUI.BeginChangeCheck();
                switch (rgbIndex)
                {
                    case 0:
                        {
                            m_CurveYR = EditorGUILayout.CurveField("虚实映射曲线", m_CurveYR, curveOptions);
                            break;
                        }
                    case 1:
                        {
                            m_CurveYG = EditorGUILayout.CurveField("虚实映射曲线", m_CurveYG, curveOptions);
                            break;
                        }
                    case 2:
                        {
                            m_CurveYB = EditorGUILayout.CurveField("虚实映射曲线", m_CurveYB, curveOptions);
                            break;
                        }
                    case 3:
                        {
                            m_CurveYA= EditorGUILayout.CurveField("虚实映射曲线", m_CurveYA, curveOptions);
                            break;
                        }
                }
             
                if (EditorGUI.EndChangeCheck())
                {
                    switch (rgbIndex)
                    {
                        case 0:
                            {
                                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYR, rgbIndex);
                                break;
                            }
                        case 1:
                            {
                                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYG, rgbIndex);
                                break;
                            }
                        case 2:
                            {
                                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYB, rgbIndex);
                                break;
                            }
                        case 3:
                            {
                                m_Curve2Tex = GetCurveTex(m_Curve2Tex, m_CurveYA, rgbIndex);
                                break;
                            }
                    }
                    
                }
                //DrawNegShapeSlider(rgbIndex);
                EditorGUI.BeginChangeCheck();
                switch (rgbIndex)
                {
                    case 0:
                        {
                            m_CurveNSR = EditorGUILayout.CurveField("负形", m_CurveNSR, curveOptions);
                            break;
                        }
                    case 1:
                        {
                            m_CurveNSG= EditorGUILayout.CurveField("负形", m_CurveNSG, curveOptions);
                            break;
                        }
                    case 2:
                        {
                            m_CurveNSB = EditorGUILayout.CurveField("负形", m_CurveNSB, curveOptions);
                            break;
                        }
                    case 3:
                        {
                            m_CurveNSA = EditorGUILayout.CurveField("负形", m_CurveNSA, curveOptions);
                            break;
                        }
                }
               
                if (EditorGUI.EndChangeCheck())
                {
                    switch (rgbIndex)
                    {
                        case 0:
                            {
                                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSR, rgbIndex);
                                break;
                            }
                        case 1:
                            {
                                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSG, rgbIndex);
                                break;
                            }
                        case 2:
                            {
                                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSB, rgbIndex);
                                break;
                            }
                        case 3:
                            {
                                m_CurveNSTex = GetCurveTex(m_CurveNSTex, m_CurveNSA, rgbIndex);
                                break;
                            }
                    }
                    
                }
            }
            //float NegativeShape = GetNegShape(rgbIndex);
            DrawRamp2D(m_Resolution, rgbIndex);
            GUILayout.Space(10);
           
            GUILayout.Space(10);
            GUILayout.Label("Ramp Preview:");
         
            Rect rect = EditorGUILayout.GetControlRect();

            rect = new Rect(rect.x + 50, rect.y + 10, 400, 200);

            EditorGUI.DrawPreviewTexture(rect, m_RenderTexture);
           

        }
        
    }

    private void SetPreset()
    {
        m_Gradient = RampPreset.Gradient;
        m_CurveXR = RampPreset.CurveXR;
        m_CurveYR = RampPreset.CurveYR;
        m_CurveNSR = RampPreset.CurveNSR;

        m_CurveXG = RampPreset.CurveXG;
        m_CurveYG = RampPreset.CurveYG;
        m_CurveNSG = RampPreset.CurveNSG;

        m_CurveXB = RampPreset.CurveXB;
        m_CurveYB = RampPreset.CurveYB;
        m_CurveNSB = RampPreset.CurveNSB;

        m_CurveXA = RampPreset.CurveXA;
        m_CurveYA = RampPreset.CurveYA;
        m_CurveNSA = RampPreset.CurveNSA;
    }
    private void SavePrset()
    {
       if(null==RampPreset)
        {
            RampPreset presetSaved = ScriptableObject.CreateInstance<RampPreset>();
            presetSaved.Gradient = m_Gradient;
            presetSaved.CurveXR = m_CurveXR;
            presetSaved.CurveYR = m_CurveYR;
            presetSaved.CurveNSR = m_CurveNSR;

            presetSaved.CurveXG = m_CurveXG;
            presetSaved.CurveYG = m_CurveYG;
            presetSaved.CurveNSG = m_CurveNSG;

            presetSaved.CurveXB = m_CurveXB;
            presetSaved.CurveYB = m_CurveYB;
            presetSaved.CurveNSB = m_CurveNSB;

            presetSaved.CurveXA = m_CurveXA;
            presetSaved.CurveYA = m_CurveYA;
            presetSaved.CurveNSA = m_CurveNSA;
            string defaultPath = Application.dataPath;
            savePath_Preset = "";
            savePath_Preset = EditorUtility.SaveFilePanel("保存到", defaultPath, savePath_Preset, "asset");
            savePath_Preset = savePath_Preset.Substring(savePath_Preset.IndexOf("Assets"));
            AssetDatabase.CreateAsset(presetSaved, savePath_Preset);
            AssetDatabase.Refresh();
            RampPreset = presetSaved;
            Debug.Log("Save To:" + savePath_Preset);
        }
        else 
        {

            RampPreset.Gradient = m_Gradient;
            RampPreset.CurveXR = m_CurveXR;
            RampPreset.CurveYR = m_CurveYR;
            RampPreset.CurveNSR = m_CurveNSR;

            RampPreset.CurveXG = m_CurveXG;
            RampPreset.CurveYG = m_CurveYG;
            RampPreset.CurveNSG = m_CurveNSG;

            RampPreset.CurveXB = m_CurveXB;
            RampPreset.CurveYB = m_CurveYB;
            RampPreset.CurveNSB = m_CurveNSB;

            RampPreset.CurveXA = m_CurveXA;
            RampPreset.CurveYA = m_CurveYA;
            RampPreset.CurveNSA = m_CurveNSA;
            EditorUtility.SetDirty(RampPreset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
          
        }
    }
}