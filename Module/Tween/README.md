# Tween

Simple custom tween module for Unity.

Namespace:

`VirtueSky.Tweening`

## Goal

This module provides a small tween API with a LitMotion-like flow:

`Tween.Create(...).WithEase(...).Bind...`

It is designed to keep the tween core low-allocation / zero-GC on runtime when using the built-in bind helpers.

## Main API

Entry point:

`Tween.Create(from, to, duration)`

Supported value types:

- `float`
- `Vector2`
- `Vector3`
- `Vector4`
- `Color`
- `Quaternion`

Common chain methods:

- `WithEase(Ease ease)`
- `WithDelay(float delay)`
- `WithUnscaledTime(bool value = true)`
- `WithCurve(AnimationCurve curve)`
- `WithOnComplete(Action callback)`
- `OnValueChanged(Action<T> callback)`
- `Bind(Action<T> callback)`
- `Play()`

Handle API:

- `handle.IsActive`
- `handle.Cancel()`
- `handle.Complete()`

## Quick Start

```csharp
using UnityEngine;
using VirtueSky.Tweening;

public class TweenExample : MonoBehaviour
{
    private TweenHandle handle;

    private void Start()
    {
        handle = Tween.Create(transform.localScale, Vector3.one * 1.2f, 0.25f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(transform);
    }

    private void OnDisable()
    {
        handle.Cancel();
    }
}
```

## Zero-GC Usage

To keep usage zero-GC, prefer the built-in bind helpers instead of `Bind(value => ...)`.

Recommended:

```csharp
Tween.Create(transform.position, targetPosition, 0.3f)
    .WithEase(Ease.OutQuad)
    .BindToPosition(transform);
```

Avoid if you want strict zero-GC:

```csharp
Tween.Create(transform.position, targetPosition, 0.3f)
    .Bind(value => transform.position = value);
```

Reason:

- `BindTo...` uses the tween runtime's internal binding path.
- `Bind(...)` may allocate because of delegate/lambda usage.
- `WithOnComplete(() => ...)` may also allocate if the callback captures outer variables.

## Available Bind Helpers

### `Vector3TweenBuilder`

- `BindToPosition(Transform target)`
- `BindToLocalPosition(Transform target)`
- `BindToLocalScale(Transform target)`
- `BindToEulerAngles(Transform target)`
- `BindToLocalEulerAngles(Transform target)`

### `QuaternionTweenBuilder`

- `BindToRotation(Transform target)`
- `BindToLocalRotation(Transform target)`

### `ColorTweenBuilder`

- `BindToColor(SpriteRenderer target)`
- `BindToColor(UnityEngine.UI.Graphic target)`

### `FloatTweenBuilder`

- `BindToAlpha(CanvasGroup target)`
- `BindToFillAmount(UnityEngine.UI.Image target)`

## Examples

### Move

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
    .WithEase(Ease.Linear)
    .BindToAlpha(canvasGroup);
```

### Fill Amount

```csharp
Tween.Create(0f, 1f, 0.5f)
    .WithEase(Ease.OutQuad)
    .BindToFillAmount(image);
```

## Ease

The module includes common ease presets:

- `Linear`
- `InSine`, `OutSine`, `InOutSine`
- `InQuad`, `OutQuad`, `InOutQuad`
- `InCubic`, `OutCubic`, `InOutCubic`
- `InQuart`, `OutQuart`, `InOutQuart`
- `InQuint`, `OutQuint`, `InOutQuint`
- `InExpo`, `OutExpo`, `InOutExpo`
- `InCirc`, `OutCirc`, `InOutCirc`
- `InElastic`, `OutElastic`, `InOutElastic`
- `InBack`, `OutBack`, `InOutBack`
- `InBounce`, `OutBounce`, `InOutBounce`
- `CustomAnimationCurve`

## Notes

- Runtime storage is fixed-size per value type.
- Current capacity is `256` tweens per type.
- If a storage bucket is full, the runtime logs an error instead of resizing, to avoid runtime allocations.
- A hidden runner object is created automatically on first use.

## Files

- `Tween.cs`: entry API
- `TweenBuilder.cs`: builder structs and bind extensions
- `TweenHandle.cs`: tween handle
- `TweenRuntime.cs`: internal runtime and storage
- `Ease.cs`: ease enum
- `EaseUtility.cs`: ease evaluation
