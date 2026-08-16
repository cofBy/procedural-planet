using System.Collections.Generic;
using UnityEngine;

public class planetGenerator : MonoBehaviour
{
    [Header("making cube sphere")]
    public float size;
    public int subDivisions;

    [Header("random generation")]
    public float noiseScale;
    public float noiseStrength;

    public int octaves;
    public float octaveStengthDecrease;
    public float octaveScaleIncrease;

    public int seed;

    [Header("collider")]
    public MeshCollider col;

    [Header("uvs")]
    public float uvScale;
    public float uvSharpness;

    private void Start()
    {
        seed = Random.Range(0, 99);
        generate();
    }
    private void OnValidate()
    {
        generate();
    }
    public void generate()
    {
        MeshFilter filter = GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        mesh.name = "planet";

        int res = subDivisions + 2;
        int surfaceVertices = (int)Mathf.Pow(res, 3) - (int)Mathf.Pow(subDivisions, 3);

        Vector3[] vertices = new Vector3[surfaceVertices];
        Vector2[] uv = new Vector2[surfaceVertices];
        Vector2[] noiseUV = new Vector2[surfaceVertices];

        var indexLookup = new Dictionary<int, int>();

        float half = ((float)subDivisions + 1) * 0.5f;
        int index = 0;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                for (int z = 0; z < res; z++)
                {
                    bool isSurface = (x == 0 || x == subDivisions + 1 ||
                                      y == 0 || y == subDivisions + 1 ||
                                      z == 0 || z == subDivisions + 1);

                    if (!isSurface) continue;

                    Vector3 cubePos = new Vector3((x - half) / (subDivisions + 1), (y - half) / (subDivisions + 1), (z - half) / (subDivisions + 1));
                    Vector3 normal = cubePos.normalized;
                    float u = (float)x / (subDivisions + 1) * uvScale;
                    float v = (float)y / (subDivisions + 1) * uvScale;
                    float w = (float)z / (subDivisions + 1) * uvScale;
                    Vector3 blend = new Vector3(Mathf.Pow(Mathf.Abs(normal.x), uvSharpness), Mathf.Pow(Mathf.Abs(normal.y), uvSharpness), Mathf.Pow(Mathf.Abs(normal.z), uvSharpness));
                    blend /= Vector3.Dot(blend, Vector3.one);
                    uv[index] = new Vector2(w, v) * blend.x + new Vector2(u, w) * blend.y + new Vector2(u, v) * blend.z;

                    indexLookup[x + y * res + z * res * res] = index;

                    float noiseValue = Noise3D(x * noiseScale + seed, y * noiseScale + seed, z * noiseScale + seed) * noiseStrength;
                    float oScale = noiseScale;
                    float oStrength = noiseStrength;
                    for (int i = 0; i < octaves; i++)
                    {
                        oScale += octaveScaleIncrease;
                        oStrength = Mathf.Max(oStrength - octaveStengthDecrease, 0);
                        noiseValue += Noise3D(x * oScale + i * 10, y * oScale + i * 10, z * oScale + i * 10) * oStrength;
                    }
                    Vector3 vertPos = cubeSphere(cubePos * 2) * size + normal * noiseValue;

                    vertices[index] = vertPos;
                    noiseUV[index] = new Vector2(noiseValue, 0);
                    index++;
                }
            }
        }

        int GetIndex(int gx, int gy, int gz) => indexLookup[gx + gy * res + gz * res * res];

        var triList = new List<int>();

        void AddQuad(int a, int b, int c, int d)
        {
            triList.Add(a);
            triList.Add(b);
            triList.Add(c);
            triList.Add(a);
            triList.Add(c);
            triList.Add(d);
        }

        int last = subDivisions + 1;

        for (int y = 0; y < last; y++)
        {
            for (int z = 0; z < last; z++)
            {
                AddQuad(GetIndex(0, y, z), GetIndex(0, y, z + 1), GetIndex(0, y + 1, z + 1), GetIndex(0, y + 1, z));
                AddQuad(GetIndex(last, y, z), GetIndex(last, y + 1, z), GetIndex(last, y + 1, z + 1), GetIndex(last, y, z + 1));
            }
        }
        for (int x = 0; x < last; x++)
        {
            for (int z = 0; z < last; z++)
            {
                AddQuad(GetIndex(x, 0, z), GetIndex(x + 1, 0, z), GetIndex(x + 1, 0, z + 1), GetIndex(x, 0, z + 1));
                AddQuad(GetIndex(x, last, z), GetIndex(x, last, z + 1), GetIndex(x + 1, last, z + 1), GetIndex(x + 1, last, z));
            }
        }
        for (int x = 0; x < last; x++)
        {
            for (int y = 0; y < last; y++)
            {
                AddQuad(GetIndex(x, y, 0), GetIndex(x, y + 1, 0), GetIndex(x + 1, y + 1, 0), GetIndex(x + 1, y, 0));
                AddQuad(GetIndex(x, y, last), GetIndex(x + 1, y, last), GetIndex(x + 1, y + 1, last), GetIndex(x, y + 1, last));
            }
        }

        int[] triangles = triList.ToArray();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.uv2 = noiseUV;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.mesh = mesh;
        if (col != null)
        {
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
    float Noise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);

        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float zx = Mathf.PerlinNoise(z, x);

        return (xy + yz + xz + yx + zy + zx) / 6;
    }
    public static Vector3 cubeSphere(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;

        float x = p.x * Mathf.Sqrt(Mathf.Max(0f, 1f - y2 / 2f - z2 / 2f + (y2 * z2) / 3f));
        float y = p.y * Mathf.Sqrt(Mathf.Max(0f, 1f - z2 / 2f - x2 / 2f + (z2 * x2) / 3f));
        float z = p.z * Mathf.Sqrt(Mathf.Max(0f, 1f - x2 / 2f - y2 / 2f + (x2 * y2) / 3f));

        return new Vector3(x, y, z);
    }
}