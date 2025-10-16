using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using System.Linq;
using UnityEditor.U2D;

public class SpriteAtlasGenerator
{
    [MenuItem("Assets/Sprite Atlas From Selection", false, 30)]
    private static void CreateSpriteAtlasFromSelection()
    {
        // 获取选中的纹理资源
        var selectedTextures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        if (selectedTextures.Length > 0)
        {
            CreateAtlasFromTextures(selectedTextures);
            return;
        }

        // 如果没有直接选中纹理，检查是否选中了文件夹
        var selectedFolders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets)
            .Where(asset => AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(asset)))
            .ToArray();

        if (selectedFolders.Length > 0)
        {
            CreateAtlasFromFolder(selectedFolders[0]);
            return;
        }

        EditorUtility.DisplayDialog("Error", "Please select either textures or a folder containing textures", "OK");
    }

    [MenuItem("Assets/Sprite Atlas From Selection", true)]
    private static bool ValidateCreateSpriteAtlasFromSelection()
    {
        // 验证菜单项是否可用（选中了纹理或文件夹）
        return Selection.GetFiltered<Texture2D>(SelectionMode.Assets).Length > 0 ||
               Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets)
                   .Any(asset => AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(asset)));
    }

    private static void CreateAtlasFromFolder(DefaultAsset folder)
    {
        string folderPath = AssetDatabase.GetAssetPath(folder);
        
        // 获取文件夹下所有纹理
        var texturePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(p => p.EndsWith(".png") || p.EndsWith(".jpg") || p.EndsWith(".jpeg"))
            .ToArray();

        if (texturePaths.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No valid textures found in the selected folder", "OK");
            return;
        }

        // 加载所有纹理
        var textures = texturePaths
            .Select(p => AssetDatabase.LoadAssetAtPath<Texture2D>(p))
            .Where(t => t != null)
            .ToArray();

        CreateAtlasFromTextures(textures, folderPath);
    }

    private static void CreateAtlasFromTextures(Texture2D[] textures, string targetPath = null)
    {
        if (textures.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No valid textures selected", "OK");
            return;
        }

        // 创建 Sprite Atlas
        var spriteAtlas = new SpriteAtlas();
        
        // 设置 Sprite Atlas 参数
        var packSettings = new SpriteAtlasPackingSettings()
        {
            blockOffset = 1,
            enableRotation = false,
            enableTightPacking = true,
            padding = 2
        };
        spriteAtlas.SetPackingSettings(packSettings);

        // 添加纹理到 Sprite Atlas
        foreach (var texture in textures)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                // 确保纹理类型设置为 Sprite
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    AssetDatabase.ImportAsset(path);
                }
                
                spriteAtlas.Add(new[] { AssetDatabase.LoadAssetAtPath<Object>(path) });
            }
        }

        // 确定保存路径
        string savePath;
        if (string.IsNullOrEmpty(targetPath))
        {
            savePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(textures[0]));
        }
        else
        {
            savePath = targetPath;
        }

        var atlasPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/NewSpriteAtlas.spriteatlas");
        
        AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Sprite Atlas created at {atlasPath}", "OK");
    }
}