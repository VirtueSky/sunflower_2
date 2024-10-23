﻿using UnityEditor;
using UnityEngine;
using VirtueSky.Ads;
using VirtueSky.Inspector;
using VirtueSky.UtilsEditor;

namespace VirtueSky.ControlPanel.Editor
{
    public class CPAdvertisingDrawer
    {
        private static Vector2 _scrollPosition;
        private static UnityEditor.Editor _editor;
        private static AdSettings _adSetting;
        private static Vector2 scroll = Vector2.zero;

        public static void OnEnable()
        {
            Init();
        }

        public static void Init()
        {
            if (_editor != null)
            {
                _editor = null;
            }

            _adSetting = CreateAsset.GetScriptableAsset<AdSettings>();
            _editor = UnityEditor.Editor.CreateEditor(_adSetting);
        }

        public static void OnDrawAdvertising(Rect position)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            CPUtility.DrawHeaderIcon(StatePanelControl.Advertising, "Advertising");
            GUILayout.Space(10);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (_adSetting == null)
            {
                if (GUILayout.Button("Create AdSetting"))
                {
                    _adSetting = CreateAsset.CreateAndGetScriptableAsset<VirtueSky.Ads.AdSettings>();
                    Init();
                }
            }
            else
            {
                if (_editor == null)
                {
                    EditorGUILayout.HelpBox("Couldn't create the settings resources editor.",
                        MessageType.Error);
                    return;
                }
                else
                {
                    _editor.OnInspectorGUI();
                }


                switch (AdSettings.CurrentAdNetwork)
                {
                    case AdNetwork.Max:
                        DrawMaxField(position);
                        break;
                    case AdNetwork.Admob:
                        DrawAdmobField(position);
                        break;
                    case AdNetwork.IronSource:
                        DrawIronSource(position);
                        break;
                }
            }

            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Ping Ads Settings");
            GUILayout.Space(10);
            if (GUILayout.Button("Ping"))
            {
                if (_adSetting == null)
                {
                    Debug.LogError("AdSetting have not been created yet");
                }
                else
                {
                    EditorGUIUtility.PingObject(_adSetting);
                    Selection.activeObject = _adSetting;
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        static void DrawMaxField(Rect position)
        {
            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Install Max Sdk");
            GUILayout.Space(10);
            if (GUILayout.Button("Install Max Sdk Plugin"))
            {
                AssetDatabase.ImportPackage(
                    FileExtension.GetPathFileInCurrentEnvironment(
                        "Module/Utils/Editor/UnityPackage/max-sdk.unitypackage"), false);
            }

            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Define symbols");
            GUILayout.Space(10);
#if !VIRTUESKY_ADS || !VIRTUESKY_APPLOVIN
            EditorGUILayout.HelpBox(
                $"Add scripting define symbols \"{ConstantDefineSymbols.VIRTUESKY_ADS}\" and \"{ConstantDefineSymbols.VIRTUESKY_APPLOVIN}\" to use Max Ads",
                MessageType.Info);
#endif

            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_ADS);
            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_APPLOVIN);
        }

        static void DrawAdmobField(Rect position)
        {
            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Install Admob Sdk");
            GUILayout.Space(10);
            if (GUILayout.Button("Install Admob Sdk Plugin"))
            {
                AssetDatabase.ImportPackage(
                    FileExtension.GetPathFileInCurrentEnvironment(
                        "Module/Utils/Editor/UnityPackage/google-mobile-ads.unitypackage"), false);
            }

            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Define symbols");
            GUILayout.Space(10);
#if !VIRTUESKY_ADS || !VIRTUESKY_ADMOB
            EditorGUILayout.HelpBox(
                $"Add scripting define symbols \"{ConstantDefineSymbols.VIRTUESKY_ADS}\" and \"{ConstantDefineSymbols.VIRTUESKY_ADMOB}\" to use Admob Ads",
                MessageType.Info);
#endif

            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_ADS);
            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_ADMOB);
        }

        private static void DrawIronSource(Rect position)
        {
            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Install IronSource Sdk");
            GUILayout.Space(10);
            if (GUILayout.Button("Install IronSource Sdk Plugin"))
            {
                AssetDatabase.ImportPackage(
                    FileExtension.GetPathFileInCurrentEnvironment(
                        "Module/Utils/Editor/UnityPackage/is-sdk.unitypackage"), false);
            }

            GUILayout.Space(10);
            CPUtility.GuiLine(2);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Define symbols");
            GUILayout.Space(10);
#if !VIRTUESKY_ADS || !VIRTUESKY_IRONSOURCE
            EditorGUILayout.HelpBox(
                $"Add scripting define symbols \"{ConstantDefineSymbols.VIRTUESKY_ADS}\" and \"{ConstantDefineSymbols.VIRTUESKY_IRONSOURCE}\" to use IronSource Ads",
                MessageType.Info);
#endif


            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_ADS);
            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_IRONSOURCE);
        }
    }
}