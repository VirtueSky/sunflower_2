using UnityEngine;

namespace VirtueSky.Tweening
{
    public static class EaseUtility
    {
        public static float Evaluate(float t, Ease ease, AnimationCurve customCurve = null)
        {
            t = Mathf.Clamp01(t);

            if (ease == Ease.CustomAnimationCurve)
            {
                return customCurve == null ? t : customCurve.Evaluate(t);
            }

            return ease switch
            {
                Ease.InSine => 1f - Mathf.Cos(t * Mathf.PI * 0.5f),
                Ease.OutSine => Mathf.Sin(t * Mathf.PI * 0.5f),
                Ease.InOutSine => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f,
                Ease.InQuad => t * t,
                Ease.OutQuad => 1f - (1f - t) * (1f - t),
                Ease.InOutQuad => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f,
                Ease.InCubic => t * t * t,
                Ease.OutCubic => 1f - Mathf.Pow(1f - t, 3f),
                Ease.InOutCubic => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f,
                Ease.InQuart => t * t * t * t,
                Ease.OutQuart => 1f - Mathf.Pow(1f - t, 4f),
                Ease.InOutQuart => t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) * 0.5f,
                Ease.InQuint => t * t * t * t * t,
                Ease.OutQuint => 1f - Mathf.Pow(1f - t, 5f),
                Ease.InOutQuint => t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) * 0.5f,
                Ease.InExpo => Mathf.Approximately(t, 0f) ? 0f : Mathf.Pow(2f, 10f * t - 10f),
                Ease.OutExpo => Mathf.Approximately(t, 1f) ? 1f : 1f - Mathf.Pow(2f, -10f * t),
                Ease.InOutExpo => InOutExpo(t),
                Ease.InCirc => 1f - Mathf.Sqrt(1f - t * t),
                Ease.OutCirc => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f)),
                Ease.InOutCirc => InOutCirc(t),
                Ease.InElastic => InElastic(t),
                Ease.OutElastic => OutElastic(t),
                Ease.InOutElastic => InOutElastic(t),
                Ease.InBack => InBack(t),
                Ease.OutBack => OutBack(t),
                Ease.InOutBack => InOutBack(t),
                Ease.InBounce => 1f - OutBounce(1f - t),
                Ease.OutBounce => OutBounce(t),
                Ease.InOutBounce => InOutBounce(t),
                _ => t
            };
        }

        static float InOutExpo(float t)
        {
            if (Mathf.Approximately(t, 0f)) return 0f;
            if (Mathf.Approximately(t, 1f)) return 1f;
            return t < 0.5f
                ? Mathf.Pow(2f, 20f * t - 10f) * 0.5f
                : (2f - Mathf.Pow(2f, -20f * t + 10f)) * 0.5f;
        }

        static float InOutCirc(float t)
        {
            return t < 0.5f
                ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) * 0.5f
                : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) * 0.5f;
        }

        static float InBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        static float OutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        static float InOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5f
                ? Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2) * 0.5f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (2f * t - 2f) + c2) + 2f) * 0.5f;
        }

        static float InElastic(float t)
        {
            const float c4 = 2f * Mathf.PI / 3f;
            if (Mathf.Approximately(t, 0f)) return 0f;
            if (Mathf.Approximately(t, 1f)) return 1f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((10f * t - 10.75f) * c4);
        }

        static float OutElastic(float t)
        {
            const float c4 = 2f * Mathf.PI / 3f;
            if (Mathf.Approximately(t, 0f)) return 0f;
            if (Mathf.Approximately(t, 1f)) return 1f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((10f * t - 0.75f) * c4) + 1f;
        }

        static float InOutElastic(float t)
        {
            const float c5 = 2f * Mathf.PI / 4.5f;
            if (Mathf.Approximately(t, 0f)) return 0f;
            if (Mathf.Approximately(t, 1f)) return 1f;
            return t < 0.5f
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) * 0.5f
                : Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5) * 0.5f + 1f;
        }

        static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        static float InOutBounce(float t)
        {
            return t < 0.5f
                ? (1f - OutBounce(1f - 2f * t)) * 0.5f
                : (1f + OutBounce(2f * t - 1f)) * 0.5f;
        }
    }
}
