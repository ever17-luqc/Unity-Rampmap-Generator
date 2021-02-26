using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ToonRampMapGen : EditorWindow
{
    [MenuItem("ever17_Tools/美术贴图工具/ToonRampMap生成器",false, 1000)]
    public static void window()
    {
        Rect rect = new Rect(Screen.width / 2, Screen.height / 2, 500, 500);
        ToonRampMapGen window = (ToonRampMapGen)EditorWindow.GetWindow(typeof(ToonRampMapGen), true, "ToonRampMap生成器");
        window.position = rect;
    }
    enum TexResolutions
    {
        Tex_128 =0,
        Tex_256 =1,
        Tex_512=2,
        Tex_1024=3,
        Tex_2048=4,
       
            
            }
   
    List<Color> color = new List<Color>();
    string savePath = "";
    string saveName = "";
    Gradient gradient=new Gradient();
    TexResolutions resolution = 0;
    
   
      void DrawRamp(Gradient gradient, TexResolutions resolution,string saveName)
    {    
        int _resolution = 128*(int)Mathf.Pow(2, (int)resolution);
        Texture2D RampTex = new Texture2D(_resolution, 1);
        

        color.Clear();
        for(int index=0; index < _resolution; index++)
        {
            color.Add(gradient.Evaluate(index /(float) _resolution));
            RampTex.SetPixel(index, 0, color[index]);
        }

        RampTex.Apply();
        
        
       
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
        //EditorGUILayout.LabelField("RampMap X方向分辨率");
        resolution = (TexResolutions)EditorGUILayout.EnumPopup("RampMap X方向分辨率", resolution);
        GUILayoutOption[] gradientOptions = new GUILayoutOption[] { GUILayout.Width(400), GUILayout.Height(50) };
        gradient =EditorGUILayout.GradientField("选择颜色映射",gradient, gradientOptions);
        //EditorGUILayout.LabelField("输入保存贴图的名字");
        saveName = EditorGUILayout.TextField("输入保存贴图的名字",saveName);
        
        if (GUI.Button(new Rect(150, 400, 300, 30), "保存"))
        {
            DrawRamp(gradient, resolution, saveName);
            
            
         }
    }

}
