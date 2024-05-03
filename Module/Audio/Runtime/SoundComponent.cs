using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using VirtueSky.Core;
using VirtueSky.Inspector;

namespace VirtueSky.Audio
{
    [RequireComponent(typeof(AudioSource))]
    [EditorIcon("icon_csharp")]
    public class SoundComponent : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        public event UnityAction<SoundComponent> OnCompleted;
        public event UnityAction<SoundComponent> OnPaused;
        public event UnityAction<SoundComponent> OnResumed;
        public event UnityAction<SoundComponent> OnStopped;

        public AudioClip GetClip => audioSource.clip;
        public bool IsPlaying => audioSource.isPlaying;
        public bool IsLooping => audioSource.loop;

        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = value;
        }

        private void Awake()
        {
            audioSource.playOnAwake = false;
        }

        internal void PlayAudioClip(AudioClip audioClip, bool isLooping, float volume)
        {
            audioSource.clip = audioClip;
            audioSource.loop = isLooping;
            audioSource.volume = volume;
            audioSource.time = 0;
            audioSource.Play();
            if (!isLooping)
            {
                App.Delay(this, audioClip.length, OnCompletedInvoke);
            }
        }

        void FadeInVolumeMusic(AudioClip audioClip, bool isLooping, float endValue, float duration)
        {
            PlayAudioClip(audioClip, isLooping, 0);
            Tween.AudioVolume(audioSource, endValue, duration);
        }

        void FadeOutVolumeMusic(float duration, Action fadeCompleted)
        {
            Tween.AudioVolume(audioSource, 0, duration).OnComplete(fadeCompleted);
        }


        internal void Resume()
        {
            OnResumed?.Invoke(this);
            audioSource.UnPause();
        }

        internal void Pause()
        {
            OnPaused?.Invoke(this);
            audioSource.Pause();
        }

        internal void Stop()
        {
            OnStopped?.Invoke(this);
            audioSource.Stop();
        }

        internal void Finish()
        {
            if (!audioSource.loop) return;
            audioSource.loop = false;
            float remainingTime = audioSource.clip.length - audioSource.time;
            App.Delay(this, remainingTime, OnCompletedInvoke);
        }

        internal void FadePlayMusic(AudioClip audioClip, bool isLooping, float volume, bool isMusicFadeVolume,
            float durationOut,
            float durationIn)
        {
            if (isMusicFadeVolume && volume != 0)
            {
                if (audioSource.isPlaying)
                {
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
#if UNITY_EDITOR
        private void Reset()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }
#endif
    }
}