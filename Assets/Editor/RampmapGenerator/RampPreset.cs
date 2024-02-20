using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RampPreset : ScriptableObject
{
    public Gradient Gradient=new Gradient();
    //public List<Keyframe> Keys=new List<Keyframe>();

    public AnimationCurve CurveXR = new AnimationCurve();
    public AnimationCurve CurveYR = new AnimationCurve();
    public AnimationCurve CurveNSR = new AnimationCurve();

    public AnimationCurve CurveXG = new AnimationCurve();
    public AnimationCurve CurveYG = new AnimationCurve();
    public AnimationCurve CurveNSG = new AnimationCurve();

    public AnimationCurve CurveXB = new AnimationCurve();
    public AnimationCurve CurveYB = new AnimationCurve();
    public AnimationCurve CurveNSB = new AnimationCurve();
    
    public AnimationCurve CurveXA = new AnimationCurve();
    public AnimationCurve CurveYA = new AnimationCurve();
    public AnimationCurve CurveNSA = new AnimationCurve();
}
