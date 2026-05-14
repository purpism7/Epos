using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class EffectMaterialUrpConverter
{
    private const string TargetShaderName = "Universal Render Pipeline/Particles/Unlit";

    private static readonly string[] SearchFolders =
    {
        "Assets/3_Resource/Effect",
        "Assets/Epic Toon FX"
    };

    [MenuItem("Tools/Epos/Materials/Convert Effect Materials To URP Particles Unlit")]
    public static void ConvertEffectMaterials()
    {
        var targetShader = Shader.Find(TargetShaderName);
        if (targetShader == null)
        {
            Debug.LogError($"Could not find shader: {TargetShaderName}");
            return;
        }

        var materialGuids = AssetDatabase.FindAssets("t:Material", SearchFolders);
        var changed = 0;
        var skipped = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in materialGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                    continue;

                if (!ShouldConvert(material))
                {
                    skipped++;
                    continue;
                }

                Undo.RecordObject(material, "Convert Effect Material To URP");
                ConvertMaterial(material, targetShader, path);
                EditorUtility.SetDirty(material);
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Converted {changed} effect materials to {TargetShaderName}. Skipped {skipped} materials.");
    }

    private static bool ShouldConvert(Material material)
    {
        if (material.shader == null)
            return false;

        if (material.shader.name == TargetShaderName)
            return false;

        return material.HasProperty("_MainTex") ||
               material.HasProperty("_TintColor") ||
               material.HasProperty("_SoftParticlesEnabled");
    }

    private static void ConvertMaterial(Material material, Shader targetShader, string path)
    {
        var mainTexture = GetTexture(material, "_BaseMap", "_MainTex");
        var mainScale = material.HasProperty("_MainTex") ? material.GetTextureScale("_MainTex") : Vector2.one;
        var mainOffset = material.HasProperty("_MainTex") ? material.GetTextureOffset("_MainTex") : Vector2.zero;
        var color = GetColor(material, "_BaseColor", "_Color");
        var emissionMap = GetTexture(material, "_EmissionMap");
        var emissionColor = GetColor(Color.black, material, "_EmissionColor");
        var cutoff = GetFloat(material, "_Cutoff", 0.5f);
        var softParticlesEnabled = GetFloat(material, "_SoftParticlesEnabled", 0f) > 0.5f;
        var cameraFadingEnabled = GetFloat(material, "_CameraFadingEnabled", 0f) > 0.5f;
        var blend = ResolveBlendMode(material, path);

        material.shader = targetShader;

        SetTexture(material, "_BaseMap", mainTexture, mainScale, mainOffset);
        SetColor(material, "_BaseColor", color);
        SetTexture(material, "_EmissionMap", emissionMap, Vector2.one, Vector2.zero);
        SetColor(material, "_EmissionColor", emissionColor);
        SetFloat(material, "_Cutoff", cutoff);

        SetTransparentRenderState(material, blend);

        SetFloat(material, "_SoftParticlesEnabled", softParticlesEnabled ? 1f : 0f);
        SetKeyword(material, "_SOFTPARTICLES_ON", softParticlesEnabled);
        SetFloat(material, "_CameraFadingEnabled", cameraFadingEnabled ? 1f : 0f);
        SetKeyword(material, "_FADING_ON", cameraFadingEnabled);
    }

    private static BlendModeType ResolveBlendMode(Material material, string path)
    {
        var srcBlend = Mathf.RoundToInt(GetFloat(material, "_SrcBlend", -1f));
        var dstBlend = Mathf.RoundToInt(GetFloat(material, "_DstBlend", -1f));
        var lowerPath = path.ToLowerInvariant();

        if (srcBlend == (int)BlendMode.SrcAlpha && dstBlend == (int)BlendMode.One)
            return BlendModeType.Additive;

        if (lowerPath.Contains("_add") || lowerPath.Contains(" add"))
            return BlendModeType.Additive;

        return BlendModeType.Alpha;
    }

    private static void SetTransparentRenderState(Material material, BlendModeType blend)
    {
        SetFloat(material, "_Surface", 1f);
        SetFloat(material, "_Cull", (float)CullMode.Off);
        SetFloat(material, "_ZWrite", 0f);
        SetFloat(material, "_AlphaClip", 0f);
        SetFloat(material, "_BlendOp", (float)BlendOp.Add);
        SetFloat(material, "_SrcBlendAlpha", (float)BlendMode.One);
        SetFloat(material, "_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;

        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_ALPHAMODULATE_ON");

        if (blend == BlendModeType.Additive)
        {
            SetFloat(material, "_Blend", 2f);
            SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloat(material, "_DstBlend", (float)BlendMode.One);
            return;
        }

        SetFloat(material, "_Blend", 0f);
        SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
        SetFloat(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
    }

    private static Texture GetTexture(Material material, params string[] names)
    {
        foreach (var name in names)
        {
            if (material.HasProperty(name))
                return material.GetTexture(name);
        }

        return null;
    }

    private static Color GetColor(Material material, params string[] names)
    {
        return GetColor(Color.white, material, names);
    }

    private static Color GetColor(Color defaultValue, Material material, params string[] names)
    {
        foreach (var name in names)
        {
            if (material.HasProperty(name))
                return material.GetColor(name);
        }

        return defaultValue;
    }

    private static float GetFloat(Material material, string name, float defaultValue)
    {
        return material.HasProperty(name) ? material.GetFloat(name) : defaultValue;
    }

    private static void SetTexture(Material material, string name, Texture texture, Vector2 scale, Vector2 offset)
    {
        if (!material.HasProperty(name))
            return;

        material.SetTexture(name, texture);
        material.SetTextureScale(name, scale);
        material.SetTextureOffset(name, offset);
    }

    private static void SetColor(Material material, string name, Color color)
    {
        if (material.HasProperty(name))
            material.SetColor(name, color);
    }

    private static void SetFloat(Material material, string name, float value)
    {
        if (material.HasProperty(name))
            material.SetFloat(name, value);
    }

    private static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
            material.EnableKeyword(keyword);
        else
            material.DisableKeyword(keyword);
    }

    private enum BlendModeType
    {
        Alpha,
        Additive
    }
}
