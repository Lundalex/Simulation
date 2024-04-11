using Unity.Mathematics;
using UnityEngine;

public class FileImporter : MonoBehaviour
{
    public Mesh[] Meshes;
    public Tri[] LoadOBJ(int meshIndex, float scale)
    {
        Mesh mesh = Meshes[meshIndex];
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        int triNum = triangles.Length / 3;

        // Set tris data
        Tri[] tris = new Tri[triNum];
        for (int triCount = 0; triCount < triNum; triCount++)
        {
            int triCount3 = 3 * triCount;
            int indexA = triangles[triCount3];
            int indexB = triangles[triCount3 + 1];
            int indexC = triangles[triCount3 + 2];

            tris[triCount] = new Tri
            {
                vA = vertices[indexA] * scale,
                vB = vertices[indexB] * scale,
                vC = vertices[indexC] * scale,
                normal = new float3(0.0f, 0.0f, 0.0f),
                materialKey = 1,
                parentKey = 0,
            };
        }

        return tris;
    }
}