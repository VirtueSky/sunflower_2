using System;
using UnityEngine;

namespace VirtueSky.Tweening
{
    public enum LoopType { Restart, PingPong }

    internal struct TweenCommonSettings
    {
        public Ease Ease;
        public float Delay;
        public bool UseUnscaledTime;
        public AnimationCurve CustomCurve;
        public Action OnComplete;
        public Action OnStart;
        public int Loops;
        public LoopType LoopType;
    }

    public struct FloatTweenBuilder
    {
        internal float From;
        internal float To;
        internal float Duration;
        internal TweenCommonSettings Settings;
        internal Action<float> OnValue;

        internal FloatTweenBuilder(float from, float to, float duration)
        {
            From = from;
            To = to;
            Duration = duration < 0f ? 0f : duration;
            Settings = default;
            Settings.Ease = Ease.Linear;
            OnValue = null;
        }

        public FloatTweenBuilder WithEase(Ease value) { Settings.Ease = value; return this; }
        public FloatTweenBuilder WithDelay(float value) { Settings.Delay = Mathf.Max(0f, value); return this; }
        public FloatTweenBuilder WithUnscaledTime(bool value = true) { Settings.UseUnscaledTime = value; return this; }
        public FloatTweenBuilder WithCurve(AnimationCurve value) { Settings.CustomCurve = value; Settings.Ease = Ease.CustomAnimationCurve; return this; }
        public FloatTweenBuilder WithOnComplete(Action callback) { Settings.OnComplete += callback; return this; }
        public FloatTweenBuilder WithOnStart(Action callback) { Settings.OnStart += callback; return this; }
        public FloatTweenBuilder WithLoops(int count) { Settings.Loops = count; return this; }
        public FloatTweenBuilder WithLoopType(LoopType loopType) { Settings.LoopType = loopType; return this; }
        public FloatTweenBuilder OnValueChanged(Action<float> callback) { OnValue += callback; return this; }
        public TweenHandle Bind(Action<float> callback) { OnValue += callback; return TweenRuntime.Add(ref this); }
        public TweenHandle Play() => TweenRuntime.Add(ref this);
    }

    public struct Vector2TweenBuilder
    {
        internal Vector2 From;
        internal Vector2 To;
        internal float Duration;
        internal TweenCommonSettings Settings;
        internal Action<Vector2> OnValue;

        internal Vector2TweenBuilder(Vector2 from, Vector2 to, float duration)
        {
            From = from;
            To = to;
            Duration = duration < 0f ? 0f : duration;
            Settings = default;
            Settings.Ease = Ease.Linear;
            OnValue = null;
        }

        public Vector2TweenBuilder WithEase(Ease value) { Settings.Ease = value; return this; }
        public Vector2TweenBuilder WithDelay(float value) { Settings.Delay = Mathf.Max(0f, value); return this; }
        public Vector2TweenBuilder WithUnscaledTime(bool value = true) { Settings.UseUnscaledTime = value; return this; }
        public Vector2TweenBuilder WithCurve(AnimationCurve value) { Settings.CustomCurve = value; Settings.Ease = Ease.CustomAnimationCurve; return this; }
        public Vector2TweenBuilder WithOnComplete(Action callback) { Settings.OnComplete += callback; return this; }
        public Vector2TweenBuilder WithOnStart(Action callback) { Settings.OnStart += callback; return this; }
        public Vector2TweenBuilder WithLoops(int count) { Settings.Loops = count; return this; }
        public Vector2TweenBuilder WithLoopType(LoopType loopType) { Settings.LoopType = loopType; return this; }
        public Vector2TweenBuilder OnValueChanged(Action<Vector2> callback) { OnValue += callback; return this; }
        public TweenHandle Bind(Action<Vector2> callback) { OnValue += callback; return TweenRuntime.Add(ref this); }
        public TweenHandle Play() => TweenRuntime.Add(ref this);
    }

    public struct Vector3TweenBuilder
    {
        internal Vector3 From;
        internal Vector3 To;
        internal float Duration;
        internal TweenCommonSettings Settings;
        internal Action<Vector3> OnValue;

        internal Vector3TweenBuilder(Vector3 from, Vector3 to, float duration)
        {
            From = from;
            To = to;
            Duration = duration < 0f ? 0f : duration;
            Settings = default;
            Settings.Ease = Ease.Linear;
            OnValue = null;
        }

