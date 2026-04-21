using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.DataStorage;
using VirtueSky.Inspector;
using VirtueSky.ObjectPooling;

namespace VirtueSky.Audio
{
    [EditorIcon("icon_sound_mixer"), HideMonoScript]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private bool isDontDestroyOnLoad;
        [Space, SerializeField] private Transform audioHolder;
        [SerializeField] private SoundComponent soundComponentPrefab;
        private SoundComponent music;

        [ReadOnly, SerializeField] private List<SoundComponent> listCacheSfx = new List<SoundComponent>();

        private int key = 0;
        private const string KEY_MUSIC_VOLUME = "KEY_MUSIC_VOLUME";
        private const string KEY_SFX_VOLUME = "KEY_SFX_VOLUME";

        private static event Func<SoundData, SoundCache> OnPlaySfxEvent;
        private static event Action<SoundCache> OnStopSfxEvent;
        private static event Action<SoundCache> OnPauseSfxEvent;
        private static event Action<SoundCache> OnResumeSfxEvent;
        private static event Action<SoundCache> OnFinishSfxEvent;
        private static event Action OnStopAllSfxEvent;

        private static event Action<SoundData> OnPlayMusicEvent;
        private static event Action OnStopMusicEvent;
        private static event Action OnPauseMusicEvent;
        private static event Action OnResumeMusicEvent;

        public static event Action<float> OnVolumeSfxChangedEvent;
        public static event Action<float> OnVolumeMusicChangedEvent;

