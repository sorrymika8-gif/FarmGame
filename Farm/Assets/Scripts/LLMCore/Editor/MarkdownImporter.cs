using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

namespace GameLLM.Editor
{
    [ScriptedImporter(1, "md")]
    public class MarkdownImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var text = File.ReadAllText(ctx.assetPath);
            var asset = new TextAsset(text);
            ctx.AddObjectToAsset("main", asset);
            ctx.SetMainObject(asset);
        }
    }
}
