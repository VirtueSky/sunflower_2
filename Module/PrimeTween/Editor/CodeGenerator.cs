using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;
using TweenType = PrimeTween.TweenAnimation.TweenType;

internal class CodeGenerator : ScriptableObject {
    [SerializeField] MonoScript methodsScript;
    [SerializeField] MonoScript dotweenMethodsScript;
    [SerializeField] MonoScript tweenComponentScript;
    [SerializeField] MonoScript editorUtilsScript;
    [SerializeField] MethodGenerationData[] methodsData;
    [SerializeField] AdditiveMethodsGenerator additiveMethodsGenerator;
    [SerializeField] SpeedBasedMethodsGenerator speedBasedMethodsGenerator;

    /*void OnEnable() {
        #if PRIME_TWEEN_SAFETY_CHECKS
        foreach (var data in methodsData) {
            data.description = "";
            var methodPrefix = getMethodPrefix(data.dependency);
            if (!string.IsNullOrEmpty(methodPrefix)) {
                data.description += methodPrefix + "_";
            }
            data.description = data.methodName + "_" + getTypeByName(data.targetType).Name;
        }
        #endif
    }*/

    [Serializable]
    class AdditiveMethodsGenerator {
        [SerializeField] AdditiveMethodsGeneratorData[] additiveMethods;

        [Serializable]
        class AdditiveMethodsGeneratorData {
            [SerializeField] internal string methodName;
            [SerializeField] internal PropType propertyType;
            [SerializeField] internal string setter;
        }

