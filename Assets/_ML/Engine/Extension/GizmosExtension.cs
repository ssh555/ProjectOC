using System.Collections.Generic;
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
                Gizmos.DrawWireCube(collider.transform.TransformPoint(box.center), box.size);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphere = collider as SphereCollider;
                Gizmos.DrawWireSphere(collider.transform.TransformPoint(sphere.center), sphere.radius);
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsule = collider as CapsuleCollider;
                DrawWireCapsule(capsule);
            }
            else if (collider is MeshCollider)
            {
                MeshCollider meshCollider = collider as MeshCollider;
                Mesh mesh = meshCollider.sharedMesh;
                if (mesh != null)
                {
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = meshCollider.transform.localToWorldMatrix;
                    Gizmos.DrawWireMesh(mesh, Vector3.zero, Quaternion.identity, Vector3.one);
                    Gizmos.matrix = oldMatrix;
                }
            }
            Gizmos.color = gcolor;
        }

        public static void DrawMeshCollider(Collider collider, Color color)
        {
            var gcolor = Gizmos.color;
            Gizmos.color = color;
            if (collider is BoxCollider)
            {
                BoxCollider box = collider as BoxCollider;
                Gizmos.DrawCube(collider.transform.TransformPoint(box.center), box.size);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphere = collider as SphereCollider;
                Gizmos.DrawSphere(collider.transform.TransformPoint(sphere.center), sphere.radius);
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsule = collider as CapsuleCollider;
                DrawCapsule(capsule);
            }
            else if (collider is MeshCollider)
            {
                MeshCollider meshCollider = collider as MeshCollider;
                Mesh mesh = meshCollider.sharedMesh;
                if (mesh != null)
                {
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = meshCollider.transform.localToWorldMatrix;
                    Gizmos.DrawMesh(mesh, Vector3.zero, Quaternion.identity, Vector3.one);
                    Gizmos.matrix = oldMatrix;
                }
            }
            Gizmos.color = gcolor;
        }
        private static void DrawCapsule(CapsuleCollider capsule)
        {
            Vector3 point1 = capsule.bounds.center + Vector3.up * (capsule.height / 2 - capsule.radius);
            Vector3 point2 = capsule.bounds.center - Vector3.up * (capsule.height / 2 - capsule.radius);
            float height = capsule.height - capsule.radius * 2;
            float diameter = capsule.radius * 2;

            // Create capsule mesh
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            int numSegments = 16;
            float segmentAngle = (Mathf.PI * 2) / numSegments;
            for (int i = 0; i <= numSegments; i++)
            {
                float angle = segmentAngle * i;
                float x = Mathf.Cos(angle) * capsule.radius;
                float z = Mathf.Sin(angle) * capsule.radius;

                // Add top hemisphere vertices
                vertices.Add(new Vector3(x, height / 2, z) + capsule.bounds.center);
                // Add bottom hemisphere vertices
                vertices.Add(new Vector3(x, -height / 2, z) + capsule.bounds.center);

                if (i > 0)
                {
                    // Add triangles for top hemisphere
                    indices.Add(vertices.Count - 4); // center top
                    indices.Add(vertices.Count - 2); // next top
                    indices.Add(vertices.Count - 3); // current top

                    // Add triangles for bottom hemisphere
                    indices.Add(vertices.Count - 1); // center bottom
                    indices.Add(vertices.Count - 3); // current bottom
                    indices.Add(vertices.Count - 2); // next bottom

                    // Add side triangles
                    indices.Add(vertices.Count - 4); // current top
                    indices.Add(vertices.Count - 2); // next top
                    indices.Add(vertices.Count - 1); // current bottom

                    indices.Add(vertices.Count - 1); // current bottom
                    indices.Add(vertices.Count - 3); // next bottom
                    indices.Add(vertices.Count - 4); // current top
                }
            }

            // Draw capsule mesh
            Gizmos.DrawMesh(CreateMesh(vertices, indices), capsule.bounds.center, capsule.transform.rotation, Vector3.one);
        }

        private static Mesh CreateMesh(List<Vector3> vertices, List<int> indices)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void DrawWireCapsule(CapsuleCollider capsule)
        {
            Vector3 point1 = capsule.transform.TransformPoint(capsule.center + Vector3.up * (capsule.height / 2 - capsule.radius));
            Vector3 point2 = capsule.transform.TransformPoint(capsule.center - Vector3.up * (capsule.height / 2 - capsule.radius));
            float height = capsule.height - capsule.radius * 2;
            float diameter = capsule.radius * 2;

            Gizmos.DrawWireSphere(point1, capsule.radius);
            Gizmos.DrawWireSphere(point2, capsule.radius);

            // Draw cylinder part
            Gizmos.DrawLine(point1 + capsule.transform.right * capsule.radius, point2 + capsule.transform.right * capsule.radius);
            Gizmos.DrawLine(point1 - capsule.transform.right * capsule.radius, point2 - capsule.transform.right * capsule.radius);
            Gizmos.DrawLine(point1 + capsule.transform.forward * capsule.radius, point2 + capsule.transform.forward * capsule.radius);
            Gizmos.DrawLine(point1 - capsule.transform.forward * capsule.radius, point2 - capsule.transform.forward * capsule.radius);

            // Draw hemisphere parts
            DrawWireHalfSphere(point1, capsule.transform.up, capsule.radius, Color.white);
            DrawWireHalfSphere(point2, -capsule.transform.up, capsule.radius, Color.white);
        }

        private static void DrawWireHalfSphere(Vector3 center, Vector3 upDirection, float radius, Color color)
        {
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Vector3 forward = Vector3.Slerp(-upDirection, Vector3.forward, 0.5f);
            Vector3 right = Vector3.Cross(upDirection, forward);
            for (int i = 1; i <= 8; i++)
            {
                Vector3 prevPoint = center + Quaternion.AngleAxis((i - 1) * 45, upDirection) * right * radius;
                for (int j = 1; j <= 8; j++)
                {
                    Vector3 currPoint = center + Quaternion.AngleAxis(i * 45, upDirection) * right * radius;
                    Gizmos.DrawLine(prevPoint, currPoint);
                    prevPoint = currPoint;
                }
            }
            Gizmos.color = prevColor;
        }
    }

}
