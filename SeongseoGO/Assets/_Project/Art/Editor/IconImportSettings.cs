using UnityEditor;

namespace SeongseoGO.Art.Editor
{
    /// <summary>
    /// Art/Icons 폴더의 PNG는 자동으로 Sprite(2D and UI)로 가져온다.
    /// 나중에 진짜 아이콘을 같은 파일 이름으로 덮어써도 같은 설정이 유지된다.
    /// </summary>
    public class IconImportSettings : AssetPostprocessor
    {
        const string IconFolder = "Assets/_Project/Art/Icons/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(IconFolder) || !assetPath.EndsWith(".png")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
