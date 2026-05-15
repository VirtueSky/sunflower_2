using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VirtueSky.UtilsEditor
{
    public static class CreateAsset
    {
#if UNITY_EDITOR
        public static void CreateScriptableAssets<T>(string path = "", bool isPingAsset = true)
            where T : ScriptableObject
        {
            var setting = UnityEngine.ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{GetPath(path)}/{typeof(T).Name}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Selection.activeObject = setting;
            if (isPingAsset)
            {
                EditorGUIUtility.PingObject(setting);
            }

            Debug.Log(
                $"<color=Green>{typeof(T).Name} was created ad {GetPath(path)}/{typeof(T).Name}.asset</color>");
        }

        public static void CreateScriptableAssets<T>(string path = "", string name = "", bool isPingAsset = true)
            where T : ScriptableObject
        {
            string newName = name == "" ? typeof(T).Name : name;
            var setting = UnityEngine.ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{GetPath(path)}/{newName}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Selection.activeObject = setting;
            if (isPingAsset)
            {
                EditorGUIUtility.PingObject(setting);
            }

            Debug.Log(
                $"<color=Green>{newName} was created ad {GetPath(path)}/{newName}.asset</color>");
        }

        public static void CreateScriptableAssetsOnlyName<T>(string path = "", string name = "",
            bool isPingAsset = true)
            where T : ScriptableObject
        {
            int assetCounter = 0;
            string assetName = name == "" ? $"{typeof(T).Name}" : name;
            string assetPath = $"{GetPath(path)}/{assetName}.asset";

            while (AssetDatabase.LoadAssetAtPath<T>(assetPath) != null)
            {
                assetCounter++;
                assetPath =
                    $"{GetPath(path)}/{CreateNameBasedOnGameObjectNamingScheme(assetName, assetCounter)}.asset";
            }

            var setting = ScriptableObject.CreateInstance<T>();

            UnityEditor.AssetDatabase.CreateAsset(setting, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Selection.activeObject = setting;
            if (isPingAsset)
            {
                EditorGUIUtility.PingObject(setting);
            }

            Debug.Log(
                $"<color=Green>{typeof(T).Name} was created at {assetPath}</color>");
        }


        public static T CreateAndGetScriptableAsset<T>(string path = "", string assetName = "", bool isPingAsset = true)
            where T : ScriptableObject
        {
            var so = FileExtension.FindAssetAtFolder<T>(new string[] { "Assets" }).FirstOrDefault();
            if (so == null)
            {
                CreateScriptableAssets<T>(path, assetName, isPingAsset);
                so = FileExtension.FindAssetAtFolder<T>(new string[] { "Assets" }).FirstOrDefault();
            }

            return so;
        }

        public static T CreateAndGetScriptableAssetByName<T>(string path = "", string assetName = "", bool isPingAsset = true)
            where T : ScriptableObject
        {
            var so = GetScriptableAssetByName<T>(assetName);
            if (so == null)
            {
                CreateScriptableAssets<T>(path, assetName, isPingAsset);
                so = GetScriptableAssetByName<T>(assetName);
            }

            return so;
        }

        public static T GetScriptableAsset<T>() where T : ScriptableObject
        {
            return FileExtension.FindAssetAtFolder<T>(new string[] { "Assets" }).FirstOrDefault();
        }

        public static T GetScriptableAssetByName<T>(string name) where T : ScriptableObject
        {
            var arr = FileExtension.FindAssetAtFolder<T>(new string[] { "Assets" });
            foreach (var asset in arr)
            {
                if (asset.name == name) return asset;
            }

            return null;
        }

        // public enum NamingScheme
        // {
        //     /// <summary>
        //     ///   <para>Adds a space and a number in parenthesis to the name of an instantiated or duplicated GameObject ("Prefab (1)").</para>
        //     /// </summary>
        //     SpaceParenthesis,
        //     /// <summary>
        //     ///   <para>Adds a dot followed by a number to the name of an instantiated or duplicated GameObject ("Prefab.1").</para>
        //     /// </summary>
        //     Dot,
        //     /// <summary>
        //     ///   <para>Adds an underscore and a number to the name of an instantiated or duplicated GameObject ("Prefab_1").</para>
        //     /// </summary>
        //     Underscore,
        // }
        private static string CreateNameBasedOnGameObjectNamingScheme(string baseName, int counter)
        {
            EditorSettings.NamingScheme currentNamingScheme = EditorSettings.gameObjectNamingScheme;
            return currentNamingScheme switch
            {
                EditorSettings.NamingScheme.SpaceParenthesis => $"{baseName} ({counter})",
                EditorSettings.NamingScheme.Dot => $"{baseName}.{counter}",
                EditorSettings.NamingScheme.Underscore => $"{baseName}_{counter}",
                _ => $"{baseName} ({counter})"
            };
        }
#endif


        public static string GetPath(string path = "")
        {
            string pathTemp = "";
            if (path == "")
            {
                pathTemp = FileExtension.DefaultResourcePath;
            }
            else
            {
                pathTemp = path;
            }

            FileExtension.ValidatePath(pathTemp);

            return pathTemp;
        }
    }
}