        public Vector3TweenBuilder WithEase(Ease value) { Settings.Ease = value; return this; }
        public Vector3TweenBuilder WithDelay(float value) { Settings.Delay = Mathf.Max(0f, value); return this; }
        public Vector3TweenBuilder WithUnscaledTime(bool value = true) { Settings.UseUnscaledTime = value; return this; }
        public Vector3TweenBuilder WithCurve(AnimationCurve value) { Settings.CustomCurve = value; Settings.Ease = Ease.CustomAnimationCurve; return this; }
        public Vector3TweenBuilder WithOnComplete(Action callback) { Settings.OnComplete += callback; return this; }
        public Vector3TweenBuilder WithOnStart(Action callback) { Settings.OnStart += callback; return this; }
        public Vector3TweenBuilder WithLoops(int count) { Settings.Loops = count; return this; }
        public Vector3TweenBuilder WithLoopType(LoopType loopType) { Settings.LoopType = loopType; return this; }
        public Vector3TweenBuilder OnValueChanged(Action<Vector3> callback) { OnValue += callback; return this; }
        public TweenHandle Bind(Action<Vector3> callback) { OnValue += callback; return TweenRuntime.Add(ref this); }
        public TweenHandle Play() => TweenRuntime.Add(ref this);
    }

    public struct Vector4TweenBuilder
    {
        internal Vector4 From;
        internal Vector4 To;
        internal float Duration;
        internal TweenCommonSettings Settings;
        internal Action<Vector4> OnValue;

        internal Vector4TweenBuilder(Vector4 from, Vector4 to, float duration)
        {
            From = from;
            To = to;
            Duration = duration < 0f ? 0f : duration;
            Settings = default;
            Settings.Ease = Ease.Linear;
            OnValue = null;
        }

        public Vector4TweenBuilder WithEase(Ease value) { Settings.Ease = value; return this; }
        public Vector4TweenBuilder WithDelay(float value) { Settings.Delay = Mathf.Max(0f, value); return this; }
        public Vector4TweenBuilder WithUnscaledTime(bool value = true) { Settings.UseUnscaledTime = value; return this; }
        public Vector4TweenBuilder WithCurve(AnimationCurve value) { Settings.CustomCurve = value; Settings.Ease = Ease.CustomAnimationCurve; return this; }
        public Vector4TweenBuilder WithOnComplete(Action callback) { Settings.OnComplete += callback; return this; }
        public Vector4TweenBuilder WithOnStart(Action callback) { Settings.OnStart += callback; return this; }
        public Vector4TweenBuilder WithLoops(int count) { Settings.Loops = count; return this; }
        public Vector4TweenBuilder WithLoopType(LoopType loopType) { Settings.LoopType = loopType; return this; }
        public Vector4TweenBuilder OnValueChanged(Action<Vector4> callback) { OnValue += callback; return this; }
        public TweenHandle Bind(Action<Vector4> callback) { OnValue += callback; return TweenRuntime.Add(ref this); }
        public TweenHandle Play() => TweenRuntime.Add(ref this);
    }

    public struct ColorTweenBuilder
    {
        internal Color From;
        internal Color To;
        internal float Duration;
        internal TweenCommonSettings Settings;
        internal Action<Color> OnValue;

        internal ColorTweenBuilder(Color from, Color to, float duration)
        {
            From = from;
            To = to;
            Duration = duration < 0f ? 0f : duration;
            Settings = default;
            Settings.Ease = Ease.Linear;
            OnValue = null;
        }

        public ColorTweenBuilder WithEase(Ease value) { Settings.Ease = value; return this; }
        public ColorTweenBuilder WithDelay(float value) { Settings.Delay = Mathf.Max(0f, value); return this; }
        public ColorTweenBuilder WithUnscaledTime(bool value = true) { Settings.UseUnscaledTime = value; return this; }
        public ColorTweenBuilder WithCurve(AnimationCurve value) { Settings.CustomCurve = value; Settings.Ease = Ease.CustomAnimationCurve; return this; }
        public ColorTweenBuilder WithOnComplete(Action callback) { Settings.OnComplete += callback; return this; }
        public ColorTweenBuilder WithOnStart(Action callback) { Settings.OnStart += callback; return this; }
        public ColorTweenBuilder WithLoops(int count) { Settings.Loops = count; return this; }
        public ColorTweenBuilder WithLoopType(LoopType loopType) { Settings.LoopType = loopType; return this; }
        public ColorTweenBuilder OnValueChanged(Action<Color> callback) { OnValue += callback; return this; }
        public TweenHandle Bind(Action<Color> callback) { OnValue += callback; return TweenRuntime.Add(ref this); }
        public TweenHandle Play() => TweenRuntime.Add(ref this);
    }

    public struct QuaternionTweenBuilder
    {
        internal Quaternion From;
        internal Quaternion To;
        internal float Duration;
        internal TweenCommonSettings Settings;
        internal Action<Quaternion> OnValue;

