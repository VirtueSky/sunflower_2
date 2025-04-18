﻿using PrimeTween;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;


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
        private Tween tween;
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
            tween.Stop();
        }

        public void Stop()
        {
            isBreak = true;
            tween.Stop();
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
                    tween = transform.Scale(
                            new Vector3(currentScale.x + offsetScale, currentScale.y + offsetScale,
                                currentScale.z + offsetScale), timeScale, ease)
                        .OnComplete(() => { DoEffect(-offsetScale, !delay); }, false);
                });
        }
    }
}