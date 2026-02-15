using UnityEditor;
using UnityEngine;

namespace Script.Editor
{
    [InitializeOnLoad]
    public static class CreateTimeEventChannels
    {
        static CreateTimeEventChannels()
        {
            EditorApplication.delayCall += CreateAllEventChannelsIfNeeded;
        }
        
        private static void CreateAllEventChannelsIfNeeded()
        {
            CreateEventChannel<Morning>("Assets/Morning.asset");
            CreateEventChannel<Afternoon>("Assets/Afternoon.asset");
            CreateEventChannel<Evening>("Assets/Evening.asset");
            CreateEventChannel<Night>("Assets/Night.asset");
        }
        
        private static void CreateEventChannel<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return;
            }
            
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Auto-created {typeof(T).Name} EventChannel at {path}");
        }
        
        [MenuItem("Assets/Create/Behavior/Morning Event Channel")]
        public static void CreateMorningAsset()
        {
            CreateEventChannelManually<Morning>("Assets/Morning.asset");
        }

        [MenuItem("Assets/Create/Behavior/Afternoon Event Channel")]
        public static void CreateAfternoonAsset()
        {
            CreateEventChannelManually<Afternoon>("Assets/Afternoon.asset");
        }

        [MenuItem("Assets/Create/Behavior/Evening Event Channel")]
        public static void CreateEveningAsset()
        {
            CreateEventChannelManually<Evening>("Assets/Evening.asset");
        }

        [MenuItem("Assets/Create/Behavior/Night Event Channel")]
        public static void CreateNightAsset()
        {
            CreateEventChannelManually<Night>("Assets/Night.asset");
        }
        
        private static void CreateEventChannelManually<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                Debug.LogWarning($"{typeof(T).Name}.asset already exists at {path}");
                Selection.activeObject = existing;
                return;
            }
            
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = asset;
            Debug.Log($"Created {typeof(T).Name} EventChannel at {path}");
        }
    }
}

