using UnityEditor;
using UnityEngine;

namespace ElDespacho.EditorTools
{
    /// <summary>
    /// Ajustes de importación para el pixel art del juego (Assets/Resources/Art/).
    /// Se aplican solos al añadir o reimportar una imagen en esa carpeta:
    /// Sprite, filtro Point (sin difuminar), sin compresión, sin mipmaps y 100 píxeles por unidad.
    /// Las piezas que se estiran (botones de opción y caja de diálogo) llevan sus bordes de 9-slice.
    /// No afecta a ninguna otra textura del proyecto (los menús de v0.7.1 tienen su propio importador).
    /// </summary>
    public class ElDespachoArtImporter : AssetPostprocessor
    {
        const string Folder = "Assets/Resources/Art/";

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(Folder)) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.npotScale = TextureImporterNPOTScale.None;

            // Bordes 9-slice (izquierda, abajo, derecha, arriba) medidos en píxeles del arte original
            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (name.StartsWith("UI_BotonOpcion"))
                ti.spriteBorder = new Vector4(10, 3, 3, 3);   // la flecha del estado seleccionado queda fuera del estirado
            else if (name == "UI_CajaDialogo")
                ti.spriteBorder = new Vector4(72, 10, 16, 14); // la pestaña del nombre y la flecha no se deforman
        }

        [MenuItem("Tools/El Despacho/Arte/Reimportar el arte del juego")]
        static void ReimportArt()
        {
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Art" }))
            {
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
                n++;
            }
            Debug.Log($"[Arte] {n} imágenes reimportadas con los ajustes de pixel art.");
        }
    }
}
