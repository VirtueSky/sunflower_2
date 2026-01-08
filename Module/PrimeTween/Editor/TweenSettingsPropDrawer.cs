using JetBrains.Annotations;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using Mathf = UnityEngine.Mathf;

[CustomPropertyDrawer(typeof(TweenSettings))]
internal class TweenSettingsPropDrawer : PropertyDrawer {
    static GUIContent updateTypeGuiContent;

    public override float GetPropertyHeight([NotNull] SerializedProperty property, GUIContent label) {
        if (!property.isExpanded) {
            return EditorGUIUtility.singleLineHeight;
        }
        return getPropHeight(property);
    }

    internal static float getPropHeight([NotNull] SerializedProperty property) {
        var count = 1;
        count++; // duration
        count++; // ease
        var easeIndex = property.FindPropertyRelative(nameof(TweenSettings.ease)).intValue;
        if (easeIndex == (int)Ease.Custom) {
            count++; // customEase
        }
        count++; // cycles
        var cycles = property.FindPropertyRelative(nameof(TweenSettings.cycles)).intValue;
        if (cycles != 0 && cycles != 1) {
            count++; // cycleMode
        }
        count++; // startDelay
        count++; // endDelay
        count++; // useUnscaledTime
        count++; // useFixedUpdate
        var result = EditorGUIUtility.singleLineHeight * count + EditorGUIUtility.standardVerticalSpacing * (count - 1);
        result += EditorGUIUtility.standardVerticalSpacing * 4; // extra spacing
        return result;
    }

    public override void OnGUI(Rect position, [NotNull] SerializedProperty property, GUIContent label) {
        var rect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
        EditorGUI.PropertyField(rect, property, label);
        if (!property.isExpanded) {
            return;
        }
        moveToNextLine(ref rect);
        EditorGUI.indentLevel++;
        { // duration
            property.NextVisible(true);
            DrawDuration(rect, property);
            moveToNextLine(ref rect);
        }
        drawEaseTillEnd(property, ref rect);
        EditorGUI.indentLevel--;
    }

    internal static void DrawDuration(Rect rect, [NotNull] SerializedProperty property) { // p2 todo allow duration to be 0f in Inspector? need to change how defaultValue behaves below
        if (GUI.enabled) {
            ClampProperty(property, 1f);
        }
        EditorGUI.PropertyField(rect, property);
    }

    internal static void ClampProperty(SerializedProperty prop, float defaultValue, float min = 0.01f, float max = float.MaxValue) {
        prop.floatValue = prop.floatValue == 0f ? defaultValue : Mathf.Clamp(prop.floatValue, min, max);
    }

    internal static void drawEaseTillEnd([NotNull] SerializedProperty property, ref Rect rect) {
        DrawEaseAndCycles(property, ref rect);
        drawStartDelayTillEnd(ref rect, property);
    }

    internal static void DrawEaseAndCycles(SerializedProperty property, ref Rect rect, bool addSpace = true, bool draw = true, bool allowInfiniteCycles = true) {
        { // ease
            property.NextVisible(true);
            if (draw)
                EditorGUI.PropertyField(rect, property);
            moveToNextLine(ref rect);
            // customEase
            bool isCustom = property.intValue == (int) Ease.Custom;
            property.NextVisible(true);
            if (isCustom) {
                if (draw)
                    EditorGUI.PropertyField(rect, property);
                moveToNextLine(ref rect);
            } else {
                property.animationCurveValue = new AnimationCurve();
            }
        }
        if (addSpace) {
            rect.y += EditorGUIUtility.standardVerticalSpacing * 4;
        }
        { // cycles
            property.NextVisible(false);
            Assert.AreEqual(nameof(TweenSettings.cycles), property.name);
            var cycles = DrawCycles(rect, property, draw, allowInfiniteCycles);
            moveToNextLine(ref rect);
            {
                // cycleMode
                property.NextVisible(true);
                if (cycles != 0 && cycles != 1) {
                    if (draw)
                        EditorGUI.PropertyField(rect, property);
                    moveToNextLine(ref rect);
                }
            }
        }
    }

    internal static void drawStartDelayTillEnd(ref Rect rect, [NotNull] SerializedProperty property) {
        { // startDelay, endDelay
            for (int _ = 0; _ < 2; _++) {
                property.NextVisible(true);
                if (property.floatValue < 0f) {
                    property.floatValue = 0f;
                }
                EditorGUI.PropertyField(rect, property);
                moveToNextLine(ref rect);
            }
        }
        { // useUnscaledTime
            property.NextVisible(true);
            EditorGUI.PropertyField(rect, property);
            moveToNextLine(ref rect);
        }
        { // useFixedUpdate
            property.Next(false);
            bool useFixedUpdateObsolete = property.boolValue;
            var useFixedUpdateProp = property.Copy();

            // _updateType
            property.NextVisible(false);
            var current = (_UpdateType)property.enumValueIndex;
            if (useFixedUpdateObsolete && current != _UpdateType.FixedUpdate) {
                property.serializedObject.Update();
                property.enumValueIndex = (int)_UpdateType.FixedUpdate;
                property.serializedObject.ApplyModifiedProperties();
            } else {
                if (updateTypeGuiContent == null) {
                    updateTypeGuiContent = new GUIContent(property.displayName, property.tooltip);
                }
                GUIContent guiContent = EditorGUI.BeginProperty(rect, updateTypeGuiContent, property);
                EditorGUI.BeginChangeCheck();
                var newUpdateType = (_UpdateType)EditorGUI.EnumPopup(rect, guiContent, current);
                if (EditorGUI.EndChangeCheck())
                {
                    property.enumValueIndex = (int)newUpdateType;
                    useFixedUpdateProp.boolValue = newUpdateType == _UpdateType.FixedUpdate;
                }
                moveToNextLine(ref rect);
                EditorGUI.EndProperty();
            }
        }
    }

    internal static int ClampCycles(SerializedProperty property, bool allowInfiniteCycles = true) {
        int val = property.intValue;
        if (val == 0) {
            val = 1;
        } else if (val < 0) {
            val = allowInfiniteCycles ? -1 : 1;
        }
        property.intValue = val;
        return val;
    }

    internal static int DrawCycles(Rect rect, [NotNull] SerializedProperty property, bool draw = true, bool allowInfiniteCycles = true) {
        int val = ClampCycles(property, allowInfiniteCycles);
        if (draw)
            EditorGUI.PropertyField(rect, property);
        return val;
    }

    static void moveToNextLine(ref Rect rect) {
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}

[CustomPropertyDrawer(typeof(UpdateType))]
class UpdateTypePropDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        prop.Next(true);
        EditorGUI.PropertyField(pos, prop, label);
    }
}
