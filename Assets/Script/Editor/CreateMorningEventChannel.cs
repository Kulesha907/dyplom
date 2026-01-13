using UnityEditor;
using UnityEngine;

namespace Script.Editor
{
    [InitializeOnLoad]
    public static class CreateMorningEventChannel
    {
        static CreateMorningEventChannel()
        {
            EditorApplication.delayCall += CreateMorningAssetIfNeeded;
        }

        private static void CreateMorningAssetIfNeeded()
        {
            string path = "Assets/Morning.asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<Morning>(path);
            if (existing != null)
            {
                return; // Already exists, do nothing
            }
            
            // Create the asset
            var asset = ScriptableObject.CreateInstance<Morning>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Auto-created Morning EventChannel at " + path);
        }
        
        [MenuItem("Assets/Create/Behavior/Morning Event Channel")]
        public static void CreateMorningAsset()
        {
            var asset = ScriptableObject.CreateInstance<Morning>();
            
            string path = "Assets/Morning.asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<Morning>(path);
            if (existing != null)
            {
                Debug.LogWarning("Morning.asset already exists at " + path);
                Selection.activeObject = existing;
                return;
            }
            
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = asset;
            
            Debug.Log("Created Morning EventChannel at " + path);
        }
    }
}