        [NotNull]
        internal string Generate() {
            string result = @"
#if PRIME_TWEEN_EXPERIMENTAL";
            foreach (var data in additiveMethods) {
                const string template = @"        
        public static Tween PositionAdditive([NotNull] UnityEngine.Transform target, Single deltaValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => PositionAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween PositionAdditive([NotNull] UnityEngine.Transform target, Single deltaValue, float duration, Easing ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => PositionAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween PositionAdditive([NotNull] UnityEngine.Transform target, Single deltaValue, TweenSettings settings) 
            => CustomAdditive(target, deltaValue, settings, (_target, delta) => additiveTweenSetter());
";
                result += template.Replace("Single", data.propertyType.ToFullTypeName())
                    .Replace("PositionAdditive", data.methodName)
                    .Replace("additiveTweenSetter()", data.setter);
            }
            result += "#endif";
            return result;
        }
    }

    [ContextMenu(nameof(Generate))]
    internal void Generate() {
        #if !PRIME_TWEEN_EXPERIMENTAL
        throw new Exception("add PRIME_TWEEN_EXPERIMENTAL to defines");
        #endif
        generateMethods();
        generateDotweenMethods();
    }

    const string generatorBeginLabel = "// CODE GENERATOR BEGIN";

    void GenerateTweenComponent(Dictionary<MethodGenerationData, string> methodDataToEnumName, List<string> manualTweenTypes) {
        if (tweenComponentScript == null) {
            Debug.LogError("Not generating TweenAnimationComponent script because this component is only available in PrimeTween PRO.");
            return;
        }

        string str = tweenComponentScript.text;
        int searchIndex = str.IndexOf(generatorBeginLabel, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, searchIndex);
        str = str.Substring(0, searchIndex + generatorBeginLabel.Length) + "\n";

        var generationData = methodsData
            .GroupBy(_ => _.dependency)
            .SelectMany(x => x)
            .SkipWhile(x => x.description != "Range_Light")
            .ToArray();

        Dependency dependency = Dependency.None;
        for (var i = 0; i < generationData.Length; i++) {
            var data = generationData[i];
            if (dependency != data.dependency) {
                if (shouldWrapInDefine(dependency)) {
                    str += "                    #endif\n";
                }
                dependency = data.dependency;
                if (shouldWrapInDefine(dependency)) {
                    switch (dependency) {
                        case Dependency.PRIME_TWEEN_EXPERIMENTAL:
                        case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
                        case Dependency.TEXT_MESH_PRO_INSTALLED:
                            str += $"                    #if {dependency}\n";
                            break;
                        default:
                            str += $"                    #if !UNITY_2019_1_OR_NEWER || {dependency}\n";
                            break;
                    }
                }
            }
            if (!methodDataToEnumName.TryGetValue(data, out string tweenType)) {
                continue;
            }
            if (manualTweenTypes.Contains(tweenType)) {
                continue;
            }
            if (data.dependency == Dependency.UI_ELEMENTS_MODULE_INSTALLED || data.targetType.Contains("PrimeTween.")) { // skip both TweenTimeScale
                continue;
            }
            Assert.IsTrue(tweenType.Contains(data.methodName), $"{i}, {tweenType}, {data.methodName}");

            var template = "                    case TweenType.Position: { return GetUnityTarget<Transform>(out var tweenTarget, ref err) ? Tween.Position(tweenTarget, settingsVector3) : default; }";
            template = template.Replace("TweenType.Position", $"TweenType.{tweenType}");
            template = template.Replace("Transform", getTypeByName(data.targetType).Name);
            template = template.Replace("Tween.Position", $"Tween.{getPrefix()}{data.methodName}");
            template = template.Replace("settingsVector3", $"settings{data.propertyType}");
            str += template + "\n";

            string getPrefix() => data.placeInGlobalScope ? null : getMethodPrefix(data.dependency);
        }
        if (shouldWrapInDefine(dependency)) {
            str += "                    #endif\n";
        }
        str += @"                    default:
                        throw new Exception();
                }
            }
        }
    }
}
";
        SaveScript(tweenComponentScript, str);
    }

    static void SaveScript(MonoScript script, string text) {
        var path = AssetDatabase.GetAssetPath(script);
        if (text == File.ReadAllText(path)) {
            return;
        }
        File.WriteAllText(path, text);
        EditorUtility.SetDirty(script);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    void generateDotweenMethods() {
        // p2 todo combine adapter files into one file
        var str = @"#if PRIME_TWEEN_DOTWEEN_ADAPTER
// This file is generated by CodeGenerator.cs
using JetBrains.Annotations;
using System;

namespace PrimeTween {
    [PublicAPI]
    public static partial class DOTweenAdapter {";
        const string dotweenOverload = "        public static Tween DOTWEEN_METHOD_NAME([NotNull] this UnityEngine.Camera target, Single endValue, float duration) => Tween.METHOD_NAME(target, endValue, duration);";
        str += generateWithDefines(data => {
            if (!data.dotweenMethodName.Any()) {
                return string.Empty;
            }
            Assert.IsTrue(data.dotweenMethodName.Any());
            string result = "";
            result += "\n";
            result += populateTemplate(dotweenOverload.Replace("DOTWEEN_METHOD_NAME", data.dotweenMethodName), data);
            return result;
        });
        str += @"
    }
}
#endif";
        SaveScript(dotweenMethodsScript, str);
    }

    [CanBeNull]
    static string getMethodPrefix(Dependency dep) {
        switch (dep) {
            case Dependency.UNITY_UGUI_INSTALLED:
                return "UI";
            case Dependency.AUDIO_MODULE_INSTALLED:
                return "Audio";
            case Dependency.PHYSICS_MODULE_INSTALLED:
            case Dependency.PHYSICS2D_MODULE_INSTALLED:
                return nameof(Rigidbody);
            case Dependency.None:
            case Dependency.PRIME_TWEEN_EXPERIMENTAL:
            case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
            case Dependency.TEXT_MESH_PRO_INSTALLED:
                return null;
        }
        return dep.ToString();
    }

    static bool shouldWrapInDefine(Dependency d) {
        switch (d) {
            case Dependency.UNITY_UGUI_INSTALLED:
            case Dependency.AUDIO_MODULE_INSTALLED:
            case Dependency.PHYSICS_MODULE_INSTALLED:
            case Dependency.PHYSICS2D_MODULE_INSTALLED:
            case Dependency.PRIME_TWEEN_EXPERIMENTAL:
            case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
            case Dependency.TEXT_MESH_PRO_INSTALLED:
                return true;
        }
        return false;
    }

    const string overloadTemplateTo =
@"        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => METHOD_NAME(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single startValue, Single endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => METHOD_NAME(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        [EditorBrowsable(EditorBrowsableState.Never)] [Obsolete(ObsoleteMessages.tweenSettingsOverloadEndValue)]
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single endValue, TweenSettings settings) => METHOD_NAME(target, new TweenSettings<float>(endValue, settings));
        [EditorBrowsable(EditorBrowsableState.Never)] [Obsolete(ObsoleteMessages.tweenSettingsOverloadStartEndValue)]
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single startValue, Single endValue, TweenSettings settings) => METHOD_NAME(target, new TweenSettings<float>(startValue, endValue, settings));";
    const string fullTemplate =
