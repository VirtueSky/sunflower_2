using System;
using UnityEngine;
using UnityEngine.UI;

namespace VirtueSky.Tweening
{
    internal static class TweenRuntime
    {
        const int CapacityPerType = 256;

        internal static void Update(float deltaTime, float unscaledDeltaTime)
        {
            FloatStorage.Update(deltaTime, unscaledDeltaTime);
            Vector2Storage.Update(deltaTime, unscaledDeltaTime);
            Vector3Storage.Update(deltaTime, unscaledDeltaTime);
            Vector4Storage.Update(deltaTime, unscaledDeltaTime);
            ColorStorage.Update(deltaTime, unscaledDeltaTime);
            QuaternionStorage.Update(deltaTime, unscaledDeltaTime);
        }

        internal static bool IsActive(TweenValueKind kind, int index, uint version) => kind switch
        {
            TweenValueKind.Float => FloatStorage.IsActive(index, version),
            TweenValueKind.Vector2 => Vector2Storage.IsActive(index, version),
            TweenValueKind.Vector3 => Vector3Storage.IsActive(index, version),
            TweenValueKind.Vector4 => Vector4Storage.IsActive(index, version),
            TweenValueKind.Color => ColorStorage.IsActive(index, version),
            TweenValueKind.Quaternion => QuaternionStorage.IsActive(index, version),
            _ => false
        };

        internal static bool IsPaused(TweenValueKind kind, int index, uint version) => kind switch
        {
            TweenValueKind.Float => FloatStorage.IsPaused(index, version),
            TweenValueKind.Vector2 => Vector2Storage.IsPaused(index, version),
            TweenValueKind.Vector3 => Vector3Storage.IsPaused(index, version),
            TweenValueKind.Vector4 => Vector4Storage.IsPaused(index, version),
            TweenValueKind.Color => ColorStorage.IsPaused(index, version),
            TweenValueKind.Quaternion => QuaternionStorage.IsPaused(index, version),
            _ => false
        };

        internal static void Cancel(TweenValueKind kind, int index, uint version)
        {
            switch (kind)
            {
                case TweenValueKind.Float: FloatStorage.Cancel(index, version); break;
                case TweenValueKind.Vector2: Vector2Storage.Cancel(index, version); break;
                case TweenValueKind.Vector3: Vector3Storage.Cancel(index, version); break;
                case TweenValueKind.Vector4: Vector4Storage.Cancel(index, version); break;
                case TweenValueKind.Color: ColorStorage.Cancel(index, version); break;
                case TweenValueKind.Quaternion: QuaternionStorage.Cancel(index, version); break;
            }
        }

        internal static void Complete(TweenValueKind kind, int index, uint version)
        {
            switch (kind)
            {
                case TweenValueKind.Float: FloatStorage.Complete(index, version); break;
                case TweenValueKind.Vector2: Vector2Storage.Complete(index, version); break;
                case TweenValueKind.Vector3: Vector3Storage.Complete(index, version); break;
                case TweenValueKind.Vector4: Vector4Storage.Complete(index, version); break;
                case TweenValueKind.Color: ColorStorage.Complete(index, version); break;
                case TweenValueKind.Quaternion: QuaternionStorage.Complete(index, version); break;
            }
        }

        internal static void Pause(TweenValueKind kind, int index, uint version)
        {
            switch (kind)
            {
                case TweenValueKind.Float: FloatStorage.SetPaused(index, version, true); break;
                case TweenValueKind.Vector2: Vector2Storage.SetPaused(index, version, true); break;
                case TweenValueKind.Vector3: Vector3Storage.SetPaused(index, version, true); break;
                case TweenValueKind.Vector4: Vector4Storage.SetPaused(index, version, true); break;
                case TweenValueKind.Color: ColorStorage.SetPaused(index, version, true); break;
                case TweenValueKind.Quaternion: QuaternionStorage.SetPaused(index, version, true); break;
            }
        }

        internal static void Resume(TweenValueKind kind, int index, uint version)
        {
            switch (kind)
            {
                case TweenValueKind.Float: FloatStorage.SetPaused(index, version, false); break;
                case TweenValueKind.Vector2: Vector2Storage.SetPaused(index, version, false); break;
                case TweenValueKind.Vector3: Vector3Storage.SetPaused(index, version, false); break;
                case TweenValueKind.Vector4: Vector4Storage.SetPaused(index, version, false); break;
                case TweenValueKind.Color: ColorStorage.SetPaused(index, version, false); break;
                case TweenValueKind.Quaternion: QuaternionStorage.SetPaused(index, version, false); break;
            }
        }

        // --- Add (generic callbacks) ---

