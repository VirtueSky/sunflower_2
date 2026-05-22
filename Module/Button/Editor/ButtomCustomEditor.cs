using UnityEditor;
using UnityEngine;
using VirtueSky.UIButton;
using VirtueSky.UtilsEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ButtonCustom), true)]
[CanEditMultipleObjects]
public class ButtomCustomEditor : UnityEditor.UI.ButtonEditor
{
    private ButtonCustom _buttonCustom;
    private SerializedProperty _invokeClickButton;
    private SerializedProperty _isMotion;
    private SerializedProperty _ease;
    private SerializedProperty _scale;
    private SerializedProperty _easingTypes;
    private SerializedProperty _isShrugOver;
    private SerializedProperty _timeShrug;
    private SerializedProperty _strength;
    private SerializedProperty _useSoundFx;
    private SerializedProperty _soundClickButton;

    protected override void OnEnable()
    {
        base.OnEnable();
        _buttonCustom = target as ButtonCustom;
        _invokeClickButton = serializedObject.FindProperty("invokeClickButton");
        _isMotion = serializedObject.FindProperty("isMotion");
        _easingTypes = serializedObject.FindProperty("easingTypes");
        _scale = serializedObject.FindProperty("scale");
        _isShrugOver = serializedObject.FindProperty("isShrugOver");
        _timeShrug = serializedObject.FindProperty("timeShrug");
        _strength = serializedObject.FindProperty("strength");
        _useSoundFx = serializedObject.FindProperty("useSoundFx");
        _soundClickButton = serializedObject.FindProperty("soundClickButton");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        GUILayout.Space(10);
        Uniform.DrawGroupFoldout("button_custom_setting", "Setting", DrawSetting, true);
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }

    void DrawSetting()
    {
        EditorGUILayout.PropertyField(_invokeClickButton);
        EditorGUILayout.PropertyField(_isMotion);
        if (_isMotion.boolValue)
        {
            EditorGUILayout.PropertyField(_easingTypes);
            EditorGUILayout.PropertyField(_scale);
            EditorGUILayout.PropertyField(_isShrugOver);
            if (_isShrugOver.boolValue)
            {
                EditorGUILayout.PropertyField(_timeShrug);
                EditorGUILayout.PropertyField(_strength);
            }
        }

        EditorGUILayout.PropertyField(_useSoundFx);
        if (_useSoundFx.boolValue)
        {
            EditorGUILayout.PropertyField(_soundClickButton);
        }
    }
}
#endif