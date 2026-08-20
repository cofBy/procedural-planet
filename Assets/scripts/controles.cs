using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class controles : MonoBehaviour
{
    [Header("randomizing")]
    public Button seed;
    public Button details;
    public Button all;
    public Button color;
    public planetGenerator generator;

    private void Awake()
    {
        seed.onClick.AddListener(() => { randomSeed(); });
        details.onClick.AddListener(() => { randomDetails(); });
        all.onClick.AddListener(randomAll);
        color.onClick.AddListener(randomColor);
    }

    void randomSeed(bool generate = true)
    {
        generator.seed = Random.Range(0, 999);
        if (generate == true) generator.generate();
    }
    void randomDetails(bool generate = true)
    {
        generator.noiseStrength = Random.Range(0.2f, 1f);
        generator.octaveSizeChange = Random.Range(0.5f, 2f);
        generator.octaveStrengthChange = Random.Range(0.25f, 0.85f);
        generator.octaves = Random.Range(1, 5);
        if (generate == true) generator.generate();
    }
    void randomAll()
    {
        randomSeed(false);
        randomDetails(false);
        generator.generate();
    }
    void randomColor()
    {
        generator.terrainColor = randomGradient();
        generator.steepColor = randomGradient();
        generator.steepnessCurve = randomCurve();
        generator.generate();
    }
    Gradient randomGradient()
    {
        Gradient g = new Gradient();
        g.mode = (Random.value >= 0.6f) ? GradientMode.Fixed : (Random.value >= 0.3f) ? GradientMode.Blend : GradientMode.PerceptualBlend;
        GradientColorKey[] keys = new GradientColorKey[Random.Range(2, 5)];
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i] = new GradientColorKey(Color.HSVToRGB(Random.value, Random.value, Random.value), Mathf.Clamp01(((float)i / keys.Length) + Random.Range(-0.1f, 0.1f)));
        }
        g.colorKeys = keys;
        return g;
    }
    AnimationCurve randomCurve()
    {
        Keyframe[] keys = new Keyframe[Random.Range(2, 4)];
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i] = new Keyframe(Mathf.Clamp01(((float)i / keys.Length) + Random.Range(-0.1f, 0.1f)), Random.value);
        }
        return new AnimationCurve(keys);
    }
}
