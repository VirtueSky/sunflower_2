using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Tweening;

namespace VirtueSky.Component
{
    [EditorIcon("icon_csharp"), HideMonoScript]
    public class EffectAppearComponent : BaseMono
    {
        public float delay = 0.1f;
        [Range(0, 2f)] public float TimeScale = .7f;
        public Ease ease = Ease.OutBack;
        public Vector3 fromScale = new Vector3(.5f, .5f, .5f);
        private Vector3 CurrentScale;
        private TweenHandle _tween;

        public void Awake()
        {
            CurrentScale = transform.localScale;
        }

        public void OnEnable()
        {
            transform.localScale = fromScale;
            App.Delay(this, delay, DoEffect);
        }

        public void DoEffect()
        {
            if (!gameObject.activeInHierarchy) return;
            _tween = Tween.Create(fromScale, CurrentScale, TimeScale).WithEase(ease).WithOnComplete(() =>
                {
                    if (_tween.IsActive)
                    {
                        _tween.Complete();
                    }
                })
                .BindToLocalScale(transform);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (_tween.IsActive)
            {
                _tween.Cancel();
            }
        }
    }
}