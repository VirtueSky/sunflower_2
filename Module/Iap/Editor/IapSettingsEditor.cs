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
        private SerializedProperty _runtimeAutoInit;
        private SerializedProperty _runtimeAutoInitType;
        private SerializedProperty _iapDataProducts;
        private SerializedProperty _isValidatePurchase;
        private SerializedProperty _googlePlayStoreKey;

        void Init()
        {
            _iapSettings = target as IapSettings;
            _runtimeAutoInit = serializedObject.FindProperty("runtimeAutoInit");
            _runtimeAutoInitType = serializedObject.FindProperty("runtimeAutoInitType");
            _iapDataProducts = serializedObject.FindProperty("iapDataProducts");
            _isValidatePurchase = serializedObject.FindProperty("isValidatePurchase");
            _googlePlayStoreKey = serializedObject.FindProperty("googlePlayStoreKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Init();
            // EditorGUILayout.LabelField("IAP SETTING", EditorStyles.boldLabel);
            // GuiLine(2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_runtimeAutoInit);
            if (_runtimeAutoInit.boolValue)
            {
                EditorGUILayout.PropertyField(_runtimeAutoInitType);
            }

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_iapDataProducts);
            GUILayout.Space(10);
            if (GUILayout.Button("Generate Product"))
            {
                GenerateProductImpl();
            }

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
            }

            serializedObject.ApplyModifiedProperties();
        }

        private const string pathDefaultScript = "Assets/_Root/Scripts";

        void GenerateProductImpl()
        {
            FileExtension.ValidatePath(pathDefaultScript);
            var productImplPath = $"{pathDefaultScript}/IapProduct.cs";
            var str = "namespace VirtueSky.Iap\n{";
            str += "\n\tpublic struct IapProduct\n\t{";
            str += "\n";
            var iapDataProducts = IapSettings.IapDataProducts;
            for (int i = 0; i < iapDataProducts.Length; i++)
            {
                var androidItemName = !string.IsNullOrEmpty(iapDataProducts[i].androidId) 
                    ? iapDataProducts[i].androidId.Split('.').Last() 
                    : "";
                var iosItemName = !string.IsNullOrEmpty(iapDataProducts[i].iOSId) 
                    ? iapDataProducts[i].iOSId.Split('.').Last() 
                    : "";

                // Android Section
                if (!string.IsNullOrEmpty(iapDataProducts[i].androidId))
                {
                    str += "#if UNITY_ANDROID\n";
                    str += $"\t\t// {androidItemName.ToUpper()} - ANDROID";
                    str += $"\n\t\tpublic const string ID_{androidItemName.ToUpper()} = \"{iapDataProducts[i].androidId}\";";

                    str +=
                        $"\n\t\tpublic static IapDataProduct Purchase{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(androidItemName)}() => IapManager.PurchaseProduct(IapSettings.IapDataProducts[{i}]);";

                    str +=
                        $"\n\t\tpublic static bool IsPurchased{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(androidItemName)}() => IapManager.IsPurchasedProduct(IapSettings.IapDataProducts[{i}]);";

                    str +=
                        $"\n\t\tpublic static UnityEngine.Purchasing.Product GetProduct{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(androidItemName)}() => IapManager.GetProduct(IapSettings.IapDataProducts[{i}]);";

                    str += $"\n\t\tpublic static float PriceConfig{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(androidItemName)}() => IapSettings.IapDataProducts[{i}].price;";
                    if (iapDataProducts[i].iapProductType == IapProductType.Subscription)
                    {
                        str +=
                            $"\n\t\tpublic static UnityEngine.Purchasing.SubscriptionInfo GetSubscriptionInfo{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(androidItemName)}() => IapManager.GetSubscriptionInfo(IapSettings.IapDataProducts[{i}]);";
                    }

                    str += "\n#endif\n";
                }

                // iOS Section
                if (!string.IsNullOrEmpty(iapDataProducts[i].iOSId))
                {
                    str += "#if UNITY_IOS\n";
                    str += $"\t\t// {iosItemName.ToUpper()} - iOS";
                    str += $"\n\t\tpublic const string ID_{iosItemName.ToUpper()} = \"{iapDataProducts[i].iOSId}\";";

                    str +=
                        $"\n\t\tpublic static IapDataProduct Purchase{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(iosItemName)}() => IapManager.PurchaseProduct(IapSettings.IapDataProducts[{i}]);";

                    str +=
                        $"\n\t\tpublic static bool IsPurchased{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(iosItemName)}() => IapManager.IsPurchasedProduct(IapSettings.IapDataProducts[{i}]);";

                    str +=
                        $"\n\t\tpublic static UnityEngine.Purchasing.Product GetProduct{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(iosItemName)}() => IapManager.GetProduct(IapSettings.IapDataProducts[{i}]);";

                    str += $"\n\t\tpublic static float PriceConfig{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(iosItemName)}() => IapSettings.IapDataProducts[{i}].price;";
                    if (iapDataProducts[i].iapProductType == IapProductType.Subscription)
                    {
                        str +=
                            $"\n\t\tpublic static UnityEngine.Purchasing.SubscriptionInfo GetSubscriptionInfo{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(iosItemName)}() => IapManager.GetSubscriptionInfo(IapSettings.IapDataProducts[{i}]);";
                    }

                    str += "\n#endif\n";
                }

                str += "\n";
            }

            str += "\n\t}";
            str += "\n}";

            var writer = new StreamWriter(productImplPath, false);
            writer.Write(str);
            writer.Close();
            AssetDatabase.ImportAsset(productImplPath);
        }

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