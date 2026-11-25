using System;
using UnityEngine;
using UnityEngine.Events;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Tweening;

namespace VirtueSky.Audio
{
    [RequireComponent(typeof(AudioSource))]
    [EditorIcon("icon_csharp")]
    public class SoundComponent : CacheComponent<AudioSource>
    {
        [ReadOnly, SerializeField] private int key;
        private DelayHandle completeDelay;
        private TweenHandle fadeTween;
        private bool isPaused;
        private int spawnVersion;
        public event UnityAction<SoundComponent> OnCompleted;
        public event UnityAction<SoundComponent> OnPaused;
        public event UnityAction<SoundComponent> OnResumed;
        public event UnityAction<SoundComponent> OnStopped;

        public AudioClip GetClip => component.clip;
        public bool IsPlaying => component.isPlaying;
        public bool IsLooping => component.loop;

        /// <summary>
        /// Paused by Pause() (user intent). Unlike !IsPlaying, this is not affected by AudioListener.pause (ads)
        /// or by the clip having stopped.
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// Changes every time this component is returned to the pool, so an owner can detect that
        /// its reference is stale (despawned elsewhere, possibly reused for another sound).
        /// </summary>
        public int SpawnVersion => spawnVersion;

        public float Volume
        {
            get => component.volume;
            set => component.volume = value;
        }

        public int Key
        {
            get => key;
            set => key = value;
        }

        protected override void Awake()
        {
            base.Awake(); // caches `component` if the prefab did not serialize it
            component.playOnAwake = false;
        }

        private void OnDestroy()
        {
            // Callback tweens have no target, so they are not auto-cancelled when this object is destroyed.
            CancelFade();
        }

        internal bool PlayAudioClip(AudioClip audioClip, bool isLooping, float volume)
        {
            if (audioClip == null)
            {
                Debug.LogError($"AudioClip is null");
                return false;
            }

            CancelFade();
            isPaused = false;
            component.clip = audioClip;
            component.loop = isLooping;
            component.volume = volume;
            component.time = 0;
            component.Play();
            if (isLooping) CancelCompleteDelay();
            else ScheduleComplete(audioClip.length);
            return true;
        }

        void FadeInVolumeMusic(AudioClip audioClip, bool isLooping, float endValue, float duration)
        {
            if (!PlayAudioClip(audioClip, isLooping, 0)) return;
            // Unscaled: AudioSource ignores Time.timeScale, so the fade must too (timeScale = 0 on app pause/popups).
            fadeTween = Tween.Create(0, endValue, duration).WithUnscaledTime()
                .Bind(value => component.volume = value);
        }

        void FadeOutVolumeMusic(float duration, Action fadeCompleted)
        {
            CancelFade();
            float startValue = component.volume;
            fadeTween = Tween.Create(startValue, 0, duration).WithUnscaledTime().WithOnComplete(fadeCompleted)
                .Bind(value => component.volume = value);
        }

        void CancelFade()
        {
            // Cancel does not invoke OnComplete, so a pending fade-out will not start the next clip.
            fadeTween.Cancel();
            fadeTween = default;
        }


        internal void Resume()
        {
            isPaused = false;
            OnResumed?.Invoke(this);
            component.UnPause();
            App.ResumeDelay(completeDelay);
            fadeTween.Resume();
        }

        internal void Pause()
        {
            isPaused = true;
            OnPaused?.Invoke(this);
            component.Pause();
            App.PauseDelay(completeDelay);
            // Otherwise a pending fade-out would complete and start the next clip while paused.
            fadeTween.Pause();
        }

        internal void Stop()
        {
            OnStopped?.Invoke(this);
            component.Stop();
            CancelCompleteDelay();
            CancelFade();
            isPaused = false;
        }

        internal void Finish()
        {
            if (!component.loop) return;
            component.loop = false;
            ScheduleComplete(component.clip.length - component.time);
            // Keep the completion timer frozen while paused; it resumes together with the audio.
            if (isPaused) App.PauseDelay(completeDelay);
        }

        /// <summary>
        /// Called by the pool right before this object is returned (DeSpawn).
        /// The DelayHandle is only auto-cancelled when the owner is destroyed, not when it is pooled,
        /// so it must be cancelled here, otherwise it would fire on the next reuse of this component.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            CancelCompleteDelay();
            CancelFade();
            isPaused = false;
            spawnVersion++;
            OnCompleted = null;
            OnPaused = null;
            OnResumed = null;
            OnStopped = null;
            key = 0;
        }

        /// <summary>
        /// Schedule a completion check after <paramref name="clipSeconds"/> of audio (clip time, before pitch).
        /// Uses real time so Time.timeScale (game pause / slow-mo) does not affect it, since AudioSource ignores timeScale.
        /// </summary>
        void ScheduleComplete(float clipSeconds)
        {
            CancelCompleteDelay();
            float pitch = Mathf.Max(0.01f, Mathf.Abs(component.pitch));
            completeDelay = App.Delay(this, Mathf.Max(0f, clipSeconds) / pitch, CheckCompleted, useRealTime: true);
        }

        void CancelCompleteDelay()
        {
            App.CancelDelay(completeDelay);
            completeDelay = null;
        }

        /// <summary>
        /// The delay is only a "check-in" point; the AudioSource state decides whether the clip really ended.
        /// Covers cases where real time advanced but audio did not: AudioListener.pause (ads),
        /// app backgrounded on mobile, audio thread hiccups.
        /// </summary>
        void CheckCompleted()
        {
            completeDelay = null;

            // Global audio pause: isPlaying is false but the clip has not ended. Poll until unpaused.
            if (AudioListener.pause)
            {
                completeDelay = App.Delay(this, 0.25f, CheckCompleted, useRealTime: true);
                return;
            }

            // Still playing (audio fell behind real time): wait for the actual remainder.
            if (component.isPlaying && component.clip != null)
            {
                ScheduleComplete(component.clip.length - component.time);
                return;
            }

            OnCompletedInvoke();
        }

        internal void FadePlayMusic(AudioClip audioClip, bool isLooping, float volume, bool isMusicFadeVolume,
            float durationOut,
            float durationIn)
        {
            // Drop any fade still running from a previous PlayMusic call (and its pending "play next clip" callback).
            CancelFade();
            if (isMusicFadeVolume && volume != 0)
            {
                // Only fade out something audible. A paused clip (Pause() or AudioListener.pause) is switched
                // directly; otherwise it would have to be unpaused just to be faded out.
                if (component.isPlaying && !isPaused)
                {
                    // The current clip is being replaced: it must not "complete" mid fade-out, which would
                    // despawn this component and drop the next clip.
                    CancelCompleteDelay();
                    FadeOutVolumeMusic(durationOut,
                        () => { FadeInVolumeMusic(audioClip, isLooping, volume, durationIn); });
                }
                else
                {
                    FadeInVolumeMusic(audioClip, isLooping, volume, durationIn);
                }
            }
            else
            {
                PlayAudioClip(audioClip, isLooping, volume);
            }
        }

        private void OnCompletedInvoke()
        {
            OnCompleted?.Invoke(this);
        }
    }
}