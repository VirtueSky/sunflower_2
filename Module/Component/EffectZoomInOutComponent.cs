using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Tweening;


namespace VirtueSky.Component
{
    [EditorIcon("icon_csharp"), HideMonoScript]
    public class EffectZoomInOutComponent : BaseMono
    {
        public bool playOnAwake = true;
        [Range(0, 2f)] public float timeDelay;
        [Range(0, 2f)] public float offsetScale = .1f;
        [Range(0, 2f)] public float timeScale = .7f;
        public Ease ease = Ease.Linear;
        private Vector3 currentScale;
        private TweenHandle tween;
        private bool isBreak = false;

        public void Awake()
        {
            currentScale = transform.localScale;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (playOnAwake)
            {
                Play();
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (tween.IsActive)
            {
                tween.Cancel();
            }
        }

        public void Stop()
        {
            isBreak = true;
            if (tween.IsActive)
            {
                tween.Complete();
            }
        }

        public void Play()
        {
            isBreak = false;
            DoEffect(offsetScale, false);
        }

        public void DoEffect(float offsetScale, bool delay)
        {
            if (!gameObject.activeInHierarchy) return;
            if (isBreak) return;
            App.Delay(this, timeDelay * (delay ? 1 : 0),
                () =>
                {
                    Vector3 targetScale = new Vector3(currentScale.x + offsetScale, currentScale.y + offsetScale,
                        currentScale.z + offsetScale);
                    Vector3 fromScale = transform.localScale;
                    tween = Tween.Create(fromScale, targetScale, timeScale).WithEase(ease).WithOnComplete(() =>
                        {
                            if (tween.IsActive)
                            {
                                tween.Complete();
                            }

                            DoEffect(-offsetScale, !delay);
                        })
                        .BindToLocalScale(transform);
                });
        }
    }
}