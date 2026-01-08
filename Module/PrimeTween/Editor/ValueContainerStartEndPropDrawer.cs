using System;
using System.Runtime.CompilerServices;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using Mathf = UnityEngine.Mathf;
using TweenType = PrimeTween.TweenAnimation.TweenType;

[CustomPropertyDrawer(typeof(ValueContainerStartEnd))]
public class ValueContainerStartEndPropDrawer : PropertyDrawer {
    static readonly GUIContent _startFromCurrentToggleGuiContent = new GUIContent();

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        prop.Next(true);
        var tweenType = (TweenType)prop.enumValueIndex;
        prop.Next(false);
        return GetHeight(prop, label, tweenType);
    }

    internal static float GetHeight(SerializedProperty prop, GUIContent label, TweenType tweenType) {
        var propType = Utils.TweenTypeToTweenData(tweenType).Item1;
        Assert.AreNotEqual(PropType.None, propType);
        bool startFromCurrent = prop.boolValue;
        bool hasStartValue = !startFromCurrent;
        if (hasStartValue) {
            return GetSingleItemHeight(propType, label) * 2f + EditorGUIUtility.standardVerticalSpacing;
        }
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + GetSingleItemHeight(propType, label);
    }

    static float GetSingleItemHeight(PropType propType, GUIContent label) {
        return EditorGUI.GetPropertyHeight(ToSerializedPropType(), label);
        SerializedPropertyType ToSerializedPropType() {
            switch (propType) {
                case PropType.Double:
                case PropType.Float:
                    return SerializedPropertyType.Float;
                case PropType.Color:
                    return SerializedPropertyType.Color;
                case PropType.Vector2:
                    return SerializedPropertyType.Vector2;
                case PropType.Vector3:
                    return SerializedPropertyType.Vector3;
                case PropType.Vector4:
                case PropType.Quaternion:
                    return SerializedPropertyType.Vector4;
                case PropType.Rect:
                    return SerializedPropertyType.Rect;
                case PropType.Int:
                    return SerializedPropertyType.Integer;
                case PropType.None:
                default:
                    throw new Exception();
            }
        }
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        prop.Next(true);
        var tweenType = (TweenType)prop.enumValueIndex;
        prop.Next(false);
        Draw(ref pos, prop, tweenType);
    }

    internal static void Draw(ref Rect pos, SerializedProperty prop, TweenType tweenType, bool drawStartFromCurrentToggle = true, bool invert = true) {
        var propType = Utils.TweenTypeToTweenData(tweenType).Item1;
        Assert.AreNotEqual(PropType.None, propType);
        const float toggleWidth = 18f;
        EditorGUIUtility.labelWidth -= toggleWidth;

        // startFromCurrent toggle
        bool newStartFromCurrent = false;
        if (drawStartFromCurrentToggle)
        {
            var togglePos = new Rect(pos.x + 2, pos.y, toggleWidth - 2, EditorGUIUtility.singleLineHeight);
            var guiContent = EditorGUI.BeginProperty(togglePos, _startFromCurrentToggleGuiContent, prop); // p2 todo is it possible to display tooltip? tooltip is only displayed over the label, but I need to display it over the ToggleLeft
            EditorGUI.BeginChangeCheck();
            if (invert) {
                newStartFromCurrent = !EditorGUI.ToggleLeft(togglePos, guiContent, !prop.boolValue);
            } else {
                newStartFromCurrent = EditorGUI.ToggleLeft(togglePos, guiContent, prop.boolValue);
            }
            if (EditorGUI.EndChangeCheck()) {
                prop.boolValue = newStartFromCurrent;
            }
            EditorGUI.EndProperty();
        }

        pos.x += toggleWidth;
        pos.width -= toggleWidth;

        prop.NextVisible(nameof(ValueContainerStartEnd.startValue));
        using (new EditorGUI.DisabledScope(newStartFromCurrent ^ !invert)) {
            DrawValueContainer(ref pos, prop, propType);
        }

        pos.y += pos.height + EditorGUIUtility.standardVerticalSpacing;
        DrawValueContainer(ref pos, prop, propType);
        pos.y += pos.height + EditorGUIUtility.standardVerticalSpacing;

        pos.x -= toggleWidth;
        pos.width += toggleWidth;
    }

    static void DrawValueContainer(ref Rect pos, SerializedProperty prop, PropType propType) {
        var root = prop.Copy();
        prop.Next(true);
        TweenAnimation.ValueWrapper valueContainer = default;
        for (int i = 0; i < 4; i++) {
            valueContainer[i] = prop.floatValue;
            prop.Next(false);
        }
        var guiContent = new GUIContent(root.displayName, root.tooltip);
        pos.height = GetSingleItemHeight(propType, guiContent);
        guiContent = EditorGUI.BeginProperty(pos, guiContent, root);
        EditorGUI.BeginChangeCheck();
        TweenAnimation.ValueWrapper newVal = DrawField(pos);
        TweenAnimation.ValueWrapper DrawField(Rect position) {
            switch (propType) {
                case PropType.Float:
                    return EditorGUI.FloatField(position, guiContent, valueContainer.single).ToContainer();
                case PropType.Color:
                    return EditorGUI.ColorField(position, guiContent, valueContainer.color).ToContainer();
                case PropType.Vector2:
                    return EditorGUI.Vector2Field(position, guiContent, valueContainer.vector2).ToContainer();
                case PropType.Vector3:
                    return EditorGUI.Vector3Field(position, guiContent, valueContainer.vector3).ToContainer();
                case PropType.Vector4:
                case PropType.Quaternion: // p2 todo don't draw quaternion. Or draw it as Vector3 euler angles?
                    return EditorGUI.Vector4Field(position, guiContent, valueContainer.vector4).ToContainer();
                case PropType.Rect:
                    return EditorGUI.RectField(position, guiContent, valueContainer.rect).ToContainer();
                case PropType.Int:
                    var newIntVal = EditorGUI.IntField(position, guiContent, Mathf.RoundToInt(valueContainer.single));
                    return ((float)newIntVal).ToContainer();
                case PropType.Double: // should be used for display only. Unity serializes floats to text, not binary,so it's not possible to serialze two floats as one double
                    return EditorGUI.DoubleField(position, guiContent, valueContainer.DoubleVal).ToContainer();
                case PropType.None:
                default:
                    throw new Exception();
            }
        }
        if (EditorGUI.EndChangeCheck()) {
            root.Next(true);
            for (int i = 0; i < 4; i++) {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (root.floatValue != newVal[i]) {
                    root.floatValue = newVal[i];
                }
                root.Next(false);
            }
        }
        EditorGUI.EndProperty();
    }
}

internal static class SerializedPropertyExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void NextVisible(this SerializedProperty prop, string expectedName, bool enterChildren = false) {
        prop.NextVisible(enterChildren);
        #if PRIME_TWEEN_SAFETY_CHECKS
        Assert.AreEqual(expectedName, prop.name);
        #endif
    }
}
