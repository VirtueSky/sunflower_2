using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VirtueSky.UtilsEditor;

namespace VirtueSky.Iap
{
    [CustomEditor(typeof(IapSettings), true)]
    public class IapSettingsEditor : Editor
    {
        private IapSettings _iapSettings;

        //private SerializedProperty _runtimeInitType;

        // private SerializedProperty _skusData;
        // private SerializedProperty _products;
        private SerializedProperty _isValidatePurchase;
        private SerializedProperty _isCustomValidatePurchase;
        private SerializedProperty _validatePurchase;
        private SerializedProperty _googlePlayStoreKey;

        void Init()
        {
            _iapSettings = target as IapSettings;
            //_runtimeInitType = serializedObject.FindProperty("runtimeInitType");
            // _skusData = serializedObject.FindProperty("skusData");
            // _products = serializedObject.FindProperty("products");
            _isValidatePurchase = serializedObject.FindProperty("isValidatePurchase");
            _isCustomValidatePurchase = serializedObject.FindProperty("isCustomValidatePurchase");
            _validatePurchase = serializedObject.FindProperty("validatePurchase");
            _googlePlayStoreKey = serializedObject.FindProperty("googlePlayStoreKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Init();
            GUILayout.Space(10);

            //EditorGUILayout.PropertyField(_runtimeInitType);

            //GUILayout.Space(10);
            // EditorGUILayout.PropertyField(_skusData);
            // EditorGUILayout.PropertyField(_products);
            // GUILayout.Space(10);
            // if (GUILayout.Button("Generate Product From SkusData"))
            // {
            //     GenerateProductImpl();
            // }

            GUILayout.Space(10);
            GuiLine(2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_isValidatePurchase);
            if (_isValidatePurchase.boolValue)
            {
                EditorGUILayout.PropertyField(_googlePlayStoreKey);
                GUILayout.Space(10);
                if (GUILayout.Button("Obfuscator Key"))
                {
                    ObfuscatorKeyImpl();
                }

                GUILayout.Space(10);
                EditorGUILayout.PropertyField(_isCustomValidatePurchase);
                if (_isCustomValidatePurchase.boolValue)
                {
                    EditorGUILayout.PropertyField(_validatePurchase);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        // void GenerateProductImpl()
        // {
        //     IapSettings.Products.Clear();
        //     var products = IapSettings.Products;
        //     var skusData = IapSettings.SkusData;
        //     foreach (var data in skusData)
        //     {
        //         Debug.Log($"{data.androidId}");
        //         bool isCustomName = false;
        //         string itemName = data.Id.Split('.').Last();
        //         if (!string.IsNullOrEmpty(data.customProductName))
        //         {
        //             isCustomName = true;
        //             itemName = data.customProductName;
        //         }
        //
        //         var product = CreateAsset.CreateAndGetScriptableAssetByName<IapDataProduct>($"{FileExtension.DefaultRootPath}/Iap/Products",
        //             isCustomName ? $"{itemName.ToLower()}" : $"iap_{itemName.ToLower()}");
        //         product.Init(data.androidId, data.iosId, data.productType);
        //         Debug.Log($"{product.androidId}");
        //         products.Add(product);
        //     }
        //
        //     serializedObject.ApplyModifiedProperties();
        //     EditorUtility.SetDirty(target);
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        // }

        void ObfuscatorKeyImpl()
        {
            var googleError = "";
            var appleError = "";
            ObfuscationGenerator.ObfuscateSecrets(includeGoogle: true,
                appleError: ref googleError,
                googleError: ref appleError,
                googlePlayPublicKey: _iapSettings.GooglePlayStoreKey);
            string pathAsmdef =
                FileExtension.GetPathFileInCurrentEnvironment(
                    $"Module/Utils/Editor/TempAssembly/PurchasingGeneratedAsmdef.txt");
            string pathAsmdefMeta =
                FileExtension.GetPathFileInCurrentEnvironment(
                    $"Module/Utils/Editor/TempAssembly/PurchasingGeneratedAsmdefMeta.txt");
            var asmdef = (TextAsset)AssetDatabase.LoadAssetAtPath(pathAsmdef, typeof(TextAsset));
            var meta = (TextAsset)AssetDatabase.LoadAssetAtPath(pathAsmdefMeta, typeof(TextAsset));
            string path = Path.Combine(TangleFileConsts.k_OutputPath, "Wolf.Purchasing.Generate.asmdef");
            string pathMeta = Path.Combine(TangleFileConsts.k_OutputPath, "Wolf.Purchasing.Generate.asmdef.meta");
            if (!File.Exists(path))
            {
                var writer = new StreamWriter(path, false);
                writer.Write(asmdef.text);
                writer.Close();
            }

            if (!File.Exists(pathMeta))
            {
                var writer = new StreamWriter(pathMeta, false);
                writer.Write(meta.text);
                writer.Close();
            }

            AssetDatabase.ImportAsset(path);
        }

        void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, new Color32(0, 0, 0, 255));
        }
    }
}