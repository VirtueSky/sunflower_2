using System;
using UnityEngine;

namespace VirtueSky.Tweening
{
    public static class Tween
    {
        public static FloatTweenBuilder Create(float from, float to, float duration) => new FloatTweenBuilder(from, to, duration);
        public static Vector2TweenBuilder Create(Vector2 from, Vector2 to, float duration) => new Vector2TweenBuilder(from, to, duration);
        public static Vector3TweenBuilder Create(Vector3 from, Vector3 to, float duration) => new Vector3TweenBuilder(from, to, duration);
        public static Vector4TweenBuilder Create(Vector4 from, Vector4 to, float duration) => new Vector4TweenBuilder(from, to, duration);
        public static ColorTweenBuilder Create(Color from, Color to, float duration) => new ColorTweenBuilder(from, to, duration);
        public static QuaternionTweenBuilder Create(Quaternion from, Quaternion to, float duration) => new QuaternionTweenBuilder(from, to, duration);

        /// <summary>Chờ <paramref name="duration"/> giây rồi gọi <paramref name="onComplete"/>.</summary>
        public static TweenHandle Delay(float duration, Action onComplete, bool unscaledTime = false)
            => new FloatTweenBuilder(0f, 0f, duration)
                .WithUnscaledTime(unscaledTime)
                .WithOnComplete(onComplete)
                .Play();
    }
}