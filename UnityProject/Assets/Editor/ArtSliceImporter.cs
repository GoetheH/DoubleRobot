#if UNITY_EDITOR
using UnityEditor;
public class ArtSliceImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/Resources/ArtSlice/")) return;
        var importer=(TextureImporter)assetImporter;
        importer.textureType=TextureImporterType.Default;
        importer.filterMode=UnityEngine.FilterMode.Point;
        importer.mipmapEnabled=false;
        importer.npotScale=TextureImporterNPOTScale.None;
        importer.maxTextureSize=2048;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
    }
}
#endif
