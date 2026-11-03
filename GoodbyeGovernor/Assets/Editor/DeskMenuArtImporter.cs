#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Scoped strictly to the six menu textures; no global import defaults are changed.
public sealed class DeskMenuArtImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        const string folder = "Assets/Resources/Menus/";
        if (!assetPath.StartsWith(folder, System.StringComparison.Ordinal)) return;
        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        bool button = name == "button_default" || name == "button_hover" || name == "button_pressed" || name == "button_disabled";
        if (!button && name != "PresidentialOffice" && name != "GreenLamp") return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = button ? TextureImporterType.Sprite : TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.crunchedCompression = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.isReadable = false;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = button;
        if (button)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.spriteBorder = new Vector4(400, 200, 400, 220);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
        }
        // Avoid an inherited platform override reducing these UI textures again.
        var desktop = importer.GetPlatformTextureSettings("Standalone");
        desktop.overridden = false;
        importer.SetPlatformTextureSettings(desktop);
    }
    public static void RunValidation() { ReimportMenuArt(); MenuValidationRunner.Run(); }

    public static void ReimportMenuArt()
    {
        foreach (var file in new[] { "PresidentialOffice", "GreenLamp", "button_default", "button_hover", "button_pressed", "button_disabled" })
            AssetDatabase.ImportAsset("Assets/Resources/Menus/" + file + ".png", ImportAssetOptions.ForceUpdate);
        Debug.Log("Menu UI import settings applied to six textures.");
    }
}
#endif
