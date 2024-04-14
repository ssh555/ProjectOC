using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

namespace ML.MathExtension
{
    [CustomEditor(typeof(BezierComponent))]
    public class BezierComponentEditor : UnityEditor.Editor
    {
        //这两个画Beizer线段的时候要用
        private Vector3 lastPosition;

        private Vector3 lastOutTangent;

        //正在操作哪个控制点
        int pickedIndex = -1;

        //正在操作控制点的哪一部分
        enum CtrlPointPickedType
        {
            Position,
            InTangent,
            OutTangent
        }

        CtrlPointPickedType pickedType = CtrlPointPickedType.Position;

        private void OnSceneGUI()
        {
            BezierComponent bezierComponent = target as BezierComponent;
            //处理拖动操作的部分
            //防越界
            if (pickedIndex >= bezierComponent.ctrlPoints.Count)
            {
                pickedIndex = -1;
            }

            if (pickedIndex != -1)
            {
                //得到正在操作的控制点
                BezierComponent.CtrlPoint pickedCtrlPoint = bezierComponent.ctrlPoints[pickedIndex];
                //角点只能编辑位置不能编辑Tangent
                if (pickedCtrlPoint.type == BezierComponent.BezierPointType.Corner)
                    pickedType = CtrlPointPickedType.Position;
                if (pickedType == CtrlPointPickedType.Position)
                {
                    //使用PositionHandle操作它的位置
                    Vector3 newPosition = Handles.PositionHandle(pickedCtrlPoint.position, Quaternion.identity);
                    pickedCtrlPoint.position = newPosition;
                }
                else if (pickedType == CtrlPointPickedType.InTangent)
                {
                    //使用PositionHandle操作InTangent
                    Vector3 position = pickedCtrlPoint.position;
                    Vector3 newInTangent =
                        Handles.PositionHandle(pickedCtrlPoint.InTangent + position, Quaternion.identity) - position;
                    pickedCtrlPoint.InTangent = newInTangent;
                }
                else if (pickedType == CtrlPointPickedType.OutTangent)
                {
                    //跟上一个差不多
                    Vector3 position = pickedCtrlPoint.position;
                    Vector3 newOutTangent =
                        Handles.PositionHandle(pickedCtrlPoint.OutTangent + position, Quaternion.identity) - position;
                    pickedCtrlPoint.OutTangent = newOutTangent;
                }
            }


            for (int i = 0; i < bezierComponent.ctrlPoints.Count; i++)
            {
                //一个个地把控制点渲染出来
                BezierComponent.CtrlPoint ctrlPoint = bezierComponent.ctrlPoints[i];
                BezierComponent.BezierPointType type = ctrlPoint.type;

                Vector3 position = BezierComponent.TranslateBezierPos(bezierComponent, ctrlPoint.position);

                Vector3 inTangentPoint =
                    BezierComponent.TranslateBezierPos(bezierComponent, ctrlPoint.InTangent + ctrlPoint.position);
                Vector3 outTangentPoint =
                    BezierComponent.TranslateBezierPos(bezierComponent, ctrlPoint.OutTangent + ctrlPoint.position);

                /*
                bool button_picked = Handles.Button(position, Quaternion.identity, 0.1f, 0.1f, Handles.CubeHandleCap);
                if (button_picked) {
                    //只要点了这个控制点，就是选了它，下一帧PositionHandle就在它上面出现
                    pickedIndex = i;
                    pickedType = CtrlPointPickedType.Position;
                    //to-do:
                }

                if (type != BezierComponent.BezierPointType.Corner)
                {
                    //画InTangent
                    Handles.DrawLine(position, inTangentPoint);
                    bool in_tangent_picked = Handles.Button(inTangentPoint, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap);
                    if (in_tangent_picked)
                    {
                        pickedIndex = i;
                        pickedType = CtrlPointPickedType.InTangent;
                        //to-do:
                    }
                    //画OutTangent
                    Handles.DrawLine(position, outTangentPoint);
                    bool out_tangent_picked = Handles.Button(outTangentPoint, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap);
                    if (out_tangent_picked)
                    {
                        pickedIndex = i;
                        pickedType = CtrlPointPickedType.OutTangent;
                        //to_do:
                    }
                }
                */
                //从第二个控制点开始画Bezier线段
                if (i > 0)
                {
                    Handles.DrawBezier(lastPosition, position, lastOutTangent, inTangentPoint, Color.green, null, 2f);
                }

                //所以每次先暂存下控制点位置和OutTangent，留给下一个控制点画线用
                lastPosition = position;
                lastOutTangent = outTangentPoint;
            }
        }

    }
}