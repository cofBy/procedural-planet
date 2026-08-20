using System.Collections.Generic;
using UnityEngine;

public class planetGenerator : MonoBehaviour
{
    [Header("making cube sphere")]
    public float size;
    public int subDivisions;
    Mesh mesh;

    [Header("random generation")]
    public float noiseScale;
    public float noiseStrength;

    public int octaves;
    public float octaveStrengthChange;
    public float octaveSizeChange;

    public int seed;

    [Header("color")]
    public Gradient terrainColor;
    public Gradient steepColor;
    public AnimationCurve steepnessCurve;
    public float sampleDistance;
    public Material terrainMat;
    public int colorRes;

    [Header("collider")]
    public MeshCollider col;

    [Header("uvs")]
    public float uvScale;
    public float uvSharpness;

    private void Start()
    {
        generate();
    }
    public void generate()
    {
        MeshFilter filter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = "planet";
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

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

                    Vector3 pos = cubeSphere(new Vector3((x - half) / (subDivisions + 1), (y - half) / (subDivisions + 1), (z - half) / (subDivisions + 1)) * 2);
                    Vector3 normal = pos.normalized;

                    float u = (float)x / (subDivisions + 1) * uvScale;
                    float v = (float)y / (subDivisions + 1) * uvScale;
                    float w = (float)z / (subDivisions + 1) * uvScale;
                    Vector3 blend = new Vector3(Mathf.Pow(Mathf.Abs(normal.x), uvSharpness), Mathf.Pow(Mathf.Abs(normal.y), uvSharpness), Mathf.Pow(Mathf.Abs(normal.z), uvSharpness));
                    blend /= Vector3.Dot(blend, Vector3.one);
                    uv[index] = new Vector2(w, v) * blend.x + new Vector2(u, w) * blend.y + new Vector2(u, v) * blend.z;

                    indexLookup[x + y * res + z * res * res] = index;

                    float noiseValue = calculateHeight(pos);

                    Vector3 vertPos = pos * size + normal * noiseValue * noiseStrength;
                    Vector3 grad = calculateGradient(pos, sampleDistance);
                    grad -= normal * Vector3.Dot(grad, normal);
                    float slope = (noiseStrength / size) * grad.magnitude;
                    float steepnessDeg = Mathf.Atan(slope) * Mathf.Rad2Deg;
                    vertices[index] = vertPos;
                    noiseUV[index] = new Vector2(noiseValue * 0.5f + 0.5f, steepnessDeg / 90f);
                    index++;
                }
            }
        }

        Texture2D gradientTex = new Texture2D(colorRes, colorRes, TextureFormat.RGBAHalf, false);
        for (int u = 0; u < colorRes; u++)
        {
            for (int v = 0; v < colorRes; v++)
            {
                Color flat = terrainColor.Evaluate((float)u / colorRes);
                Color steep = steepColor.Evaluate((float)u / colorRes);
                float blend = steepnessCurve.Evaluate((float)v / colorRes);

                gradientTex.SetPixel(u, v, Color.Lerp(flat, steep, blend));
            }
        }
        gradientTex.Apply(false);
        terrainMat.SetTexture("_colorGradient", gradientTex);

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

    Vector3 cubeSphere(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;

        float x = p.x * Mathf.Sqrt(Mathf.Max(0f, 1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f));
        float y = p.y * Mathf.Sqrt(Mathf.Max(0f, 1f - z2 / 2f - x2 / 2f + z2 * x2 / 3f));
        float z = p.z * Mathf.Sqrt(Mathf.Max(0f, 1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f));

        return new Vector3(x, y, z);
    }
    float calculateHeight(Vector3 pos)
    {
        float noiseValue = 0;
        float scale = noiseScale;
        float strength = 1;
        float maxStrength = 0;
        for (int i = 0; i < octaves; i++)
        {
            noiseValue += perlinNoise3D.perlin3D(pos.x * scale + seed, pos.y * scale + seed, pos.z * scale + seed) * strength;
            maxStrength += scale;
            strength *= octaveStrengthChange;
            scale *= octaveSizeChange;
        }
        return noiseValue;
    }
    Vector3 calculateGradient(Vector3 pos, float step)
    {
        float height = calculateHeight(pos);
        float dx = (calculateHeight(pos + new Vector3(step, 0, 0)) - height) / step;
        float dy = (calculateHeight(pos + new Vector3(0, step, 0)) - height) / step;
        float dz = (calculateHeight(pos + new Vector3(0, 0, step)) - height) / step;
        return new Vector3(dx, dy, dz);
    }
}