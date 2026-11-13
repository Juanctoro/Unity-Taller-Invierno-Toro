using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildWinterParticles
{
    [MenuItem("Tools/Taller Invernal/Agregar nieve y humo")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        if (GameObject.Find("Global_Blizzard") != null || GameObject.Find("Exhaust_Smoke") != null)
            return;

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            throw new System.InvalidOperationException("URP particle shader missing.");

        var material = new Material(shader);
        material.SetFloat("_Surface", 1);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = 3000;
        const string materialPath = "Assets/Scenes/WinterParticles.mat";
        var existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (existingMaterial == null)
            AssetDatabase.CreateAsset(material, materialPath);
        else
        {
            Object.DestroyImmediate(material);
            material = existingMaterial;
        }

        var snow = Create("Global_Blizzard", null, new Vector3(0, 7, 0), Quaternion.Euler(90, 0, 0), material);
        var main = snow.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.085f);
        main.startColor = new Color(0.82f, 0.91f, 1f, 0.85f);
        main.gravityModifier = 0.23f;
        main.maxParticles = 1800;
        var emission = snow.emission;
        emission.rateOverTime = 420;
        var shape = snow.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18, 16, 0.1f);
        var noise = snow.noise;
        noise.enabled = true;
        noise.strength = 0.9f;

        var car = GameObject.Find("Police_Car").transform;
        var smoke = Create("Exhaust_Smoke", car, new Vector3(0.6f, 0.8f, -2.3f), Quaternion.Euler(-55, 180, 0), material);
        main = smoke.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.3f, 2.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.24f);
        emission = smoke.emission;
        emission.rateOverTime = 23;
        shape = smoke.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 7;
        shape.radius = 0.045f;
        var fade = smoke.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(0.18f, 0.19f, 0.21f), 0), new GradientColorKey(new Color(0.38f, 0.4f, 0.44f), 1) },
            new[] { new GradientAlphaKey(0.62f, 0), new GradientAlphaKey(0, 1) });
        fade.color = new ParticleSystem.MinMaxGradient(gradient);
        var size = smoke.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.25f, 1, 1));

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Winter particles saved.");
    }

    private static ParticleSystem Create(string name, Transform parent, Vector3 position, Quaternion rotation, Material material)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.SetLocalPositionAndRotation(position, rotation);
        var particles = obj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.prewarm = true;
        obj.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        return particles;
    }
}
