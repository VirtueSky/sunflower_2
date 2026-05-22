# Tween

Simple custom tween module for Unity.

Namespace: `VirtueSky.Tweening`

## Goal

This module provides a small tween API with a LitMotion-like flow:

`Tween.Create(...).WithEase(...).BindTo...`

Designed to keep the tween core low-allocation / zero-GC on runtime when using the built-in bind helpers.

---

## Main API

### Entry points

```csharp
Tween.Create(from, to, duration)   // returns a typed builder
Tween.Delay(duration, onComplete)  // fire-and-forget timer
```

Supported value types: `float`, `Vector2`, `Vector3`, `Vector4`, `Color`, `Quaternion`

### Builder chain methods (all types)

| Method | Description |
|---|---|
| `WithEase(Ease)` | Set easing function |
| `WithDelay(float)` | Delay before starting |
| `WithUnscaledTime(bool)` | Use `Time.unscaledDeltaTime` |
| `WithCurve(AnimationCurve)` | Custom animation curve |
| `WithOnStart(Action)` | Callback fired once when tween starts (after delay) |
| `WithOnComplete(Action)` | Callback fired when tween finishes |
| `WithLoops(int)` | Number of loops: `-1` = infinite, `1` = once (default), `N` = N times |
| `WithLoopType(LoopType)` | `LoopType.Restart` (default) or `LoopType.PingPong` |
| `OnValueChanged(Action<T>)` | Callback fired every frame with current value (same as DOTween's `OnUpdate`) |
| `Bind(Action<T>)` | `OnValueChanged` + `Play()` combined — terminates the builder |
| `Play()` | Start the tween, returns a `TweenHandle` |

### TweenHandle

```csharp
handle.IsActive   // true if still running
handle.IsPaused   // true if paused
handle.Cancel()   // stop immediately, no OnComplete
handle.Complete() // jump to end value, fires OnComplete
handle.Pause()    // freeze in place
handle.Resume()   // continue from where it paused
```

---

## Quick Start

```csharp
using VirtueSky.Tweening;

// Scale popup
TweenHandle handle = Tween.Create(Vector3.zero, Vector3.one, 0.25f)
    .WithEase(Ease.OutBack)
    .BindToLocalScale(transform);

// Cancel on destroy
void OnDisable() => handle.Cancel();
```

---

## Tween.Delay

```csharp
// Fire a callback after 1.5 seconds
Tween.Delay(1.5f, () => ShowNextPanel());

// With unscaled time (works when Time.timeScale == 0)
Tween.Delay(0.3f, () => HidePauseMenu(), unscaledTime: true);

// Cancellable
TweenHandle timer = Tween.Delay(5f, OnCountdownEnd);
timer.Cancel(); // cancel before it fires
```

---

## Loop

```csharp
// Infinite loop — bounce scale
Tween.Create(Vector3.one, Vector3.one * 1.1f, 0.5f)
    .WithLoops(-1)
    .WithLoopType(LoopType.PingPong)
    .WithEase(Ease.InOutSine)
    .BindToLocalScale(transform);

// Play exactly 3 times then stop
Tween.Create(0f, 1f, 0.4f)
    .WithLoops(3)
    .BindToFillAmount(image);
```

---

## OnStart / OnComplete

```csharp
Tween.Create(0f, 1f, 0.5f)
    .WithDelay(1f)
    .WithOnStart(() => Debug.Log("started after 1s delay"))
    .WithOnComplete(() => Debug.Log("done"))
    .BindToAlpha(canvasGroup);
```

---

## Pause / Resume

```csharp
TweenHandle handle = Tween.Create(startPos, endPos, 2f)
    .BindToLocalPosition(transform);

// On game pause
handle.Pause();

// On game resume
handle.Resume();

if (handle.IsPaused) { ... }
```

---

## Available Bind Helpers

### `Vector3TweenBuilder`

| Method | Target |
|---|---|
| `BindToPosition(Transform)` | `transform.position` |
| `BindToLocalPosition(Transform)` | `transform.localPosition` |
| `BindToLocalScale(Transform)` | `transform.localScale` |
| `BindToEulerAngles(Transform)` | `transform.eulerAngles` |
| `BindToLocalEulerAngles(Transform)` | `transform.localEulerAngles` |

### `QuaternionTweenBuilder`

| Method | Target |
|---|---|
| `BindToRotation(Transform)` | `transform.rotation` |
| `BindToLocalRotation(Transform)` | `transform.localRotation` |

### `ColorTweenBuilder`

| Method | Target |
|---|---|
| `BindToColor(SpriteRenderer)` | `spriteRenderer.color` |
| `BindToColor(Graphic)` | `graphic.color` |

### `FloatTweenBuilder`

| Method | Target |
|---|---|
| `BindToAlpha(CanvasGroup)` | `canvasGroup.alpha` |
| `BindToFillAmount(Image)` | `image.fillAmount` |
| `BindToLocalPositionX(Transform)` | `localPosition.x` |
| `BindToLocalPositionY(Transform)` | `localPosition.y` |
| `BindToLocalPositionZ(Transform)` | `localPosition.z` |
| `BindToAnchoredPositionX(RectTransform)` | `anchoredPosition.x` |
| `BindToAnchoredPositionY(RectTransform)` | `anchoredPosition.y` |

### `Vector2TweenBuilder`

| Method | Target |
|---|---|
| `BindToAnchoredPosition(RectTransform)` | `anchoredPosition` |
| `BindToSizeDelta(RectTransform)` | `sizeDelta` |

---

## Examples

### Move (world space)

```csharp
Tween.Create(transform.position, new Vector3(0f, 3f, 0f), 0.5f)
    .WithEase(Ease.OutCubic)
    .BindToPosition(transform);
```

### Scale

```csharp
Tween.Create(Vector3.one, Vector3.one * 1.1f, 0.2f)
    .WithEase(Ease.OutBack)
    .BindToLocalScale(transform);
```

### Rotate

```csharp
Tween.Create(transform.localRotation, Quaternion.Euler(0f, 180f, 0f), 0.4f)
    .WithEase(Ease.InOutSine)
    .BindToLocalRotation(transform);
```

### Fade CanvasGroup

```csharp
Tween.Create(0f, 1f, 0.25f)
    .BindToAlpha(canvasGroup);
```

### UI slide in

```csharp
Tween.Create(new Vector2(0f, -200f), Vector2.zero, 0.35f)
    .WithEase(Ease.OutQuart)
    .BindToAnchoredPosition(rectTransform);
```

### Resize panel

```csharp
Tween.Create(new Vector2(100f, 100f), new Vector2(400f, 300f), 0.3f)
    .WithEase(Ease.OutCubic)
    .BindToSizeDelta(rectTransform);
```

### Heartbeat loop

```csharp
Tween.Create(Vector3.one, Vector3.one * 1.15f, 0.3f)
    .WithLoops(-1)
    .WithLoopType(LoopType.PingPong)
    .WithEase(Ease.InOutSine)
    .BindToLocalScale(transform);
```

### Delayed chain (manual)

```csharp
Tween.Create(0f, 1f, 0.3f)
    .WithOnComplete(() =>
        Tween.Delay(0.5f, () =>
            Tween.Create(1f, 0f, 0.3f).BindToAlpha(canvasGroup)))
    .BindToAlpha(canvasGroup);
```

---

## Zero-GC Usage

Prefer built-in `BindTo...` helpers. They route through the internal binding path and avoid delegate allocation at call site.

```csharp
// Recommended (zero-GC at call site)
Tween.Create(transform.position, target, 0.3f)
    .BindToPosition(transform);

// Allocates a delegate/closure
Tween.Create(transform.position, target, 0.3f)
    .Bind(v => transform.position = v);
```

`WithOnComplete` and `WithOnStart` can also allocate if the lambda captures outer variables.

---

## Ease

`Linear` · `InSine` · `OutSine` · `InOutSine` · `InQuad` · `OutQuad` · `InOutQuad` · `InCubic` · `OutCubic` · `InOutCubic` · `InQuart` · `OutQuart` · `InOutQuart` · `InQuint` · `OutQuint` · `InOutQuint` · `InExpo` · `OutExpo` · `InOutExpo` · `InCirc` · `OutCirc` · `InOutCirc` · `InElastic` · `OutElastic` · `InOutElastic` · `InBack` · `OutBack` · `InOutBack` · `InBounce` · `OutBounce` · `InOutBounce` · `CustomAnimationCurve`

---

## Notes

- Storage is fixed-size: **256 tweens per value type**. Full storage logs an error instead of resizing.
- A hidden `[VirtueSky.TweenRunner]` GameObject is created automatically on first use (`DontDestroyOnLoad`).
- Auto-cancel: if a bound target (Transform, RectTransform, etc.) is destroyed mid-tween, the entry deactivates automatically on the next frame.
- `WithLoops(-1)` with `LoopType.PingPong` swaps `From`/`To` each cycle in-place on the entry struct.

---

## Files

| File | Role |
|---|---|
| `Tween.cs` | Entry API (`Tween.Create`, `Tween.Delay`) |
| `TweenBuilder.cs` | Builder structs, `LoopType` enum, bind extensions |
| `TweenHandle.cs` | Handle struct (cancel / complete / pause / resume) |
| `TweenRuntime.cs` | Internal runtime and per-type storage |
| `TweenRunner.cs` | MonoBehaviour driver |
| `Ease.cs` | Ease enum |
| `EaseUtility.cs` | Ease evaluation |