using UnityEditor;
using UnityEngine;

namespace Script.Editor
{
    /// <summary>
    /// Автоматично створює EventChannel для різних частин доби
    /// Automatically creates EventChannels for different times of day
    /// </summary>
    [InitializeOnLoad]
    public static class CreateTimeEventChannels
    {
        static CreateTimeEventChannels()
        {
            EditorApplication.delayCall += CreateAllEventChannelsIfNeeded;
        }

        /// <summary>
        /// Створює всі EventChannel, якщо вони ще не існують
        /// Creates all EventChannels if they don't exist yet
        /// </summary>
        private static void CreateAllEventChannelsIfNeeded()
        {
            // Створюємо EventChannel для всіх частин доби
            // Create EventChannels for all times of day
            CreateEventChannel<Morning>("Assets/Morning.asset");
            CreateEventChannel<Afternoon>("Assets/Afternoon.asset");
            CreateEventChannel<Evening>("Assets/Evening.asset");
            CreateEventChannel<Night>("Assets/Night.asset");
        }

        /// <summary>
        /// Створює EventChannel заданого типу, якщо він ще не існує
        /// Creates an EventChannel of the given type if it doesn't exist
        /// </summary>
        private static void CreateEventChannel<T>(string path) where T : ScriptableObject
        {
            // Перевіряємо чи вже існує
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return; // Вже існує, нічого не робимо / Already exists, do nothing
            }
            
            // Створюємо asset
            // Create the asset
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Auto-created {typeof(T).Name} EventChannel at {path}");
        }
        
        // Меню для ручного створення кожного EventChannel
        // Menu items for manually creating each EventChannel
        
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

        /// <summary>
        /// Створює EventChannel вручну через меню
        /// Creates an EventChannel manually through the menu
        /// </summary>
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