        internal QuaternionTweenBuilder(Quaternion from, Quaternion to, float duration)
        {
            From = from;
            To = to;
            Duration = duration < 0f ? 0f : duration;
            Settings = default;
            Settings.Ease = Ease.Linear;
            OnValue = null;
        }

        public QuaternionTweenBuilder WithEase(Ease value) { Settings.Ease = value; return this; }
        public QuaternionTweenBuilder WithDelay(float value) { Settings.Delay = Mathf.Max(0f, value); return this; }
        public QuaternionTweenBuilder WithUnscaledTime(bool value = true) { Settings.UseUnscaledTime = value; return this; }
        public QuaternionTweenBuilder WithCurve(AnimationCurve value) { Settings.CustomCurve = value; Settings.Ease = Ease.CustomAnimationCurve; return this; }
        public QuaternionTweenBuilder WithOnComplete(Action callback) { Settings.OnComplete += callback; return this; }
        public QuaternionTweenBuilder WithOnStart(Action callback) { Settings.OnStart += callback; return this; }
        public QuaternionTweenBuilder WithLoops(int count) { Settings.Loops = count; return this; }
        public QuaternionTweenBuilder WithLoopType(LoopType loopType) { Settings.LoopType = loopType; return this; }
        public QuaternionTweenBuilder OnValueChanged(Action<Quaternion> callback) { OnValue += callback; return this; }
        public TweenHandle Bind(Action<Quaternion> callback) { OnValue += callback; return TweenRuntime.Add(ref this); }
        public TweenHandle Play() => TweenRuntime.Add(ref this);
    }

    public static class TweenBindExtensions
    {
        // Transform
        public static TweenHandle BindToPosition(this Vector3TweenBuilder builder, Transform target) => TweenRuntime.AddPosition(ref builder, target);
        public static TweenHandle BindToLocalPosition(this Vector3TweenBuilder builder, Transform target) => TweenRuntime.AddLocalPosition(ref builder, target);
        public static TweenHandle BindToLocalScale(this Vector3TweenBuilder builder, Transform target) => TweenRuntime.AddLocalScale(ref builder, target);
        public static TweenHandle BindToEulerAngles(this Vector3TweenBuilder builder, Transform target) => TweenRuntime.AddEulerAngles(ref builder, target);
        public static TweenHandle BindToLocalEulerAngles(this Vector3TweenBuilder builder, Transform target) => TweenRuntime.AddLocalEulerAngles(ref builder, target);
        public static TweenHandle BindToLocalPositionX(this FloatTweenBuilder builder, Transform target) => TweenRuntime.AddLocalPositionX(ref builder, target);
        public static TweenHandle BindToLocalPositionY(this FloatTweenBuilder builder, Transform target) => TweenRuntime.AddLocalPositionY(ref builder, target);
        public static TweenHandle BindToLocalPositionZ(this FloatTweenBuilder builder, Transform target) => TweenRuntime.AddLocalPositionZ(ref builder, target);
        // Rotation
        public static TweenHandle BindToRotation(this QuaternionTweenBuilder builder, Transform target) => TweenRuntime.AddRotation(ref builder, target);
        public static TweenHandle BindToLocalRotation(this QuaternionTweenBuilder builder, Transform target) => TweenRuntime.AddLocalRotation(ref builder, target);
        // Color
        public static TweenHandle BindToColor(this ColorTweenBuilder builder, SpriteRenderer target) => TweenRuntime.AddSpriteColor(ref builder, target);
        public static TweenHandle BindToColor(this ColorTweenBuilder builder, UnityEngine.UI.Graphic target) => TweenRuntime.AddGraphicColor(ref builder, target);
        // UI
        public static TweenHandle BindToAlpha(this FloatTweenBuilder builder, CanvasGroup target) => TweenRuntime.AddCanvasGroupAlpha(ref builder, target);
        public static TweenHandle BindToFillAmount(this FloatTweenBuilder builder, UnityEngine.UI.Image target) => TweenRuntime.AddImageFillAmount(ref builder, target);
        public static TweenHandle BindToAnchoredPosition(this Vector2TweenBuilder builder, RectTransform target) => TweenRuntime.AddUIAnchoredPosition(ref builder, target);
        public static TweenHandle BindToAnchoredPositionX(this FloatTweenBuilder builder, RectTransform target) => TweenRuntime.AddAnchoredPositionX(ref builder, target);
        public static TweenHandle BindToAnchoredPositionY(this FloatTweenBuilder builder, RectTransform target) => TweenRuntime.AddAnchoredPositionY(ref builder, target);
        public static TweenHandle BindToSizeDelta(this Vector2TweenBuilder builder, RectTransform target) => TweenRuntime.AddSizeDelta(ref builder, target);
    }
}