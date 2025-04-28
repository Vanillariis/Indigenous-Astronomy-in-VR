using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosphereGenerator : MonoBehaviour
{
    [Range(0, 5)]
    public int subdivisions = 1;
    public float radius = 1f;

    private static readonly float t = (1f + Mathf.Sqrt(5f)) / 2f;

    private void Start()
    {
        Generate();
    }
    
    private void OnValidate()
    {
        Generate();
    }
    void Generate()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Create initial icosahedron vertices
        var vertices = new List<Vector3>
        {
            new Vector3(-1,  t,  0).normalized,
            new Vector3( 1,  t,  0).normalized,
            new Vector3(-1, -t,  0).normalized,
            new Vector3( 1, -t,  0).normalized,

            new Vector3( 0, -1,  t).normalized,
            new Vector3( 0,  1,  t).normalized,
            new Vector3( 0, -1, -t).normalized,
            new Vector3( 0,  1, -t).normalized,

            new Vector3( t,  0, -1).normalized,
            new Vector3( t,  0,  1).normalized,
            new Vector3(-t,  0, -1).normalized,
            new Vector3(-t,  0,  1).normalized
        };

        var triangles = new List<int>
        {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
            1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
            4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };

        // Refine triangles
        var midCache = new Dictionary<long, int>();
        for (int i = 0; i < subdivisions; i++)
        {
            var newTris = new List<int>();
            for (int j = 0; j < triangles.Count; j += 3)
            {
                int v1 = triangles[j];
                int v2 = triangles[j + 1];
                int v3 = triangles[j + 2];

                int a = GetMidpoint(vertices, midCache, v1, v2);
                int b = GetMidpoint(vertices, midCache, v2, v3);
                int c = GetMidpoint(vertices, midCache, v3, v1);

                newTris.AddRange(new[] {
                    v1, a, c,
                    v2, b, a,
                    v3, c, b,
                    a, b, c
                });
            }
            triangles = newTris;
        }

        // Finalize vertices and normals
        var finalVerts = new Vector3[vertices.Count];
        var normals = new Vector3[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            finalVerts[i] = vertices[i] * radius;
            normals[i] = vertices[i]; // point outward
        }

        mesh.Clear();
        mesh.vertices = finalVerts;
        mesh.normals = normals;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }

    int GetMidpoint(List<Vector3> verts, Dictionary<long, int> cache, int i1, int i2)
    {
        long key = ((long)Mathf.Min(i1, i2) << 32) + Mathf.Max(i1, i2);
        if (cache.TryGetValue(key, out int ret))
            return ret;

        Vector3 mid = ((verts[i1] + verts[i2]) * 0.5f).normalized;
        verts.Add(mid);
        ret = verts.Count - 1;
        cache[key] = ret;
        return ret;
    }
}
