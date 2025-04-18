using UnityEditor;
using UnityEngine;
using VirtueSky.Tracking;
using VirtueSky.UtilsEditor;

namespace VirtueSky.ControlPanel.Editor
{
    public class CPAppsFlyerDrawer
    {
        private static AppsFlyerConfig config;
        private static UnityEditor.Editor _editor;
        private static Vector2 scroll = Vector2.zero;
        private static readonly string pathScriptableTracking = $"{FileExtension.DefaultRootPath}/Tracking_AppsFlyer";

        public static void OnEnable()
        {
            Init();
        }

        private static void Init()
        {
            if (_editor != null) _editor = null;
            config = CreateAsset.GetScriptableAsset<AppsFlyerConfig>();
            _editor = UnityEditor.Editor.CreateEditor(config);
        }

        public static void OnDrawAppsFlyer()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            CPUtility.DrawHeaderIcon(StatePanelControl.AppsFlyer, "AppsFlyer");
            GUILayout.Space(10);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            CPUtility.DrawButtonInstallPackage("Install AppsFlyer", "Remove AppsFlyer",
                ConstantPackage.PackageNameAppFlyer, ConstantPackage.MaxVersionAppFlyer);
            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
#if !VIRTUESKY_APPSFLYER
            EditorGUILayout.HelpBox(
                $"Add scripting define symbols: {ConstantDefineSymbols.VIRTUESKY_APPSFLYER} for AppsFlyer to use",
                MessageType.Info);
            GUILayout.Space(10);
#endif

            CPUtility.DrawHeader("Define Symbols");
            GUILayout.Space(10);
            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_APPSFLYER);
            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("AppsFlyer Config");
            GUILayout.Space(10);
            if (config == null)
            {
                if (GUILayout.Button("Create AppsFlyerConfig"))
                {
                    config =
                        CreateAsset.CreateAndGetScriptableAsset<AppsFlyerConfig>(isPingAsset: false);

                    Init();
                }
            }
            else
            {
                if (_editor == null)
                {
                    EditorGUILayout.HelpBox("Couldn't create the settings editor.",
                        MessageType.Error);
                    return;
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Set your devKey and appID to init the AppsFlyer SDK and start tracking. You must modify these fields and provide:\ndevKey - Your application devKey provided by AppsFlyer.\nappId - For iOS only. Your iTunes Application ID.\nUWP app id - For UWP only. Your application app id \nMac OS app id - For MacOS app only.",
                        MessageType.Info);
                    _editor.OnInspectorGUI();
                }
            }

            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("AppsFlyer Tracking");
            GUILayout.Space(10);
            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer No Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerNoParam>(pathScriptableTracking,
                    "tracking_appsflyer_no_param");
            }

            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer 1 Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerOneParam>(pathScriptableTracking,
                    "tracking_appsflyer_1_param");
            }

            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer 2 Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerTwoParam>(pathScriptableTracking,
                    "tracking_appsflyer_2_param");
            }

            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer 3 Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerThreeParam>(
                    pathScriptableTracking, "tracking_appsflyer_3_param");
            }

            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer 4 Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerFourParam>(pathScriptableTracking,
                    "tracking_appsflyer_5_param");
            }

            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer 5 Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerFiveParam>(pathScriptableTracking,
                    "tracking_appsflyer_5_param");
            }

            if (GUILayout.Button("Create Scriptable Tracking AppsFlyer Has Param"))
            {
                CreateAsset.CreateScriptableAssetsOnlyName<ScriptableTrackingAppsFlyerHasParam>(pathScriptableTracking,
                    "tracking_appsflyer_has_param");
            }

            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Ping AppsFlyerConfig");
            GUILayout.Space(10);
            if (GUILayout.Button("Ping"))
            {
                if (config == null)
                {
                    Debug.LogError("AppsFlyerConfig have not been created yet");
                }
                else
                {
                    EditorGUIUtility.PingObject(config);
                    Selection.activeObject = config;
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}