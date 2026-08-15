using UnityEngine;
using UnityEditor;

public class SpriteAssetCreator : EditorWindow
{
    [MenuItem("Tools/Create Mech Sprites")]
    public static void CreateSprites()
    {
        string savePath = "Assets/Sprites/";
        
        // Player mech sprite
        Texture2D playerTex = PixelSpriteGenerator.GenerateMechTexture(Color.cyan, Color.blue, true);
        byte[] playerBytes = playerTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath + "PlayerMech.png", playerBytes);

        // Enemy mech sprite
        Texture2D enemyTex = PixelSpriteGenerator.GenerateMechTexture(Color.red, Color.magenta, false);
        byte[] enemyBytes = enemyTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath + "EnemyMech.png", enemyBytes);

        // Background sprite
        Texture2D bgTex = PixelSpriteGenerator.GenerateBackgroundTexture();
        byte[] bgBytes = bgTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath + "Background.png", bgBytes);

        AssetDatabase.ImportAsset("Assets/Sprites/PlayerMech.png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Sprites/EnemyMech.png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Sprites/Background.png", ImportAssetOptions.ForceUpdate);

        // Set texture import settings to point filter (pixel art)
        SetPixelArtSettings("Assets/Sprites/PlayerMech.png");
        SetPixelArtSettings("Assets/Sprites/EnemyMech.png");
        SetPixelArtSettings("Assets/Sprites/Background.png");

        AssetDatabase.SaveAssets();
        Debug.Log("Mech sprites created in Assets/Sprites/");
    }

    private static void SetPixelArtSettings(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 16;
            importer.SaveAndReimport();
        }
    }
}
