﻿using System;
using UnityEditor;
using UnityEngine;
using VirtueSky.DataStorage;
using VirtueSky.Inspector;


namespace VirtueSky.ControlPanel.Editor
{
    public class ControlPanelWindowEditor : EditorWindow
    {
        private StatePanelControl statePanelControl;
        private bool isFieldMax = false;
        private bool isFielAdmob = false;
        private string inputPackageFullNameAdd = "";
        private string inputPackageFullNameRemove = "";
        private Vector2 scrollButton = Vector2.zero;

        [MenuItem("Unity-Common/Control Panel &1", false, priority = 1)]
        public static void ShowPanelControlWindow()
        {
            ControlPanelWindowEditor window =
                GetWindow<ControlPanelWindowEditor>("Unity-Common Control Panel");
            if (window == null)
            {
                Debug.LogError("Couldn't open the window!");
                return;
            }

            window.minSize = new Vector2(600, 300);
            window.Show();
        }

        private void OnEnable()
        {
            statePanelControl = StatePanelControl.About;
            CPAdvertisingDrawer.OnEnable();
            CPIapDrawer.OnEnable();
        }

        private void OnDisable()
        {
            CPLevelEditorDrawer.OnDisable();
        }

        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height),
                GameDataEditor.ColorBackgroundRectWindowSunflower.ToColor());
            GUILayout.Space(10);
            GUI.contentColor = GameDataEditor.ColorTextContentWindowSunflower.ToColor();
            GUILayout.Label("UNITY-COMMON CONTROL PANEL", EditorStyles.boldLabel);
            GUI.backgroundColor = GameDataEditor.ColorContentWindowSunflower.ToColor();
            Handles.color = Color.black;
            Handles.DrawAAPolyLine(4, new Vector3(0, 30), new Vector3(position.width, 30));
            // GuiLine(2, Color.black);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(ConstantControlPanel.WIDTH_CONTENT_BUTTON_STATE_CONTROL_PANEL));
            scrollButton = EditorGUILayout.BeginScrollView(scrollButton);
            DrawButton();
            EditorGUILayout.EndScrollView();
            Handles.DrawAAPolyLine(4, new Vector3(ConstantControlPanel.POSITION_X_START_CONTENT, 0),
                new Vector3(210, position.height));
            GUILayout.EndVertical();
            DrawContent();
            GUILayout.EndHorizontal();
        }

        void DrawButton()
        {
            DrawButtonChooseState("Advertising", StatePanelControl.Advertising);
            DrawButtonChooseState("In App Purchase", StatePanelControl.InAppPurchase);
            DrawButtonChooseState("Audio", StatePanelControl.Audio);
            DrawButtonChooseState("Assets Finder", StatePanelControl.AssetsFinder);
            DrawButtonChooseState("Level Editor", StatePanelControl.LevelEditor, () => CPLevelEditorDrawer.OnEnable());
            DrawButtonChooseState("Firebase", StatePanelControl.Firebase);
            DrawButtonChooseState("Game Service", StatePanelControl.GameService);
            DrawButtonChooseState("Hierarchy", StatePanelControl.Hierarchy);
            DrawButtonChooseState("Scripting Define Symbols", StatePanelControl.ScriptDefineSymbols);
            DrawButtonChooseState("Register Package", StatePanelControl.RegisterPackage);
            DrawButtonChooseState("Extensions", StatePanelControl.Extensions);
            DrawButtonChooseState("About", StatePanelControl.About);
        }

        void DrawContent()
        {
            switch (statePanelControl)
            {
                case StatePanelControl.Advertising:
                    CPAdvertisingDrawer.OnDrawAdvertising(position);
                    break;
                case StatePanelControl.InAppPurchase:
                    CPIapDrawer.OnDrawIap(position);
                    break;
                case StatePanelControl.AssetsFinder:
                    CPAssetFinderDrawer.OnDrawAssetUsageDetector();
                    break;
                case StatePanelControl.Audio:
                    CPAudioDrawer.OnDrawAudio(position);
                    break;
                case StatePanelControl.LevelEditor:
                    CPLevelEditorDrawer.OnDrawLevelEditor(position);
                    break;
                case StatePanelControl.Firebase:
                    CPFirebaseDrawer.OnDrawFirebase(position);
                    break;
                case StatePanelControl.ScriptDefineSymbols:
                    CPScriptingDefineSymbolsDrawer.OnDrawScriptingDefineSymbols();
                    break;
                case StatePanelControl.RegisterPackage:
                    CPRegisterPackageDrawer.OnDrawRegisterPackageByManifest(position);
                    break;
                case StatePanelControl.Hierarchy:
                    CPHierarchyDrawer.OnDrawQHierarchyEvent(position, this);
                    break;
                case StatePanelControl.GameService:
                    CPGameServiceDrawer.OnDrawGameService();
                    break;
                case StatePanelControl.Extensions:
                    CPExtensionsDrawer.OnDrawExtensions(position);
                    break;
                case StatePanelControl.About:
                    CPAboutDrawer.OnDrawAbout(position);
                    break;
            }
        }

        #region Setup theme color

        void OnSettingColorTheme()
        {
            GameDataEditor.ColorContentWindowSunflower =
                (CustomColor)EditorGUILayout.EnumPopup("Color Content:",
                    GameDataEditor.ColorContentWindowSunflower);
            GameDataEditor.ColorTextContentWindowSunflower =
                (CustomColor)EditorGUILayout.EnumPopup("Color Text Content:",
                    GameDataEditor.ColorTextContentWindowSunflower);
            GameDataEditor.ColorBackgroundRectWindowSunflower =
                (CustomColor)EditorGUILayout.EnumPopup("Color Background:",
                    GameDataEditor.ColorBackgroundRectWindowSunflower);
            GUILayout.Space(10);
            if (GUILayout.Button("Theme Default"))
            {
                GameDataEditor.ColorContentWindowSunflower = CustomColor.Bright;
                GameDataEditor.ColorTextContentWindowSunflower = CustomColor.Gold;
                GameDataEditor.ColorBackgroundRectWindowSunflower = CustomColor.DarkSlateGray;
            }
        }

        #endregion

        void DrawButtonChooseState(string title, StatePanelControl _statePanelControlTab, Action OnCompleted = null)
        {
            bool clicked = GUILayout.Toggle(_statePanelControlTab == statePanelControl, title, GUI.skin.button,
                GUILayout.ExpandWidth(true));
            if (clicked && statePanelControl != _statePanelControlTab)
            {
                statePanelControl = _statePanelControlTab;
                OnCompleted?.Invoke();
            }
        }
    }

    public enum StatePanelControl
    {
        Advertising,
        InAppPurchase,
        AssetsFinder,
        Audio,
        Firebase,
        LevelEditor,
        ScriptDefineSymbols,
        RegisterPackage,
        Hierarchy,
        GameService,
        Extensions,
        About,
    }
}