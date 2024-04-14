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
        //��������Beizer�߶ε�ʱ��Ҫ��
        private Vector3 lastPosition;

        private Vector3 lastOutTangent;

        //���ڲ����ĸ����Ƶ�
        int pickedIndex = -1;

        //���ڲ������Ƶ����һ����
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
            //�����϶������Ĳ���
            //��Խ��
            if (pickedIndex >= bezierComponent.ctrlPoints.Count)
            {
                pickedIndex = -1;
            }

            if (pickedIndex != -1)
            {
                //�õ����ڲ����Ŀ��Ƶ�
                BezierComponent.CtrlPoint pickedCtrlPoint = bezierComponent.ctrlPoints[pickedIndex];
                //�ǵ�ֻ�ܱ༭λ�ò��ܱ༭Tangent
                if (pickedCtrlPoint.type == BezierComponent.BezierPointType.Corner)
                    pickedType = CtrlPointPickedType.Position;
                if (pickedType == CtrlPointPickedType.Position)
                {
                    //ʹ��PositionHandle��������λ��
                    Vector3 newPosition = Handles.PositionHandle(pickedCtrlPoint.position, Quaternion.identity);
                    pickedCtrlPoint.position = newPosition;
                }
                else if (pickedType == CtrlPointPickedType.InTangent)
                {
                    //ʹ��PositionHandle����InTangent
                    Vector3 position = pickedCtrlPoint.position;
                    Vector3 newInTangent =
                        Handles.PositionHandle(pickedCtrlPoint.InTangent + position, Quaternion.identity) - position;
                    pickedCtrlPoint.InTangent = newInTangent;
                }
                else if (pickedType == CtrlPointPickedType.OutTangent)
                {
                    //����һ�����
                    Vector3 position = pickedCtrlPoint.position;
                    Vector3 newOutTangent =
                        Handles.PositionHandle(pickedCtrlPoint.OutTangent + position, Quaternion.identity) - position;
                    pickedCtrlPoint.OutTangent = newOutTangent;
                }
            }


            for (int i = 0; i < bezierComponent.ctrlPoints.Count; i++)
            {
                //һ�����ذѿ��Ƶ���Ⱦ����
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
                    //ֻҪ����������Ƶ㣬����ѡ��������һ֡PositionHandle�������������
                    pickedIndex = i;
                    pickedType = CtrlPointPickedType.Position;
                    //to-do:
                }

                if (type != BezierComponent.BezierPointType.Corner)
                {
                    //��InTangent
                    Handles.DrawLine(position, inTangentPoint);
                    bool in_tangent_picked = Handles.Button(inTangentPoint, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap);
                    if (in_tangent_picked)
                    {
                        pickedIndex = i;
                        pickedType = CtrlPointPickedType.InTangent;
                        //to-do:
                    }
                    //��OutTangent
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
                //�ӵڶ������Ƶ㿪ʼ��Bezier�߶�
                if (i > 0)
                {
                    Handles.DrawBezier(lastPosition, position, lastOutTangent, inTangentPoint, Color.green, null, 2f);
                }

                //����ÿ�����ݴ��¿��Ƶ�λ�ú�OutTangent��������һ�����Ƶ㻭����
                lastPosition = position;
                lastOutTangent = outTangentPoint;
            }
        }

    }
}