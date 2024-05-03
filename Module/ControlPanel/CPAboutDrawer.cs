﻿using System;
using UnityEditor;
using UnityEngine;

namespace VirtueSky.ControlPanel.Editor
{
    public class CPAboutDrawer
    {
        public static void OnDrawAbout(Rect position, Action drawSetting = null)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("ABOUT", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.TextArea("Name: Unity-Common", EditorStyles.boldLabel);
            GUILayout.TextArea(
                "Core singleton for building Unity games",
                EditorStyles.boldLabel);
            GUILayout.TextArea($"Version: {ConstantPackage.VersionUnityCommon}",
                EditorStyles.boldLabel);
            GUILayout.TextArea("Author: VirtueSky", EditorStyles.boldLabel);
            GUILayout.Space(10);
            if (GUILayout.Button("Open GitHub Repository"))
            {
                Application.OpenURL("https://github.com/wolf-package/unity-common");
            }

            // if (GUILayout.Button("Document"))
            // {
            //     Application.OpenURL("https://github.com/VirtueSky/sunflower/wiki");
            // }

            GUILayout.Space(10);
            CPUtility.DrawLineLastRectY(3, ConstantControlPanel.POSITION_X_START_CONTENT, position.width);
            // GUILayout.Space(20);
            // GUILayout.Label("SETUP THEME", EditorStyles.boldLabel);
            // GUILayout.Space(10);
            // drawSetting?.Invoke();
            GUILayout.EndVertical();
        }
    }
}