        internal static TweenHandle Add(ref FloatTweenBuilder builder)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.Callback, null);
        }

        internal static TweenHandle Add(ref Vector2TweenBuilder builder)
        {
            TweenRunner.Ensure();
            return Vector2Storage.Add(ref builder, Vector2BindingMode.Callback, null);
        }

        internal static TweenHandle Add(ref Vector3TweenBuilder builder)
        {
            TweenRunner.Ensure();
            return Vector3Storage.Add(ref builder, Vector3BindingMode.Callback, null);
        }

        internal static TweenHandle Add(ref Vector4TweenBuilder builder)
        {
            TweenRunner.Ensure();
            return Vector4Storage.Add(ref builder);
        }

        internal static TweenHandle Add(ref ColorTweenBuilder builder)
        {
            TweenRunner.Ensure();
            return ColorStorage.Add(ref builder, ColorBindingMode.Callback, null);
        }

        internal static TweenHandle Add(ref QuaternionTweenBuilder builder)
        {
            TweenRunner.Ensure();
            return QuaternionStorage.Add(ref builder, QuaternionBindingMode.Callback, null);
        }

        // --- Transform ---

        internal static TweenHandle AddPosition(ref Vector3TweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return Vector3Storage.Add(ref builder, Vector3BindingMode.Position, target);
        }

        internal static TweenHandle AddLocalPosition(ref Vector3TweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return Vector3Storage.Add(ref builder, Vector3BindingMode.LocalPosition, target);
        }

        internal static TweenHandle AddLocalScale(ref Vector3TweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return Vector3Storage.Add(ref builder, Vector3BindingMode.LocalScale, target);
        }

        internal static TweenHandle AddEulerAngles(ref Vector3TweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return Vector3Storage.Add(ref builder, Vector3BindingMode.EulerAngles, target);
        }

        internal static TweenHandle AddLocalEulerAngles(ref Vector3TweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return Vector3Storage.Add(ref builder, Vector3BindingMode.LocalEulerAngles, target);
        }

        internal static TweenHandle AddLocalPositionX(ref FloatTweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.LocalPositionX, target);
        }

        internal static TweenHandle AddLocalPositionY(ref FloatTweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.LocalPositionY, target);
        }

        internal static TweenHandle AddLocalPositionZ(ref FloatTweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.LocalPositionZ, target);
        }

        // --- Rotation ---

        internal static TweenHandle AddRotation(ref QuaternionTweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return QuaternionStorage.Add(ref builder, QuaternionBindingMode.Rotation, target);
        }

        internal static TweenHandle AddLocalRotation(ref QuaternionTweenBuilder builder, Transform target)
        {
            TweenRunner.Ensure();
            return QuaternionStorage.Add(ref builder, QuaternionBindingMode.LocalRotation, target);
        }

        // --- Color ---

        internal static TweenHandle AddSpriteColor(ref ColorTweenBuilder builder, SpriteRenderer target)
        {
            TweenRunner.Ensure();
            return ColorStorage.Add(ref builder, ColorBindingMode.SpriteRendererColor, target);
        }

        internal static TweenHandle AddGraphicColor(ref ColorTweenBuilder builder, Graphic target)
        {
            TweenRunner.Ensure();
            return ColorStorage.Add(ref builder, ColorBindingMode.GraphicColor, target);
        }

        // --- UI ---

        internal static TweenHandle AddCanvasGroupAlpha(ref FloatTweenBuilder builder, CanvasGroup target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.CanvasGroupAlpha, target);
        }

        internal static TweenHandle AddImageFillAmount(ref FloatTweenBuilder builder, Image target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.ImageFillAmount, target);
        }

        internal static TweenHandle AddUIAnchoredPosition(ref Vector2TweenBuilder builder, RectTransform target)
        {
            TweenRunner.Ensure();
            return Vector2Storage.Add(ref builder, Vector2BindingMode.UIAnchoredPosition, target);
        }

        internal static TweenHandle AddAnchoredPositionX(ref FloatTweenBuilder builder, RectTransform target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.AnchoredPositionX, target);
        }

        internal static TweenHandle AddAnchoredPositionY(ref FloatTweenBuilder builder, RectTransform target)
        {
            TweenRunner.Ensure();
            return FloatStorage.Add(ref builder, FloatBindingMode.AnchoredPositionY, target);
        }

        internal static TweenHandle AddSizeDelta(ref Vector2TweenBuilder builder, RectTransform target)
        {
            TweenRunner.Ensure();
            return Vector2Storage.Add(ref builder, Vector2BindingMode.SizeDelta, target);
        }

        // --- Binding mode enums ---

        enum FloatBindingMode : byte
        {
            Callback,
            CanvasGroupAlpha,
            ImageFillAmount,
            LocalPositionX,
            LocalPositionY,
            LocalPositionZ,
            AnchoredPositionX,
            AnchoredPositionY
        }

        enum Vector2BindingMode : byte
        {
            Callback,
            UIAnchoredPosition,
            SizeDelta
        }

        enum Vector3BindingMode : byte
        {
            Callback,
            Position,
            LocalPosition,
            LocalScale,
            EulerAngles,
            LocalEulerAngles
        }

        enum ColorBindingMode : byte
        {
            Callback,
            SpriteRendererColor,
            GraphicColor
        }

        enum QuaternionBindingMode : byte
        {
            Callback,
            Rotation,
            LocalRotation
        }

        // =====================================================================
        // Shared loop helper — called when elapsed >= duration inside each Update
        // Returns true if the tween should finish, false if it loops again.
        // =====================================================================

        static bool ShouldFinish(int loops, int loopsPlayed) =>
            loops != -1 && loopsPlayed + 1 >= loops;

        // =====================================================================
        // FloatStorage
        // =====================================================================

        static class FloatStorage
        {
            struct Entry
            {
                public bool Active;
                public bool Paused;
                public bool StartFired;
                public uint Version;
                public float From;
                public float To;
                public float Duration;
                public float Elapsed;
                public float DelayRemaining;
                public Ease Ease;
                public bool UseUnscaledTime;
                public AnimationCurve Curve;
                public Action<float> OnValue;
                public Action OnStart;
                public Action OnComplete;
                public FloatBindingMode BindingMode;
                public UnityEngine.Object Target;
                public int Loops;
                public int LoopsPlayed;
                public LoopType LoopType;
            }

            static readonly Entry[] entries = new Entry[CapacityPerType];

            public static TweenHandle Add(ref FloatTweenBuilder builder, FloatBindingMode bindingMode, UnityEngine.Object target)
            {
                if (!TryAcquireSlot(out var index))
                {
                    Debug.LogError("Tween float storage is full.");
                    return default;
                }

                ref var entry = ref entries[index];
                entry.Active = true;
                entry.Paused = false;
                entry.StartFired = false;
                entry.From = builder.From;
                entry.To = builder.To;
                entry.Duration = builder.Duration;
                entry.Elapsed = 0f;
                entry.DelayRemaining = builder.Settings.Delay;
                entry.Ease = builder.Settings.Ease;
                entry.UseUnscaledTime = builder.Settings.UseUnscaledTime;
                entry.Curve = builder.Settings.CustomCurve;
                entry.OnValue = builder.OnValue;
                entry.OnStart = builder.Settings.OnStart;
                entry.OnComplete = builder.Settings.OnComplete;
                entry.BindingMode = bindingMode;
                entry.Target = target;
                entry.Loops = builder.Settings.Loops == 0 ? 1 : builder.Settings.Loops;
                entry.LoopsPlayed = 0;
                entry.LoopType = builder.Settings.LoopType;

                if (entry.Duration <= 0f && entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 1f);
                    Finish(index);
                }
                else if (entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 0f);
                }

                return new TweenHandle(TweenValueKind.Float, index, entry.Version);
            }

            public static void Update(float deltaTime, float unscaledDeltaTime)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (!entries[i].Active || entries[i].Paused) continue;
                    ref var entry = ref entries[i];

                    if (entry.BindingMode != FloatBindingMode.Callback && entry.Target == null)
                    {
                        entries[i].Active = false;
                        continue;
                    }

                    var dt = entry.UseUnscaledTime ? unscaledDeltaTime : deltaTime;

                    if (entry.DelayRemaining > 0f)
                    {
                        entry.DelayRemaining -= dt;
                        if (entry.DelayRemaining > 0f) continue;
                        dt = -entry.DelayRemaining;
                        FireStart(ref entry);
                        Apply(ref entry, 0f);
                    }

                    if (entry.Duration <= 0f)
                    {
                        Apply(ref entry, 1f);
                        Finish(i);
                        continue;
                    }

                    entry.Elapsed += dt;

                    if (entry.Elapsed >= entry.Duration)
                    {
                        if (ShouldFinish(entry.Loops, entry.LoopsPlayed))
                        {
                            Apply(ref entry, 1f);
                            Finish(i);
                        }
                        else
                        {
                            entry.LoopsPlayed++;
                            var overflow = entry.Elapsed - entry.Duration;
                            entry.Elapsed = overflow > 0f ? overflow : 0f;
                            if (entry.LoopType == LoopType.PingPong)
                                (entry.From, entry.To) = (entry.To, entry.From);
                            Apply(ref entry, entry.Elapsed / entry.Duration);
                        }
                    }
                    else
                    {
                        Apply(ref entry, entry.Elapsed / entry.Duration);
                    }
                }
            }

            public static bool IsActive(int index, uint version) => IsValid(index, version) && entries[index].Active;
            public static bool IsPaused(int index, uint version) => IsValid(index, version) && entries[index].Paused;
            public static void Cancel(int index, uint version) { if (IsValid(index, version)) entries[index].Active = false; }
            public static void Complete(int index, uint version) { if (IsValid(index, version) && entries[index].Active) { Apply(ref entries[index], 1f); Finish(index); } }
            public static void SetPaused(int index, uint version, bool paused) { if (IsValid(index, version) && entries[index].Active) entries[index].Paused = paused; }

            static void FireStart(ref Entry entry)
            {
                if (entry.StartFired) return;
                entry.StartFired = true;
                entry.OnStart?.Invoke();
            }

            static void Apply(ref Entry entry, float progress)
            {
                var value = Mathf.LerpUnclamped(entry.From, entry.To, EaseUtility.Evaluate(progress, entry.Ease, entry.Curve));
                switch (entry.BindingMode)
                {
                    case FloatBindingMode.CanvasGroupAlpha:
                        if (entry.Target is CanvasGroup cg) cg.alpha = value;
                        break;
                    case FloatBindingMode.ImageFillAmount:
                        if (entry.Target is Image img) img.fillAmount = value;
                        break;
                    case FloatBindingMode.LocalPositionX:
                        if (entry.Target is Transform t1) { var p = t1.localPosition; p.x = value; t1.localPosition = p; }
                        break;
                    case FloatBindingMode.LocalPositionY:
                        if (entry.Target is Transform t2) { var p = t2.localPosition; p.y = value; t2.localPosition = p; }
                        break;
                    case FloatBindingMode.LocalPositionZ:
                        if (entry.Target is Transform t3) { var p = t3.localPosition; p.z = value; t3.localPosition = p; }
                        break;
                    case FloatBindingMode.AnchoredPositionX:
                        if (entry.Target is RectTransform r1) { var p = r1.anchoredPosition; p.x = value; r1.anchoredPosition = p; }
                        break;
                    case FloatBindingMode.AnchoredPositionY:
                        if (entry.Target is RectTransform r2) { var p = r2.anchoredPosition; p.y = value; r2.anchoredPosition = p; }
                        break;
                }
                entry.OnValue?.Invoke(value);
            }

            static void Finish(int index)
            {
                var callback = entries[index].OnComplete;
                entries[index].Active = false;
                entries[index].Paused = false;
                entries[index].StartFired = false;
                entries[index].OnValue = null;
                entries[index].OnStart = null;
                entries[index].OnComplete = null;
                entries[index].Curve = null;
                entries[index].Target = null;
                entries[index].LoopsPlayed = 0;
                callback?.Invoke();
            }

            static bool TryAcquireSlot(out int index)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Active) continue;
                    entries[i].Version++;
                    index = i;
                    return true;
                }
                index = -1;
                return false;
            }

            static bool IsValid(int index, uint version) =>
                index >= 0 && index < entries.Length && entries[index].Version == version;
        }

        // =====================================================================
        // Vector2Storage
        // =====================================================================

        static class Vector2Storage
        {
            struct Entry
            {
                public bool Active;
                public bool Paused;
                public bool StartFired;
                public uint Version;
                public Vector2 From;
                public Vector2 To;
                public float Duration;
                public float Elapsed;
                public float DelayRemaining;
                public Ease Ease;
                public bool UseUnscaledTime;
                public AnimationCurve Curve;
                public Action<Vector2> OnValue;
                public Action OnStart;
                public Action OnComplete;
                public Vector2BindingMode BindingMode;
                public RectTransform Target;
                public int Loops;
                public int LoopsPlayed;
                public LoopType LoopType;
            }

            static readonly Entry[] entries = new Entry[CapacityPerType];

            public static TweenHandle Add(ref Vector2TweenBuilder builder, Vector2BindingMode bindingMode, RectTransform target)
            {
                if (!TryAcquireSlot(out var index))
                {
                    Debug.LogError("Tween Vector2 storage is full.");
                    return default;
                }

                ref var entry = ref entries[index];
                entry.Active = true;
                entry.Paused = false;
                entry.StartFired = false;
                entry.From = builder.From;
                entry.To = builder.To;
                entry.Duration = builder.Duration;
                entry.Elapsed = 0f;
                entry.DelayRemaining = builder.Settings.Delay;
                entry.Ease = builder.Settings.Ease;
                entry.UseUnscaledTime = builder.Settings.UseUnscaledTime;
                entry.Curve = builder.Settings.CustomCurve;
                entry.OnValue = builder.OnValue;
                entry.OnStart = builder.Settings.OnStart;
                entry.OnComplete = builder.Settings.OnComplete;
                entry.BindingMode = bindingMode;
                entry.Target = target;
                entry.Loops = builder.Settings.Loops == 0 ? 1 : builder.Settings.Loops;
                entry.LoopsPlayed = 0;
                entry.LoopType = builder.Settings.LoopType;

                if (entry.Duration <= 0f && entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 1f);
                    Finish(index);
                }
                else if (entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 0f);
                }

                return new TweenHandle(TweenValueKind.Vector2, index, entry.Version);
            }

            public static void Update(float deltaTime, float unscaledDeltaTime)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (!entries[i].Active || entries[i].Paused) continue;
                    ref var entry = ref entries[i];

                    if (entry.BindingMode != Vector2BindingMode.Callback && entry.Target == null)
                    {
                        entries[i].Active = false;
                        continue;
                    }

                    var dt = entry.UseUnscaledTime ? unscaledDeltaTime : deltaTime;

                    if (entry.DelayRemaining > 0f)
                    {
                        entry.DelayRemaining -= dt;
                        if (entry.DelayRemaining > 0f) continue;
                        dt = -entry.DelayRemaining;
                        FireStart(ref entry);
                        Apply(ref entry, 0f);
                    }

                    if (entry.Duration <= 0f)
                    {
                        Apply(ref entry, 1f);
                        Finish(i);
                        continue;
                    }

                    entry.Elapsed += dt;

                    if (entry.Elapsed >= entry.Duration)
                    {
                        if (ShouldFinish(entry.Loops, entry.LoopsPlayed))
                        {
                            Apply(ref entry, 1f);
                            Finish(i);
                        }
                        else
                        {
                            entry.LoopsPlayed++;
                            var overflow = entry.Elapsed - entry.Duration;
                            entry.Elapsed = overflow > 0f ? overflow : 0f;
                            if (entry.LoopType == LoopType.PingPong)
                                (entry.From, entry.To) = (entry.To, entry.From);
                            Apply(ref entry, entry.Elapsed / entry.Duration);
                        }
                    }
                    else
                    {
                        Apply(ref entry, entry.Elapsed / entry.Duration);
                    }
                }
            }

            public static bool IsActive(int index, uint version) => IsValid(index, version) && entries[index].Active;
            public static bool IsPaused(int index, uint version) => IsValid(index, version) && entries[index].Paused;
            public static void Cancel(int index, uint version) { if (IsValid(index, version)) entries[index].Active = false; }
            public static void Complete(int index, uint version) { if (IsValid(index, version) && entries[index].Active) { Apply(ref entries[index], 1f); Finish(index); } }
            public static void SetPaused(int index, uint version, bool paused) { if (IsValid(index, version) && entries[index].Active) entries[index].Paused = paused; }

            static void FireStart(ref Entry entry)
            {
                if (entry.StartFired) return;
                entry.StartFired = true;
                entry.OnStart?.Invoke();
            }

            static void Apply(ref Entry entry, float progress)
            {
                var value = Vector2.LerpUnclamped(entry.From, entry.To, EaseUtility.Evaluate(progress, entry.Ease, entry.Curve));
                if (entry.Target != null)
                {
                    switch (entry.BindingMode)
                    {
                        case Vector2BindingMode.UIAnchoredPosition: entry.Target.anchoredPosition = value; break;
                        case Vector2BindingMode.SizeDelta: entry.Target.sizeDelta = value; break;
                    }
                }
                entry.OnValue?.Invoke(value);
            }

            static void Finish(int index)
            {
                var callback = entries[index].OnComplete;
                entries[index].Active = false;
                entries[index].Paused = false;
                entries[index].StartFired = false;
                entries[index].OnValue = null;
                entries[index].OnStart = null;
                entries[index].OnComplete = null;
                entries[index].Curve = null;
                entries[index].Target = null;
                entries[index].LoopsPlayed = 0;
                callback?.Invoke();
            }

            static bool TryAcquireSlot(out int index)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Active) continue;
                    entries[i].Version++;
                    index = i;
                    return true;
                }
                index = -1;
                return false;
            }

            static bool IsValid(int index, uint version) =>
                index >= 0 && index < entries.Length && entries[index].Version == version;
        }

        // =====================================================================
        // Vector3Storage
        // =====================================================================

        static class Vector3Storage
        {
            struct Entry
            {
                public bool Active;
                public bool Paused;
                public bool StartFired;
                public uint Version;
                public Vector3 From;
                public Vector3 To;
                public float Duration;
                public float Elapsed;
                public float DelayRemaining;
                public Ease Ease;
                public bool UseUnscaledTime;
                public AnimationCurve Curve;
                public Action<Vector3> OnValue;
                public Action OnStart;
                public Action OnComplete;
                public Vector3BindingMode BindingMode;
                public Transform Target;
                public int Loops;
                public int LoopsPlayed;
                public LoopType LoopType;
            }

            static readonly Entry[] entries = new Entry[CapacityPerType];

            public static TweenHandle Add(ref Vector3TweenBuilder builder, Vector3BindingMode bindingMode, Transform target)
            {
                if (!TryAcquireSlot(out var index))
                {
                    Debug.LogError("Tween Vector3 storage is full.");
                    return default;
                }

                ref var entry = ref entries[index];
                entry.Active = true;
                entry.Paused = false;
                entry.StartFired = false;
                entry.From = builder.From;
                entry.To = builder.To;
                entry.Duration = builder.Duration;
                entry.Elapsed = 0f;
                entry.DelayRemaining = builder.Settings.Delay;
                entry.Ease = builder.Settings.Ease;
                entry.UseUnscaledTime = builder.Settings.UseUnscaledTime;
                entry.Curve = builder.Settings.CustomCurve;
                entry.OnValue = builder.OnValue;
                entry.OnStart = builder.Settings.OnStart;
                entry.OnComplete = builder.Settings.OnComplete;
                entry.BindingMode = bindingMode;
                entry.Target = target;
                entry.Loops = builder.Settings.Loops == 0 ? 1 : builder.Settings.Loops;
                entry.LoopsPlayed = 0;
                entry.LoopType = builder.Settings.LoopType;

                if (entry.Duration <= 0f && entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 1f);
                    Finish(index);
                }
                else if (entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 0f);
                }

                return new TweenHandle(TweenValueKind.Vector3, index, entry.Version);
            }

            public static void Update(float deltaTime, float unscaledDeltaTime)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (!entries[i].Active || entries[i].Paused) continue;
                    ref var entry = ref entries[i];

                    if (entry.BindingMode != Vector3BindingMode.Callback && entry.Target == null)
                    {
                        entries[i].Active = false;
                        continue;
                    }

                    var dt = entry.UseUnscaledTime ? unscaledDeltaTime : deltaTime;

                    if (entry.DelayRemaining > 0f)
                    {
                        entry.DelayRemaining -= dt;
                        if (entry.DelayRemaining > 0f) continue;
                        dt = -entry.DelayRemaining;
                        FireStart(ref entry);
                        Apply(ref entry, 0f);
                    }

                    if (entry.Duration <= 0f)
                    {
                        Apply(ref entry, 1f);
                        Finish(i);
                        continue;
                    }

                    entry.Elapsed += dt;

                    if (entry.Elapsed >= entry.Duration)
                    {
                        if (ShouldFinish(entry.Loops, entry.LoopsPlayed))
                        {
                            Apply(ref entry, 1f);
                            Finish(i);
                        }
                        else
                        {
                            entry.LoopsPlayed++;
                            var overflow = entry.Elapsed - entry.Duration;
                            entry.Elapsed = overflow > 0f ? overflow : 0f;
                            if (entry.LoopType == LoopType.PingPong)
                                (entry.From, entry.To) = (entry.To, entry.From);
                            Apply(ref entry, entry.Elapsed / entry.Duration);
                        }
                    }
                    else
                    {
                        Apply(ref entry, entry.Elapsed / entry.Duration);
                    }
                }
            }

            public static bool IsActive(int index, uint version) => IsValid(index, version) && entries[index].Active;
            public static bool IsPaused(int index, uint version) => IsValid(index, version) && entries[index].Paused;
            public static void Cancel(int index, uint version) { if (IsValid(index, version)) entries[index].Active = false; }
            public static void Complete(int index, uint version) { if (IsValid(index, version) && entries[index].Active) { Apply(ref entries[index], 1f); Finish(index); } }
            public static void SetPaused(int index, uint version, bool paused) { if (IsValid(index, version) && entries[index].Active) entries[index].Paused = paused; }

            static void FireStart(ref Entry entry)
            {
                if (entry.StartFired) return;
                entry.StartFired = true;
                entry.OnStart?.Invoke();
            }

            static void Apply(ref Entry entry, float progress)
            {
                var value = Vector3.LerpUnclamped(entry.From, entry.To, EaseUtility.Evaluate(progress, entry.Ease, entry.Curve));
                var target = entry.Target;
                if (target != null)
                {
                    switch (entry.BindingMode)
                    {
                        case Vector3BindingMode.Position: target.position = value; break;
                        case Vector3BindingMode.LocalPosition: target.localPosition = value; break;
                        case Vector3BindingMode.LocalScale: target.localScale = value; break;
                        case Vector3BindingMode.EulerAngles: target.eulerAngles = value; break;
                        case Vector3BindingMode.LocalEulerAngles: target.localEulerAngles = value; break;
                    }
                }
                entry.OnValue?.Invoke(value);
            }

            static void Finish(int index)
            {
                var callback = entries[index].OnComplete;
                entries[index].Active = false;
                entries[index].Paused = false;
                entries[index].StartFired = false;
                entries[index].OnValue = null;
                entries[index].OnStart = null;
                entries[index].OnComplete = null;
                entries[index].Curve = null;
                entries[index].Target = null;
                entries[index].LoopsPlayed = 0;
                callback?.Invoke();
            }

            static bool TryAcquireSlot(out int index)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Active) continue;
                    entries[i].Version++;
                    index = i;
                    return true;
                }
                index = -1;
                return false;
            }

            static bool IsValid(int index, uint version) =>
                index >= 0 && index < entries.Length && entries[index].Version == version;
        }

        // =====================================================================
        // Vector4Storage
        // =====================================================================

        static class Vector4Storage
        {
            struct Entry
            {
                public bool Active;
                public bool Paused;
                public bool StartFired;
                public uint Version;
                public Vector4 From;
                public Vector4 To;
                public float Duration;
                public float Elapsed;
                public float DelayRemaining;
                public Ease Ease;
                public bool UseUnscaledTime;
                public AnimationCurve Curve;
                public Action<Vector4> OnValue;
                public Action OnStart;
                public Action OnComplete;
                public int Loops;
                public int LoopsPlayed;
                public LoopType LoopType;
            }

            static readonly Entry[] entries = new Entry[CapacityPerType];

            public static TweenHandle Add(ref Vector4TweenBuilder builder)
            {
                if (!TryAcquireSlot(out var index))
                {
                    Debug.LogError("Tween Vector4 storage is full.");
                    return default;
                }

                ref var entry = ref entries[index];
                entry.Active = true;
                entry.Paused = false;
                entry.StartFired = false;
                entry.From = builder.From;
                entry.To = builder.To;
                entry.Duration = builder.Duration;
                entry.Elapsed = 0f;
                entry.DelayRemaining = builder.Settings.Delay;
                entry.Ease = builder.Settings.Ease;
                entry.UseUnscaledTime = builder.Settings.UseUnscaledTime;
                entry.Curve = builder.Settings.CustomCurve;
                entry.OnValue = builder.OnValue;
                entry.OnStart = builder.Settings.OnStart;
                entry.OnComplete = builder.Settings.OnComplete;
                entry.Loops = builder.Settings.Loops == 0 ? 1 : builder.Settings.Loops;
                entry.LoopsPlayed = 0;
                entry.LoopType = builder.Settings.LoopType;

                if (entry.Duration <= 0f && entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 1f);
                    Finish(index);
                }
                else if (entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 0f);
                }

                return new TweenHandle(TweenValueKind.Vector4, index, entry.Version);
            }

            public static void Update(float deltaTime, float unscaledDeltaTime)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (!entries[i].Active || entries[i].Paused) continue;
                    ref var entry = ref entries[i];
                    var dt = entry.UseUnscaledTime ? unscaledDeltaTime : deltaTime;

                    if (entry.DelayRemaining > 0f)
                    {
                        entry.DelayRemaining -= dt;
                        if (entry.DelayRemaining > 0f) continue;
                        dt = -entry.DelayRemaining;
                        FireStart(ref entry);
                        Apply(ref entry, 0f);
                    }

                    if (entry.Duration <= 0f)
                    {
                        Apply(ref entry, 1f);
                        Finish(i);
                        continue;
                    }

                    entry.Elapsed += dt;

                    if (entry.Elapsed >= entry.Duration)
                    {
                        if (ShouldFinish(entry.Loops, entry.LoopsPlayed))
                        {
                            Apply(ref entry, 1f);
                            Finish(i);
                        }
                        else
                        {
                            entry.LoopsPlayed++;
                            var overflow = entry.Elapsed - entry.Duration;
                            entry.Elapsed = overflow > 0f ? overflow : 0f;
                            if (entry.LoopType == LoopType.PingPong)
                                (entry.From, entry.To) = (entry.To, entry.From);
                            Apply(ref entry, entry.Elapsed / entry.Duration);
                        }
                    }
                    else
                    {
                        Apply(ref entry, entry.Elapsed / entry.Duration);
                    }
                }
            }

            public static bool IsActive(int index, uint version) => IsValid(index, version) && entries[index].Active;
            public static bool IsPaused(int index, uint version) => IsValid(index, version) && entries[index].Paused;
            public static void Cancel(int index, uint version) { if (IsValid(index, version)) entries[index].Active = false; }
            public static void Complete(int index, uint version) { if (IsValid(index, version) && entries[index].Active) { Apply(ref entries[index], 1f); Finish(index); } }
            public static void SetPaused(int index, uint version, bool paused) { if (IsValid(index, version) && entries[index].Active) entries[index].Paused = paused; }

            static void FireStart(ref Entry entry)
            {
                if (entry.StartFired) return;
                entry.StartFired = true;
                entry.OnStart?.Invoke();
            }

            static void Apply(ref Entry entry, float progress)
            {
                var value = Vector4.LerpUnclamped(entry.From, entry.To, EaseUtility.Evaluate(progress, entry.Ease, entry.Curve));
                entry.OnValue?.Invoke(value);
            }

            static void Finish(int index)
            {
                var callback = entries[index].OnComplete;
                entries[index].Active = false;
                entries[index].Paused = false;
                entries[index].StartFired = false;
                entries[index].OnValue = null;
                entries[index].OnStart = null;
                entries[index].OnComplete = null;
                entries[index].Curve = null;
                entries[index].LoopsPlayed = 0;
                callback?.Invoke();
            }

            static bool TryAcquireSlot(out int index)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Active) continue;
                    entries[i].Version++;
                    index = i;
                    return true;
                }
                index = -1;
                return false;
            }

            static bool IsValid(int index, uint version) =>
                index >= 0 && index < entries.Length && entries[index].Version == version;
        }

        // =====================================================================
        // ColorStorage
        // =====================================================================

        static class ColorStorage
        {
            struct Entry
            {
                public bool Active;
                public bool Paused;
                public bool StartFired;
                public uint Version;
                public Color From;
                public Color To;
                public float Duration;
                public float Elapsed;
                public float DelayRemaining;
                public Ease Ease;
                public bool UseUnscaledTime;
                public AnimationCurve Curve;
                public Action<Color> OnValue;
                public Action OnStart;
                public Action OnComplete;
                public ColorBindingMode BindingMode;
                public UnityEngine.Object Target;
                public int Loops;
                public int LoopsPlayed;
                public LoopType LoopType;
            }

            static readonly Entry[] entries = new Entry[CapacityPerType];

            public static TweenHandle Add(ref ColorTweenBuilder builder, ColorBindingMode bindingMode, UnityEngine.Object target)
            {
                if (!TryAcquireSlot(out var index))
                {
                    Debug.LogError("Tween Color storage is full.");
                    return default;
                }

                ref var entry = ref entries[index];
                entry.Active = true;
                entry.Paused = false;
                entry.StartFired = false;
                entry.From = builder.From;
                entry.To = builder.To;
                entry.Duration = builder.Duration;
                entry.Elapsed = 0f;
                entry.DelayRemaining = builder.Settings.Delay;
                entry.Ease = builder.Settings.Ease;
                entry.UseUnscaledTime = builder.Settings.UseUnscaledTime;
                entry.Curve = builder.Settings.CustomCurve;
                entry.OnValue = builder.OnValue;
                entry.OnStart = builder.Settings.OnStart;
                entry.OnComplete = builder.Settings.OnComplete;
                entry.BindingMode = bindingMode;
                entry.Target = target;
                entry.Loops = builder.Settings.Loops == 0 ? 1 : builder.Settings.Loops;
                entry.LoopsPlayed = 0;
                entry.LoopType = builder.Settings.LoopType;

                if (entry.Duration <= 0f && entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 1f);
                    Finish(index);
                }
                else if (entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 0f);
                }

                return new TweenHandle(TweenValueKind.Color, index, entry.Version);
            }

            public static void Update(float deltaTime, float unscaledDeltaTime)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (!entries[i].Active || entries[i].Paused) continue;
                    ref var entry = ref entries[i];

                    if (entry.BindingMode != ColorBindingMode.Callback && entry.Target == null)
                    {
                        entries[i].Active = false;
                        continue;
                    }

                    var dt = entry.UseUnscaledTime ? unscaledDeltaTime : deltaTime;

                    if (entry.DelayRemaining > 0f)
                    {
                        entry.DelayRemaining -= dt;
                        if (entry.DelayRemaining > 0f) continue;
                        dt = -entry.DelayRemaining;
                        FireStart(ref entry);
                        Apply(ref entry, 0f);
                    }

                    if (entry.Duration <= 0f)
                    {
                        Apply(ref entry, 1f);
                        Finish(i);
                        continue;
                    }

                    entry.Elapsed += dt;

                    if (entry.Elapsed >= entry.Duration)
                    {
                        if (ShouldFinish(entry.Loops, entry.LoopsPlayed))
                        {
                            Apply(ref entry, 1f);
                            Finish(i);
                        }
                        else
                        {
                            entry.LoopsPlayed++;
                            var overflow = entry.Elapsed - entry.Duration;
                            entry.Elapsed = overflow > 0f ? overflow : 0f;
                            if (entry.LoopType == LoopType.PingPong)
                                (entry.From, entry.To) = (entry.To, entry.From);
                            Apply(ref entry, entry.Elapsed / entry.Duration);
                        }
                    }
                    else
                    {
                        Apply(ref entry, entry.Elapsed / entry.Duration);
                    }
                }
            }

            public static bool IsActive(int index, uint version) => IsValid(index, version) && entries[index].Active;
            public static bool IsPaused(int index, uint version) => IsValid(index, version) && entries[index].Paused;
            public static void Cancel(int index, uint version) { if (IsValid(index, version)) entries[index].Active = false; }
            public static void Complete(int index, uint version) { if (IsValid(index, version) && entries[index].Active) { Apply(ref entries[index], 1f); Finish(index); } }
            public static void SetPaused(int index, uint version, bool paused) { if (IsValid(index, version) && entries[index].Active) entries[index].Paused = paused; }

            static void FireStart(ref Entry entry)
            {
                if (entry.StartFired) return;
                entry.StartFired = true;
                entry.OnStart?.Invoke();
            }

            static void Apply(ref Entry entry, float progress)
            {
                var value = Color.LerpUnclamped(entry.From, entry.To, EaseUtility.Evaluate(progress, entry.Ease, entry.Curve));
                switch (entry.BindingMode)
                {
                    case ColorBindingMode.SpriteRendererColor:
                        if (entry.Target is SpriteRenderer sr) sr.color = value;
                        break;
                    case ColorBindingMode.GraphicColor:
                        if (entry.Target is Graphic gr) gr.color = value;
                        break;
                }
                entry.OnValue?.Invoke(value);
            }

            static void Finish(int index)
            {
                var callback = entries[index].OnComplete;
                entries[index].Active = false;
                entries[index].Paused = false;
                entries[index].StartFired = false;
                entries[index].OnValue = null;
                entries[index].OnStart = null;
                entries[index].OnComplete = null;
                entries[index].Curve = null;
                entries[index].Target = null;
                entries[index].LoopsPlayed = 0;
                callback?.Invoke();
            }

            static bool TryAcquireSlot(out int index)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Active) continue;
                    entries[i].Version++;
                    index = i;
                    return true;
                }
                index = -1;
                return false;
            }

            static bool IsValid(int index, uint version) =>
                index >= 0 && index < entries.Length && entries[index].Version == version;
        }

        // =====================================================================
        // QuaternionStorage
        // =====================================================================

        static class QuaternionStorage
        {
            struct Entry
            {
                public bool Active;
                public bool Paused;
                public bool StartFired;
                public uint Version;
                public Quaternion From;
                public Quaternion To;
                public float Duration;
                public float Elapsed;
                public float DelayRemaining;
                public Ease Ease;
                public bool UseUnscaledTime;
                public AnimationCurve Curve;
                public Action<Quaternion> OnValue;
                public Action OnStart;
                public Action OnComplete;
                public QuaternionBindingMode BindingMode;
                public Transform Target;
                public int Loops;
                public int LoopsPlayed;
                public LoopType LoopType;
            }

            static readonly Entry[] entries = new Entry[CapacityPerType];

            public static TweenHandle Add(ref QuaternionTweenBuilder builder, QuaternionBindingMode bindingMode, Transform target)
            {
                if (!TryAcquireSlot(out var index))
                {
                    Debug.LogError("Tween Quaternion storage is full.");
                    return default;
                }

                ref var entry = ref entries[index];
                entry.Active = true;
                entry.Paused = false;
                entry.StartFired = false;
                entry.From = builder.From;
                entry.To = builder.To;
                entry.Duration = builder.Duration;
                entry.Elapsed = 0f;
                entry.DelayRemaining = builder.Settings.Delay;
                entry.Ease = builder.Settings.Ease;
                entry.UseUnscaledTime = builder.Settings.UseUnscaledTime;
                entry.Curve = builder.Settings.CustomCurve;
                entry.OnValue = builder.OnValue;
                entry.OnStart = builder.Settings.OnStart;
                entry.OnComplete = builder.Settings.OnComplete;
                entry.BindingMode = bindingMode;
                entry.Target = target;
                entry.Loops = builder.Settings.Loops == 0 ? 1 : builder.Settings.Loops;
                entry.LoopsPlayed = 0;
                entry.LoopType = builder.Settings.LoopType;

                if (entry.Duration <= 0f && entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 1f);
                    Finish(index);
                }
                else if (entry.DelayRemaining <= 0f)
                {
                    FireStart(ref entry);
                    Apply(ref entry, 0f);
                }

                return new TweenHandle(TweenValueKind.Quaternion, index, entry.Version);
            }

            public static void Update(float deltaTime, float unscaledDeltaTime)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (!entries[i].Active || entries[i].Paused) continue;
                    ref var entry = ref entries[i];

                    if (entry.BindingMode != QuaternionBindingMode.Callback && entry.Target == null)
                    {
                        entries[i].Active = false;
                        continue;
                    }

                    var dt = entry.UseUnscaledTime ? unscaledDeltaTime : deltaTime;

                    if (entry.DelayRemaining > 0f)
                    {
                        entry.DelayRemaining -= dt;
                        if (entry.DelayRemaining > 0f) continue;
                        dt = -entry.DelayRemaining;
                        FireStart(ref entry);
                        Apply(ref entry, 0f);
                    }

                    if (entry.Duration <= 0f)
                    {
                        Apply(ref entry, 1f);
                        Finish(i);
                        continue;
                    }

                    entry.Elapsed += dt;

                    if (entry.Elapsed >= entry.Duration)
                    {
                        if (ShouldFinish(entry.Loops, entry.LoopsPlayed))
                        {
                            Apply(ref entry, 1f);
                            Finish(i);
                        }
                        else
                        {
                            entry.LoopsPlayed++;
                            var overflow = entry.Elapsed - entry.Duration;
                            entry.Elapsed = overflow > 0f ? overflow : 0f;
                            if (entry.LoopType == LoopType.PingPong)
                                (entry.From, entry.To) = (entry.To, entry.From);
                            Apply(ref entry, entry.Elapsed / entry.Duration);
                        }
                    }
                    else
                    {
                        Apply(ref entry, entry.Elapsed / entry.Duration);
                    }
                }
            }

            public static bool IsActive(int index, uint version) => IsValid(index, version) && entries[index].Active;
            public static bool IsPaused(int index, uint version) => IsValid(index, version) && entries[index].Paused;
            public static void Cancel(int index, uint version) { if (IsValid(index, version)) entries[index].Active = false; }
            public static void Complete(int index, uint version) { if (IsValid(index, version) && entries[index].Active) { Apply(ref entries[index], 1f); Finish(index); } }
            public static void SetPaused(int index, uint version, bool paused) { if (IsValid(index, version) && entries[index].Active) entries[index].Paused = paused; }

            static void FireStart(ref Entry entry)
            {
                if (entry.StartFired) return;
                entry.StartFired = true;
                entry.OnStart?.Invoke();
            }

            static void Apply(ref Entry entry, float progress)
            {
                var value = Quaternion.SlerpUnclamped(entry.From, entry.To, EaseUtility.Evaluate(progress, entry.Ease, entry.Curve));
                var target = entry.Target;
                if (target != null)
                {
                    switch (entry.BindingMode)
                    {
                        case QuaternionBindingMode.Rotation: target.rotation = value; break;
                        case QuaternionBindingMode.LocalRotation: target.localRotation = value; break;
                    }
                }
                entry.OnValue?.Invoke(value);
            }

            static void Finish(int index)
            {
                var callback = entries[index].OnComplete;
                entries[index].Active = false;
                entries[index].Paused = false;
                entries[index].StartFired = false;
                entries[index].OnValue = null;
                entries[index].OnStart = null;
                entries[index].OnComplete = null;
                entries[index].Curve = null;
                entries[index].Target = null;
                entries[index].LoopsPlayed = 0;
                callback?.Invoke();
            }

            static bool TryAcquireSlot(out int index)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Active) continue;
                    entries[i].Version++;
                    index = i;
                    return true;
                }
                index = -1;
                return false;
            }

            static bool IsValid(int index, uint version) =>
                index >= 0 && index < entries.Length && entries[index].Version == version;
        }
    }
}