using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ML.Engine.Extension
{
    public static class GizmosExtension
    {
        public static void DrawWireCollider(Collider collider, Color color)
        {
            var gcolor = Gizmos.color;
            Gizmos.color = color;
            if (collider is BoxCollider)
            {
                BoxCollider box = collider as BoxCollider;
                Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphere = collider as SphereCollider;
                Gizmos.DrawWireSphere(sphere.bounds.center, sphere.radius);
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsule = collider as CapsuleCollider;
                // 绘制胶囊碰撞体的线框形状...
                Vector3 point1 = capsule.bounds.center + Vector3.up * (capsule.height / 2 - capsule.radius);
                Vector3 point2 = capsule.bounds.center - Vector3.up * (capsule.height / 2 - capsule.radius);
                Gizmos.DrawWireSphere(point1, capsule.radius);
                Gizmos.DrawWireSphere(point2, capsule.radius);
                Gizmos.DrawLine(point1 + Vector3.right * capsule.radius, point2 + Vector3.right * capsule.radius);
                Gizmos.DrawLine(point1 - Vector3.right * capsule.radius, point2 - Vector3.right * capsule.radius);
                Gizmos.DrawLine(point1 + Vector3.forward * capsule.radius, point2 + Vector3.forward * capsule.radius);
                Gizmos.DrawLine(point1 - Vector3.forward * capsule.radius, point2 - Vector3.forward * capsule.radius);
            }
            else if (collider is MeshCollider)
            {
                MeshCollider meshCollider = collider as MeshCollider;
                Mesh mesh = meshCollider.sharedMesh;
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    int[] triangles = mesh.GetTriangles(i);
                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        Vector3 p1 = meshCollider.transform.TransformPoint(mesh.vertices[triangles[j]]);
                        Vector3 p2 = meshCollider.transform.TransformPoint(mesh.vertices[triangles[j + 1]]);
                        Vector3 p3 = meshCollider.transform.TransformPoint(mesh.vertices[triangles[j + 2]]);
                        Gizmos.DrawLine(p1, p2);
                        Gizmos.DrawLine(p2, p3);
                        Gizmos.DrawLine(p3, p1);
                    }
                }
            }
            Gizmos.color = gcolor;
        }
    }

}