@"        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, TweenSettings<float> settings)
            => animate(target, ref settings, TweenType.CameraOrthographicSize);";

    void generateMethods() {
        var text = methodsScript.text;
        var searchIndex = text.IndexOf(generatorBeginLabel, StringComparison.Ordinal);
        Assert.AreNotEqual(-1, searchIndex);
        text = text.Substring(0, searchIndex + generatorBeginLabel.Length) + "\n";

        var methodDataToEnumName = new Dictionary<MethodGenerationData, string>();
        var manualTweenTypes = new List<string> {
            "MainSequence",
            "NestedSequence",
            "TweenAnimationComponent",

            "Delay",
            "Callback",
            "TweenAwaiter",

            "ShakeLocalPosition",
            "ShakeLocalRotation",
            "ShakeScale",
            "ShakeCustom",
            "ShakeCamera",

            "CustomFloat",
            "CustomColor",
            "CustomVector2",
            "CustomVector3",
            "CustomVector4",
            "CustomQuaternion",
            "CustomRect",
            "CustomDouble",

            "MaterialColorProperty",
            "MaterialProperty",
            "MaterialAlphaProperty",
            "MaterialTextureOffset",
            "MaterialTextureScale",
            "MaterialPropertyVector4",

            "EulerAngles",
            "LocalEulerAngles",
            "GlobalTimeScale"
        };
        { // generate enums
            var enums = new List<string>(manualTweenTypes);
            foreach (var group in methodsData.GroupBy(_ => _.dependency)) {
                foreach (var data in group) {
                    string enumName = GetTweenTypeEnumName(data);
                    if (methodDataToEnumName.Values.Contains(enumName)) {
                        // skip duplicates like VisualElementColor_VisualElement / Color_VisualElement and VisualElementOpacity_VisualElement / Alpha_VisualElement
                        // Debug.Log($"skip duplicate {enumName}");
                        continue;
                    }
                    methodDataToEnumName.Add(data, enumName);
                    enums.Add(enumName);
                }
            }
            enums.Sort();
            enums.Insert(0, "Disabled");
            for (int i = 0; i < enums.Count; i++) {
                string enumName = enums[i];
                bool isDouble = enumName == "CustomDouble";
                if (isDouble) {
                    text += @"            #if PRIME_TWEEN_EXPERIMENTAL
";
                }
                text += $"            {enumName} = {i},\n";
                if (isDouble) {
                    text += @"            #endif
";
                }
            }
            text += @"        }
    }

";
        }

        {
            string utilsText = editorUtilsScript.text;
            int startIndex;
            {
                // generate GetAnimatedValue()
                startIndex = utilsText.IndexOf(generatorBeginLabel, StringComparison.Ordinal);
                Assert.AreNotEqual(-1, startIndex);
                const string generatorEndLabel = "// CODE GENERATOR END";
                int endIndex = utilsText.IndexOf(generatorEndLabel, startIndex, StringComparison.Ordinal);
                Assert.AreNotEqual(-1, endIndex);
                string afterEnd = utilsText.Substring(endIndex);
                utilsText = utilsText.Substring(0, startIndex + generatorBeginLabel.Length) + "\n";
                {
                    foreach (var group in methodsData.GroupBy(x => x.dependency)) {
                        Dependency dependency = group.Key;
                        if (shouldWrapInDefine(dependency)) {
                            switch (dependency) {
                                case Dependency.PRIME_TWEEN_EXPERIMENTAL:
                                case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
                                case Dependency.TEXT_MESH_PRO_INSTALLED:
                                    utilsText += $"            #if {dependency}\n";
                                    break;
                                default:
                                    utilsText += $"            #if !UNITY_2019_1_OR_NEWER || {dependency}\n";
                                    break;
                            }
                        }
                        foreach (MethodGenerationData data in group) {
                            if (!data.generateOnlyOverloads && methodDataToEnumName.TryGetValue(data, out string enumName)) {
                                string propNameOrGetter;
                                if (data.propertyName.Any()) {
                                    CheckFieldOrProp(data);
                                    propNameOrGetter = data.propertyName;
                                } else {
                                    Assert.IsTrue(data.propertyGetter.Any());
                                    propNameOrGetter = data.propertyGetter;
                                }
                                utilsText += $@"            case TweenType.{enumName}: return (target as {getTypeByName(data.targetType).FullName}).{propNameOrGetter}.ToContainer();
";
                            }
                        }
                        if (shouldWrapInDefine(dependency)) {
                            utilsText += "            #endif\n";
                        }
                    }
                }
                utilsText += afterEnd;
            }

            {
                // generate SetAnimatedValue()
                // p2 todo combine Utils class with TweenGenerated or Extensions file
                startIndex = utilsText.IndexOf(generatorBeginLabel, startIndex + generatorBeginLabel.Length, StringComparison.Ordinal);
                Assert.AreNotEqual(-1, startIndex);
                utilsText = utilsText.Substring(0, startIndex + generatorBeginLabel.Length) + "\n";

                foreach (var group in methodsData.GroupBy(x => x.dependency)) {
                    Dependency dependency = group.Key;
                    if (shouldWrapInDefine(dependency)) {
                        switch (dependency) {
                            case Dependency.PRIME_TWEEN_EXPERIMENTAL:
                            case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
                            case Dependency.TEXT_MESH_PRO_INSTALLED:
                                utilsText += $"            #if {dependency}\n";
                                break;
                            default:
                                utilsText += $"            #if !UNITY_2019_1_OR_NEWER || {dependency}\n";
                                break;
                        }
                    }
                    foreach (MethodGenerationData data in group) {
                        if (!data.generateOnlyOverloads && methodDataToEnumName.TryGetValue(data, out string enumName)) {
                            if (data.propertyName.Any()) {
                                CheckFieldOrProp(data);
                                utilsText += $@"            case TweenType.{enumName}: (target as {getTypeByName(data.targetType).FullName}).{data.propertyName} = {data.propertyType}Val(); break;
";
                            } else {
                                Assert.IsTrue(data.propertySetter.Any());
                                utilsText += $@"            case TweenType.{enumName}: {{
                var _target = target as {getTypeByName(data.targetType).FullName};
                var val = {data.propertyType}Val();
                _target.{data.propertySetter};
                break;
            }}
";
                            }
                        }
                    }
                    if (shouldWrapInDefine(dependency)) {
                        utilsText += "            #endif\n";
                    }
                }
            }

            // generate TweenTypeToTweenData()
            utilsText +=
