using UnityEditor;
using UnityEngine;
using VirtueSky.UtilsEditor;

namespace VirtueSky.ControlPanel.Editor
{
    public static class CPInAppReviewDrawer
    {
        public static void OnDraw(Rect position)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            CPUtility.DrawHeaderIcon(StatePanelControl.InAppReview, "In App Review");
            GUILayout.Space(10);

            GUILayout.Space(10);
            CPUtility.DrawButtonInstallPackage("Install Google Play Review", "Remove Google Play Review",
                ConstantPackage.PackageNameGGPlayReview, ConstantPackage.MaxVersionGGPlayReview);
            CPUtility.DrawButtonInstallPackage("Install Google Play Core", "Remove Google Play Core",
                ConstantPackage.PackageNameGGPlayCore, ConstantPackage.MaxVersionGGPlayCore);
            CPUtility.DrawButtonInstallPackage("Install Google Play Common", "Remove Google Play Common",
                ConstantPackage.PackageNameGGPlayCommon, ConstantPackage.MaxVersionGGPlayCommon);
            CPUtility.DrawButtonInstallPackage("Install Google External Dependency Manager",
                "Remove Google External Dependency Manager",
                ConstantPackage.PackageNameGGExternalDependencyManager,
                ConstantPackage.MaxVersionGGExternalDependencyManager);

            GUILayout.Space(10);
            CPUtility.DrawLineLastRectY(3, ConstantControlPanel.POSITION_X_START_CONTENT, position.width);
            GUILayout.Space(10);
            CPUtility.DrawHeader("Define symbols");
            GUILayout.Space(10);
#if !VIRTUESKY_RATING
            EditorGUILayout.HelpBox(
                "Add scripting define symbols \"VIRTUESKY_RATING\" to use In App Review",
                MessageType.Info);
#endif

            CPUtility.DrawButtonAddDefineSymbols(ConstantDefineSymbols.VIRTUESKY_RATING);

            GUILayout.EndVertical();
        }
    }
}