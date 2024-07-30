using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;

namespace VirtueSky.UtilsEditor
{
#if UNITY_EDITOR
    public static class FileExtension
    {
        public static T GetConfigFromFolder<T>(string path) where T : ScriptableObject
        {
            var fileEntries = Directory.GetFiles(path, ".", SearchOption.AllDirectories);

            foreach (var fileEntry in fileEntries)
                if (fileEntry.EndsWith(".asset"))
                {
                    var item =
                        AssetDatabase.LoadAssetAtPath<T>(fileEntry.Replace("\\", "/"));
                    if (item)
                        return item;
                }

            return null;
        }

        public static List<T> GetConfigsFromFolder<T>(string path) where T : ScriptableObject
        {
            var fileEntries = Directory.GetFiles(path, ".", SearchOption.AllDirectories);
            var result = new List<T>();
            foreach (var fileEntry in fileEntries)
                if (fileEntry.EndsWith(".asset"))
                {
                    var item = AssetDatabase.LoadAssetAtPath<T>(fileEntry.Replace("\\", "/"));

                    if (item)
                        result.Add(item);
                }

            if (result.Count > 0)
                return result;

            return null;
        }

        public static T GetConfigFromResource<T>(string path) where T : ScriptableObject
        {
            T config =
                Resources.Load<T>(path);
            if (config != null)
            {
                return config;
            }

            return null;
        }

        public static T GetPrefabFromFolder<T>(string path) where T : MonoBehaviour
        {
            var fileEntries = Directory.GetFiles(path, ".", SearchOption.AllDirectories);

            foreach (var fileEntry in fileEntries)
                if (fileEntry.EndsWith(".prefab"))
                {
                    var item =
                        AssetDatabase.LoadAssetAtPath<T>(fileEntry.Replace("\\", "/"));
                    if (item)
                        return item;
                }

            return null;
        }

        public static List<T> GetPrefabsFromFolder<T>(string path) where T : MonoBehaviour
        {
            var fileEntries = Directory.GetFiles(path, ".", SearchOption.AllDirectories);
            var result = new List<T>();
            foreach (var fileEntry in fileEntries)
                if (fileEntry.EndsWith(".prefab"))
                {
                    var item = AssetDatabase.LoadAssetAtPath<T>(fileEntry.Replace("\\", "/"));

                    if (item)
                        result.Add(item);
                }

            if (result.Count > 0)
                return result;

            return null;
        }

        public static T FindAssetWithPath<T>(string fullPath) where T : Object
        {
            string path = GetPathFileInCurrentEnvironment(fullPath);
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }

        public static T FindAssetWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }

        public static T[] FindAssetsWithPath<T>(string nameAsset, string relativePath)
            where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().ToArray();
            if (t.Length == 0) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t;
        }

        public static string AssetInPackagePath(string relativePath, string nameAsset)
        {
            return GetPathFileInCurrentEnvironment($"{relativePath}/{nameAsset}");
        }

        public static string GetPathFileInCurrentEnvironment(string fullRelativePath)
        {
            var upmPath = $"Packages/com.wolf-package.unity-common/{fullRelativePath}";
            var normalPath = $"Assets/unity-common/{fullRelativePath}";
            return !File.Exists(Path.GetFullPath(upmPath)) ? normalPath : upmPath;
        }

        public static string GetPathFolderInCurrentEnvironment(string fullRelativePath)
        {
            var upmPath = $"Packages/com.wolf-package.unity-common/{fullRelativePath}";
            var normalPath = $"Assets/unity-common/{fullRelativePath}";
            return Directory.Exists(upmPath) ? upmPath : normalPath;
        }

        public static string FormatJson(string json)
        {
            try
            {
                JToken parsedJson = JToken.Parse(json);
                return parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception)
            {
                return json;
            }
        }

        public static string ManifestPath => Application.dataPath + "/../Packages/manifest.json";

        public static void OpenFolderInExplorer(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                Debug.LogError("The directory does not exist: " + path);
            }
        }

        public static void OpenFolderInFinder(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start("open", path);
            }
            else
            {
                Debug.LogError("The directory does not exist: " + path);
            }
        }

        public static void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public const string DefaultResourcePath = "Assets/_Root/Resources";
        public const string DefaultRootPath = "Assets/_Root";
    }
#endif
}