@"            default: {
                rt.cold.onValueChange(ref rt, ref d);
                break;
            }
        }
        return true;
    }

    internal static (PropType, Type) TweenTypeToTweenData(TweenType tweenType) {
        switch (tweenType) {
";
            foreach (var group in methodsData.GroupBy(x => x.dependency)) {
                var dependency = group.Key;
                if (shouldWrapInDefine(dependency)) {
                    switch (dependency) {
                        case Dependency.PRIME_TWEEN_EXPERIMENTAL:
                        case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
                        case Dependency.TEXT_MESH_PRO_INSTALLED:
                            utilsText += $"            #if {dependency}\n";
                            break;
                        default:
                            utilsText += $"            #if !UNITY_2019_1_OR_NEWER || {dependency}\n";
                            break;
                    }
                }
                foreach (var data in group) {
                    if (!methodDataToEnumName.TryGetValue(data, out string enumName)) {
                        continue;
                    }
                    utilsText += $"            case TweenType.{enumName}: return (PropType.{data.propertyType}, typeof({getTypeByName(data.targetType).FullName}));\n";
                }
                if (shouldWrapInDefine(dependency)) {
                    utilsText += "            #endif\n";
                }
            }
            var tweenData = new List<(TweenType, PropType, Type)> {
                (TweenType.Disabled, PropType.Float, null), // dead sequence turns into Disabled
                (TweenType.Delay, PropType.Float, null),
                (TweenType.Callback, PropType.Float, null),
                (TweenType.ShakeLocalPosition, PropType.Vector3, typeof(Transform)),
                (TweenType.ShakeLocalRotation, PropType.Quaternion, typeof(Transform)),
                (TweenType.ShakeScale, PropType.Vector3, typeof(Transform)),
                (TweenType.ShakeCustom, PropType.Vector3, null),
                (TweenType.ShakeCamera, PropType.Float, typeof(Camera)),
                (TweenType.CustomFloat, PropType.Float, null),
                (TweenType.CustomColor, PropType.Color, null),
                (TweenType.CustomVector2, PropType.Vector2, null),
                (TweenType.CustomVector3, PropType.Vector3, null),
                (TweenType.CustomVector4, PropType.Vector4, null),
                (TweenType.CustomQuaternion, PropType.Quaternion, null),
                (TweenType.CustomRect, PropType.Rect, null),
                #if PRIME_TWEEN_EXPERIMENTAL
                (TweenType.CustomDouble, PropType.Double, null),
                #endif
                (TweenType.MaterialColorProperty, PropType.Color, typeof(Material)),
                (TweenType.MaterialProperty, PropType.Float, typeof(Material)),
                (TweenType.MaterialAlphaProperty, PropType.Float, typeof(Material)),
                (TweenType.MaterialTextureOffset, PropType.Vector2, typeof(Material)),
                (TweenType.MaterialTextureScale, PropType.Vector2, typeof(Material)),
                (TweenType.MaterialPropertyVector4, PropType.Vector4, typeof(Material)),
                (TweenType.EulerAngles, PropType.Vector3, typeof(Transform)),
                (TweenType.LocalEulerAngles, PropType.Vector3, typeof(Transform)),
                (TweenType.GlobalTimeScale, PropType.Float, null),
                (TweenType.MainSequence, PropType.Float, null),
                (TweenType.NestedSequence, PropType.Float, null),
                (TweenType.TweenAwaiter, PropType.Float, null)
            };
            foreach (var tuple in tweenData) {
                bool isDouble = tuple.Item1.ToString() == "CustomDouble";
                if (isDouble) {
                    utilsText += @"            #if PRIME_TWEEN_EXPERIMENTAL
";
                }
                string typeStr = tuple.Item3 == null ? "null" : $"typeof({tuple.Item3})";
                utilsText += $"            case TweenType.{tuple.Item1}: return (PropType.{tuple.Item2}, {typeStr});\n";
                if (isDouble) {
                    utilsText += @"            #endif
";
                }
            }
            bool isPro() {
                return false;
            }
            if (isPro()) {
                utilsText +=
@"            #if PRIME_TWEEN_PRO
            case TweenType.TweenAnimationComponent: return (PropType.None, typeof(PrimeTween.TweenAnimationComponent));
            #endif
";
            }
            utilsText += @"            default:
                throw new Exception();
        }
    }
}
}
";
            SaveScript(editorUtilsScript, utilsText);
        }

        text += "    public partial struct Tween {";
        text += generateWithDefines(generate);
        text = addCustomAnimationMethods(text);
        text += additiveMethodsGenerator.Generate();
        text += speedBasedMethodsGenerator.Generate();
        text += @"
    }
}";
        SaveScript(methodsScript, text);
        GenerateTweenComponent(methodDataToEnumName, manualTweenTypes);
    }

    static void CheckFieldOrProp(MethodGenerationData data) {
        var type = getTypeByName(data.targetType);
        Assert.IsNotNull(type);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        var prop = type.GetProperty(data.propertyName, flags);
        Type expectedPropType;
        if (data.propertyType == PropType.Float) {
            expectedPropType = typeof(float);
        } else if (data.propertyType == PropType.Int) {
            expectedPropType = typeof(int);
        } else {
            var typeName = $"{data.propertyType.ToFullTypeName()}, UnityEngine.CoreModule";
            expectedPropType = Type.GetType(typeName);
            Assert.IsNotNull(expectedPropType, typeName);
        }
        if (prop != null) {
            Assert.AreEqual(expectedPropType, prop.PropertyType);
            return;
        }

        var field = type.GetField(data.propertyName, flags);
        if (field != null) {
            Assert.AreEqual(expectedPropType, field.FieldType, "Field type is incorrect.");
            return;
        }

        throw new Exception($"Field or property with name ({data.propertyName}) not found for type {type.FullName}. Generation data name: {data.description}.");
    }

    static string GetTweenTypeEnumName(MethodGenerationData data) {
        string result = "";
        var dependency = data.dependency;
        if (dependency == Dependency.UI_ELEMENTS_MODULE_INSTALLED) {
            if (data.methodName == "Alpha") {
                return "VisualElementOpacity";
            }
        }

        if (dependency != Dependency.None) {
            result += getMethodPrefix(dependency);
        }
        if (dependency == Dependency.UI_ELEMENTS_MODULE_INSTALLED && !data.methodName.Contains("VisualElement")) {
            result += "VisualElement";
        }
        result += data.methodName;
        if ((data.methodName == "Alpha" || data.methodName == "Color") && dependency == Dependency.UNITY_UGUI_INSTALLED) {
            result += getTypeByName(data.targetType).Name;
        } else if (data.methodName == "Scale" && data.propertyType == PropType.Float) {
            result += "Uniform";
        } else if ((data.methodName == "Rotation" || data.methodName == "LocalRotation" || (data.methodName == "MoveRotation" && data.targetType == "UnityEngine.Rigidbody")) && data.propertyType == PropType.Quaternion) {
            result += "Quaternion";
        } else if (data.targetType == "PrimeTween.Sequence") {
            result += "Sequence";
        } else if (data.targetType == "UnityEngine.Rigidbody2D") {
            result += "2D";
        }
        return result;
    }

    [NotNull]
    string generateWithDefines([NotNull] Func<MethodGenerationData, string> generator) {
        string result = "";
        foreach (var group in methodsData.GroupBy(_ => _.dependency)) {
            result += generateWithDefines(generator, group);
        }
        return result;
    }

    [NotNull]
    static string generateWithDefines([NotNull] Func<MethodGenerationData, string> generator, [NotNull] IGrouping<Dependency, MethodGenerationData> group) {
        var result = "";
        var dependency = group.Key;
        if (shouldWrapInDefine(dependency)) {
            switch (dependency) {
                case Dependency.PRIME_TWEEN_EXPERIMENTAL:
                case Dependency.UI_ELEMENTS_MODULE_INSTALLED:
                case Dependency.TEXT_MESH_PRO_INSTALLED:
                    result += $"\n        #if {dependency}";
                    break;
                default:
                    result += $"\n        #if !UNITY_2019_1_OR_NEWER || {dependency}";
                    break;
            }
        }
        foreach (var method in group) {
            var generated = generator(method);
            if (!string.IsNullOrEmpty(generated)) {
                result += generated;
                result += "\n";
            }
        }
        if (shouldWrapInDefine(dependency)) {
            result += "\n        #endif";
        }
        return result;
    }

    [NotNull]
    static Type getTypeByName(string targetType) {
        var types = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(_ => _.GetType(targetType))
            .Where(_ => _ != null)
            .Where(_ => _.FullName == targetType)
            .Distinct()
            .ToArray();
        switch (types.Length) {
            case 0:
                throw new Exception($"target type ({targetType}) not found in any of the assemblies.\n" +
                                    "Please specify the full name of the type. For example, instead of 'Transform', use 'UnityEngine.Transform'.\n" +
                                    "Or install the target package in Package Manager.\n");
            case 1:
                break;
            default:
                throw new Exception($"More than one type found that match {targetType}. Found:\n"
                                    + string.Join("\n", types.Select(_ => $"{_.AssemblyQualifiedName}\n{_.Assembly.GetName().FullName}")));
        }
        var type = types.Single();
        Assert.IsNotNull(type, $"targetType ({targetType}) wasn't found in any assembly.");
        return type;
    }

    [NotNull]
    static string generate([NotNull] MethodGenerationData data) {
        var methodName = data.methodName;
        Assert.IsTrue(System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(methodName), $"Method name is invalid: {methodName}.");
        var propertyName = data.propertyName;

        var overload = populateTemplate(overloadTemplateTo, data);
        var full = populateTemplate(fullTemplate, data);
        const string templatePropName = "orthographicSize";
        string replaced = "";
        if (data.generateOnlyOverloads) {
            replaced += "\n";
            replaced += overload;
        } else if (propertyName.Any()) {
            CheckFieldOrProp(data);
            Assert.IsFalse(data.propertyGetter.Any());
            Assert.IsFalse(data.propertySetter.Any());
            replaced += "\n";
            replaced += overload;
            replaced += "\n";
            replaced += full;
            replaced = replaced.Replace(templatePropName, propertyName);
        } else {
            Assert.IsTrue(data.propertySetter.Any());
            if (data.propertyGetter.Any()) {
                replaced += "\n";
                replaced += replaceGetter(overload);
            }

            replaced += "\n";
            full = replaceSetter(full);
            replaced += replaceGetter(full);

            string replaceGetter(string str) {
                while (true) {
                    var j = str.IndexOf(templatePropName, StringComparison.Ordinal);
                    if (j == -1) {
                        break;
                    }
                    Assert.AreNotEqual(-1, j);
                    str = str.Remove(j, templatePropName.Length);
                    str = str.Insert(j, data.propertyGetter);
                }
                return str;
            }

            // ReSharper disable once AnnotateNotNullTypeMember
            string replaceSetter(string str) {
                while (true) {
                    var k = str.IndexOf("orthographicSize =", StringComparison.Ordinal);
                    if (k == -1) {
                        break;
                    }
                    Assert.AreNotEqual(-1, k);
                    var endIndex = str.IndexOf(";", k, StringComparison.Ordinal);
                    Assert.AreNotEqual(-1, endIndex);
                    str = str.Remove(k, endIndex - k);
                    str = str.Insert(k, data.propertySetter);
                }
                return str;
            }
        }
        return replaced;
    }

    [NotNull]
    static string addCustomAnimationMethods(string text) {
        const string template =
@"        public static Tween Custom_TEMPLATE(Single startValue, Single endValue, float duration, [NotNull] Action<Single> onValueChange, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Custom_TEMPLATE(new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE(Single startValue, Single endValue, float duration, [NotNull] Action<Single> onValueChange, Easing ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Custom_TEMPLATE(new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE(Single startValue, Single endValue, TweenSettings settings, [NotNull] Action<Single> onValueChange) => Custom_TEMPLATE(new TweenSettings<float>(startValue, endValue, settings), onValueChange);
        public static Tween Custom_TEMPLATE(TweenSettings<float> settings, [NotNull] Action<Single> onValueChange) {
            Assert.IsNotNull(onValueChange);
            if (settings.startFromCurrent) {
                UnityEngine.Debug.LogWarning(Constants.customTweensDontSupportStartFromCurrentWarning);
            }
            if (PrimeTweenManager.Instance.isDestroyed) {
                return default;
            }
            var tween = PrimeTweenManager.FetchTween(settings.settings._updateType);
            ref var rt = ref tween.managedData;
            ref var d = ref tween.data;

            d.startValue.CopyFrom(ref settings.startValue);
            rt.endValueOrDiff.CopyFrom(ref settings.endValue);

            tween.customOnValueChange = onValueChange;
            tween.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, false, TweenType.CustomFloat, ref rt, ref d);
            tween.onValueChange = (ref TweenData rt2, ref UnmanagedTweenData d2) => {
                if (d2.isUpdating) {
                    UnityEngine.Debug.LogError(Constants.recursiveCallError);
                    return;
                }
                d2.isUpdating = true;

                var startValue = d2.startValue;
                var t = d2.easedInterpolationFactor;
                var diff = rt2.endValueOrDiff;
                var val = TweenData.FloatVal(startValue, t, diff);
                var _onValueChange = rt2.cold.customOnValueChange as Action<Single>;
                try {
                    _onValueChange(val);
                } finally {
                    d2.isUpdating = false;
                }
            };
            return PrimeTweenManager.Animate(ref rt, ref d);
        }
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, Single startValue, Single endValue, float duration, [NotNull] Action<T, Single> onValueChange, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
            => Custom_internal(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, Single startValue, Single endValue, float duration, [NotNull] Action<T, Single> onValueChange, Easing ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
            => Custom_internal(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, Single startValue, Single endValue, TweenSettings settings, [NotNull] Action<T, Single> onValueChange) where T : class
            => Custom_internal(target, new TweenSettings<float>(startValue, endValue, settings), onValueChange);
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, TweenSettings<float> settings, [NotNull] Action<T, Single> onValueChange) where T : class
            => Custom_internal(target, settings, onValueChange);
        #if PRIME_TWEEN_EXPERIMENTAL
        public static Tween CustomAdditive<T>([NotNull] T target, Single deltaValue, TweenSettings settings, [NotNull] Action<T, Single> onDeltaChange) where T : class
            => Custom_internal(target, new TweenSettings<float>(default, deltaValue, settings), onDeltaChange, true);
        #endif
        static Tween Custom_internal<T>([NotNull] T target, TweenSettings<float> settings, [NotNull] Action<T, Single> onValueChange, bool isAdditive = false) where T : class {
            Assert.IsNotNull(onValueChange);
            if (settings.startFromCurrent) {
                UnityEngine.Debug.LogWarning(Constants.customTweensDontSupportStartFromCurrentWarning);
            }
            if (PrimeTweenManager.Instance.isDestroyed) {
                return default;
            }
            var tween = PrimeTweenManager.FetchTween(settings.settings._updateType);
            ref var rt = ref tween.managedData;
            ref var d = ref tween.data;

            d.startValue.CopyFrom(ref settings.startValue);
            d.isAdditive = isAdditive;
            rt.endValueOrDiff.CopyFrom(ref settings.endValue);

            tween.customOnValueChange = onValueChange;
            tween.Setup(target, ref settings.settings, false, TweenType.CustomFloat, ref rt, ref d);
            tween.onValueChange = (ref TweenData rt2, ref UnmanagedTweenData d2) => {
                if (d2.isUpdating) {
                    UnityEngine.Debug.LogError(Constants.recursiveCallError);
                    return;
                }
                d2.isUpdating = true;

                var startValue = d2.startValue;
                var t = d2.easedInterpolationFactor;
                var dataIsAdditive = d2.isAdditive;

                var _target = rt2.target as T;
                var diff = rt2.endValueOrDiff;

                Single val;
                if (dataIsAdditive) {
                    var newVal = TweenData.FloatVal(startValue, t, diff);
                    val = newVal.calcDelta(rt2.cold.prevVal);
                    rt2.cold.prevVal.single = newVal;
                } else {
                    val = TweenData.FloatVal(startValue, t, diff);
                }
                var _onValueChange = rt2.cold.customOnValueChange as Action<T, Single>;
                try {
                    _onValueChange(_target, val);
                } finally {
                    d2.isUpdating = false;
                }
            };
            return PrimeTweenManager.Animate(ref rt, ref d);
        }
        static Tween animate(object target, ref TweenSettings<float> settings, TweenType _tweenType) {
            if (PrimeTweenManager.Instance.isDestroyed) {
                return default;
            }
            var tween = PrimeTweenManager.FetchTween(settings.settings._updateType);
            ref var d = ref tween.data;
            ref var rt = ref tween.managedData;

            d.startValue.CopyFrom(ref settings.startValue);
            rt.endValueOrDiff.CopyFrom(ref settings.endValue);

            tween.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref rt, ref d);
            return PrimeTweenManager.Animate(ref rt, ref d);
        }
        static Tween animateWithIntParam([NotNull] object target, int intParam, ref TweenSettings<float> settings, TweenType _tweenType) {
            if (PrimeTweenManager.Instance.isDestroyed) {
                return default;
            }
            var tween = PrimeTweenManager.FetchTween(settings.settings._updateType);
            ref var d = ref tween.data;
            ref var rt = ref tween.managedData;

            d.startValue.CopyFrom(ref settings.startValue);
            rt.endValueOrDiff.CopyFrom(ref settings.endValue);

            tween.intParam = intParam;
            tween.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref rt, ref d);
            return PrimeTweenManager.Animate(ref rt, ref d);
        }";

        var types = new[] { typeof(float), typeof(Color), typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Quaternion), typeof(Rect) };
        foreach (var type in types) {
            text += "\n\n";
            var isFloat = type == typeof(float);
            var replaced = template;
            replaced = replaced.Replace("Single", isFloat ? "float" : type.FullName);
            if (!isFloat) {
                replaced = replaced.Replace("TweenSettings<float>", $"TweenSettings<{type.FullName}>");
                replaced = replaced.Replace("prevVal.single", $"prevVal.{type.Name.ToLower()}");
                replaced = replaced.Replace(".FloatVal", $".{type.Name}Val");
                replaced = replaced.Replace("Single val;", $"{type.Name} val;");
                replaced = replaced.Replace("PropType.Float", $"PropType.{type.Name}");
                replaced = replaced.Replace("TweenType.CustomFloat", $"TweenType.Custom{type.Name}");
            }
            replaced = replaced.Replace("Custom_TEMPLATE", "Custom");
            text += replaced;
        }
        return text;
    }

    [NotNull]
    static string populateTemplate([NotNull] string str, [NotNull] MethodGenerationData data) {
        var methodName = data.methodName;
        var prefix = getMethodPrefix(data.dependency);
        if (prefix != null && !data.placeInGlobalScope) {
            methodName = prefix + methodName;
        }
        var targetType = data.targetType;
        if (string.IsNullOrEmpty(targetType)) {
            str = str.Replace("[NotNull] UnityEngine.Camera target, ", "")
                .Replace("METHOD_NAME(target, ", "METHOD_NAME(");
        } else {
            str = str.Replace("UnityEngine.Camera", targetType);
        }
        str = str.Replace("METHOD_NAME",  methodName);
        str = str.Replace("TweenType.CameraOrthographicSize",  $"TweenType.{GetTweenTypeEnumName(data)}");
        if (data.propertyType != PropType.Float) {
            str = str.Replace("Single", data.propertyType.ToFullTypeName());
            str = str.Replace("_tween.FloatVal", $"_tween.{data.propertyType.ToString()}Val");
            str = str.Replace("TweenSettings<float>", $"TweenSettings<{data.propertyType.ToFullTypeName()}>");
        }
        return str;
    }

    [Serializable]
    internal class SpeedBasedMethodsGenerator {
        [SerializeField] Data[] data;

        [Serializable]
        class Data {
            [SerializeField] internal string methodName;
            [SerializeField] internal PropType propType;
            [SerializeField] internal string propName;
            [SerializeField] internal string speedParamName;
        }

        [NotNull]
        internal string Generate() {
            string result = "";
            foreach (var d in data) {
                const string template = @"
        public static Tween PositionAtSpeed([NotNull] UnityEngine.Transform target, UnityEngine.Vector3 endValue, float averageSpeed, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => PositionAtSpeed(target, new TweenSettings<UnityEngine.Vector3>(endValue, new TweenSettings(averageSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween PositionAtSpeed([NotNull] UnityEngine.Transform target, UnityEngine.Vector3 startValue, UnityEngine.Vector3 endValue, float averageSpeed, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => PositionAtSpeed(target, new TweenSettings<UnityEngine.Vector3>(startValue, endValue, new TweenSettings(averageSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        static Tween PositionAtSpeed([NotNull] UnityEngine.Transform target, TweenSettings<UnityEngine.Vector3> settings) {
            var speed = settings.settings.duration;
            if (speed <= 0) {
                UnityEngine.Debug.LogError($""Invalid speed provided to the Tween.{nameof(PositionAtSpeed)}() method: {speed}."");
                return default;
            }
            if (settings.startFromCurrent) {
                settings.startFromCurrent = false;
                settings.startValue = target.position;
            }
            settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / speed;
            return Tween.Position(target, settings);
        }
";
                result += template.Replace("PositionAtSpeed", $"{d.methodName}AtSpeed")
                    .Replace("UnityEngine.Vector3", d.propType.ToFullTypeName())
                    .Replace("Tween.Position", $"{d.methodName}")
                    .Replace("target.position", $"target.{d.propName}")
                    .Replace("averageSpeed", $"{d.speedParamName}")
                    ;
            }
            return result;
        }
    }
}

[Serializable]
class MethodGenerationData {
    public string description;
    public string methodName;
    public string targetType;
    public PropType propertyType;

    public string propertyName;

    public string propertyGetter;
    public string propertySetter;
    public string dotweenMethodName;
    public Dependency dependency;
    public bool placeInGlobalScope;
    public bool generateOnlyOverloads;
}

[PublicAPI]
enum Dependency {
    None,
    UNITY_UGUI_INSTALLED,
    AUDIO_MODULE_INSTALLED,
    PHYSICS_MODULE_INSTALLED,
    PHYSICS2D_MODULE_INSTALLED,
    Camera,
    Material,
    Light,
    PRIME_TWEEN_EXPERIMENTAL,
    UI_ELEMENTS_MODULE_INSTALLED,
    TEXT_MESH_PRO_INSTALLED
}

static class Ext {
    [NotNull]
    internal static string ToFullTypeName(this PropType type) {
        Assert.AreNotEqual(PropType.Float, type);
        if (type == PropType.Int) {
            return "int";
        }
        return $"UnityEngine.{type}";
    }
}
