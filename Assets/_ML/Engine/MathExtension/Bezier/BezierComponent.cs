using System;
using System.Collections.Generic;
using UnityEngine;

namespace ML.MathExtension
{
    public class BezierComponent : MonoBehaviour
{
    private void Awake()
    {
        this.enabled = false;
    }

    public List<CtrlPoint> ctrlPoints;
    public Vector3 centerPos; 
   public enum BezierPointType
   {
       Corner,
       Smooth,
       BezierCorner
   }
   
   public Vector3 Evaluate(float t)
   {
       float segmentCount = ctrlPoints.Count-1;
       if (ctrlPoints.Count == 0) return transform.position;
       if (ctrlPoints.Count == 1) return ctrlPoints[0].position;
       
       t = Mathf.Clamp(t, 0, segmentCount);
       int segment_index = (int)t;
       if (segment_index == segmentCount) segment_index -= 1;
       Vector3 p0 = ctrlPoints[segment_index].position;
       Vector3 p1 = ctrlPoints[segment_index].OutTangent + p0;
       Vector3 p3 = ctrlPoints[segment_index + 1].position;
       Vector3 p2 = ctrlPoints[segment_index + 1].InTangent + p3;

       t = t - segment_index;
       float u = 1 - t;
       Vector3 res = p0 * u * u * u + 3 * p1 * u * u * t + 3 * p2 * u * t * t + p3 * t * t * t;
       return TranslateBezierPos(this,res);
   }
   public static Vector3 TranslateBezierPos(BezierComponent bezierComponent,Vector3 originPos)
   {
       Transform bezierTrans = bezierComponent.transform;
       Vector3 offsetPos = bezierTrans.localPosition + bezierComponent.centerPos;
       return bezierTrans.TransformPoint(originPos + offsetPos);
   }
   
   //该类可以序列化
   [System.Serializable] 
   public class CtrlPoint
   {
       public BezierPointType type;
       public Vector3 position;
       [SerializeField]
       Vector3 inTangent;
       [SerializeField]
       Vector3 outTangent;
   
       public Vector3 InTangent
       {
           get {
               if (type == BezierPointType.Corner) return Vector3.zero;
               else return inTangent;
           }
           set {
               if (type != BezierPointType.Corner) inTangent = value;
               if (value.sqrMagnitude > 0.001 && type == BezierPointType.Smooth) {
                   outTangent = value.normalized * (-1) * outTangent.magnitude;
               }
           }
       }
   
       public Vector3 OutTangent 
       {
           get {
               if (type == BezierPointType.Corner) return Vector3.zero;
               if (type == BezierPointType.Smooth)
               {
                   if (inTangent.sqrMagnitude > 0.001)
                   {
                       return inTangent.normalized * (-1) * outTangent.magnitude;
                   }
               }
               return outTangent;
           }
           set {
               if (type == BezierPointType.Smooth) {
                   if (value.sqrMagnitude > 0.001) {
                       inTangent = value.normalized * (-1) * inTangent.magnitude;
                   }
                   outTangent = value;
               }
               if (type == BezierPointType.BezierCorner) outTangent = value;
           }
       }
       
   } 
}
}