        private void Awake()
        {
            if (isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }

        private void OnEnable()
        {
            OnPlaySfxEvent += InternalPlaySfx;
            OnStopSfxEvent += InternalStopSfx;
            OnPauseSfxEvent += InternalPauseSfx;
            OnResumeSfxEvent += InternalResumeSfx;
            OnFinishSfxEvent += InternalFinishSfx;
            OnStopAllSfxEvent += InternalStopAllSfx;

            OnPlayMusicEvent += InternalPlayMusic;
            OnStopMusicEvent += InternalStopMusic;
            OnPauseMusicEvent += InternalPauseMusic;
            OnResumeMusicEvent += InternalResumeMusic;

            OnVolumeSfxChangedEvent += OnSfxVolumeChanged;
            OnVolumeMusicChangedEvent += OnMusicVolumeChanged;
        }

        private void OnDisable()
        {
            OnPlaySfxEvent -= InternalPlaySfx;
            OnStopSfxEvent -= InternalStopSfx;
            OnPauseSfxEvent -= InternalPauseSfx;
            OnResumeSfxEvent -= InternalResumeSfx;
            OnFinishSfxEvent -= InternalFinishSfx;
            OnStopAllSfxEvent -= InternalStopAllSfx;

            OnPlayMusicEvent -= InternalPlayMusic;
            OnStopMusicEvent -= InternalStopMusic;
            OnPauseMusicEvent -= InternalPauseMusic;
            OnResumeMusicEvent -= InternalResumeMusic;

            OnVolumeSfxChangedEvent -= OnSfxVolumeChanged;
            OnVolumeMusicChangedEvent -= OnMusicVolumeChanged;
        }


        #region Sfx Method

        private SoundCache InternalPlaySfx(SoundData soundData)
        {
            SoundComponent sfxComponent = soundComponentPrefab.Spawn(audioHolder);
            sfxComponent.PlayAudioClip(soundData.GetAudioClip(), soundData.loop, soundData.volume * SfxVolume);
            if (!soundData.loop) sfxComponent.OnCompleted += OnFinishPlayingAudio;
            SoundCache soundCache = GetSoundCache(soundData);
            sfxComponent.Key = key;
            listCacheSfx.Add(sfxComponent);
            return soundCache;
        }


        private void InternalStopSfx(SoundCache soundCache)
        {
            var soundComponent = GetSoundComponent(soundCache);
            if (soundComponent == null) return;
            StopAndCleanAudioComponent(soundComponent);
        }


        private void InternalPauseSfx(SoundCache soundCache)
        {
            var soundComponent = GetSoundComponent(soundCache);
            if (soundComponent == null || !soundComponent.IsPlaying) return;
            soundComponent.Pause();
        }

        private void InternalResumeSfx(SoundCache soundCache)
        {
            var soundComponent = GetSoundComponent(soundCache);
            if (soundComponent == null || soundComponent.IsPlaying) return;
            soundComponent.Resume();
        }


        private void InternalFinishSfx(SoundCache soundCache)
        {
            var soundComponent = GetSoundComponent(soundCache);
            if (soundComponent == null || !soundComponent.IsPlaying) return;
            soundComponent.Finish();
            soundComponent.OnCompleted += OnFinishPlayingAudio;
        }

        private void InternalStopAllSfx()
        {
            var listTemp = listCacheSfx.ToList();
            for (int i = 0; i < listTemp.Count; i++)
            {
                StopAndCleanAudioComponent(listTemp[i]);
            }

            listCacheSfx.Clear();
            listTemp.Clear();
            key = 0;
        }

        #endregion

        #region Music Method

        private void InternalPlayMusic(SoundData soundData)
        {
            if (music == null || !music.IsPlaying)
            {
                music = soundComponentPrefab.Spawn(audioHolder);
            }

            music.FadePlayMusic(soundData.GetAudioClip(), soundData.loop, soundData.volume * MusicVolume,
                soundData.isMusicFadeVolume, soundData.fadeOutDuration, soundData.fadeInDuration);
            music.OnCompleted += StopAudioMusic;
        }


        private void InternalStopMusic()
        {
            if (music != null && music.IsPlaying)
            {
                music.Stop();
                music.gameObject.DeSpawn();
            }
        }

        private void InternalPauseMusic()
        {
            if (music != null && music.IsPlaying)
            {
                music.Pause();
            }
        }

        private void InternalResumeMusic()
        {
            if (music != null && !music.IsPlaying)
            {
                music.Resume();
            }
        }

        #endregion

        #region Handle

        private void OnMusicVolumeChanged(float volume)
        {
            if (music != null)
            {
                music.Volume = volume;
            }
        }


        private void OnSfxVolumeChanged(float volume)
        {
            for (var i = 0; i < listCacheSfx.Count; i++)
            {
                listCacheSfx[i].Volume = volume;
            }
        }

        void OnFinishPlayingAudio(SoundComponent soundComponent)
        {
            StopAndCleanAudioComponent(soundComponent);
        }

        void StopAndCleanAudioComponent(SoundComponent soundComponent)
        {
            if (!soundComponent.IsLooping)
            {
                soundComponent.OnCompleted -= OnFinishPlayingAudio;
            }

            soundComponent.Stop();
            soundComponent.gameObject.DeSpawn();
        }

        void StopAudioMusic(SoundComponent soundComponent)
        {
            soundComponent.OnCompleted -= StopAudioMusic;
            soundComponent.gameObject.DeSpawn();
        }

        SoundComponent GetSoundComponent(SoundCache soundCache)
        {
            if (soundCache == null) return null;
            for (var i = 0; i < listCacheSfx.Count; i++)
            {
                if (soundCache.key == listCacheSfx[i].Key) return listCacheSfx[i];
            }

            return null;
        }

        SoundCache GetSoundCache(SoundData soundData)
        {
            key++;
            return new SoundCache(key, soundData);
        }

        #endregion


        #region Public APi

        public static SoundCache PlaySfx(SoundData soundData) => OnPlaySfxEvent?.Invoke(soundData);
        public static void StopSfx(SoundCache soundCache) => OnStopSfxEvent?.Invoke(soundCache);
        public static void PauseSfx(SoundCache soundCache) => OnPauseSfxEvent?.Invoke(soundCache);
        public static void ResumeSfx(SoundCache soundCache) => OnResumeSfxEvent?.Invoke(soundCache);
        public static void FinishSfx(SoundCache soundCache) => OnFinishSfxEvent?.Invoke(soundCache);
        public static void StopAllSfx() => OnStopAllSfxEvent?.Invoke();
        public static void PlayMusic(SoundData soundData) => OnPlayMusicEvent?.Invoke(soundData);
        public static void StopMusic() => OnStopMusicEvent?.Invoke();
        public static void PauseMusic() => OnPauseMusicEvent?.Invoke();
        public static void ResumeMusic() => OnResumeMusicEvent?.Invoke();

        public static float SfxVolume
        {
            get => GameData.Get(KEY_SFX_VOLUME, 1);
            set
            {
                GameData.Set(KEY_SFX_VOLUME, value);
                OnVolumeSfxChangedEvent?.Invoke(value);
            }
        }

        public static float MusicVolume
        {
            get => GameData.Get(KEY_MUSIC_VOLUME, 1);
            set
            {
                GameData.Set(KEY_MUSIC_VOLUME, value);
                OnVolumeMusicChangedEvent?.Invoke(value);
            }
        }

        #endregion
    }
}