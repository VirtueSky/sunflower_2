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
        private int musicSpawnVersion;

        [ReadOnly, SerializeField] private List<SoundComponent> listCacheSfx = new List<SoundComponent>();

        private int key = 0;
        private const string KEY_MUSIC_VOLUME = "KEY_MUSIC_VOLUME";
        private const string KEY_SFX_VOLUME = "KEY_SFX_VOLUME";

        private static event Func<SoundData, int, SoundCache> OnPlaySfxEvent;
        private static event Action<SoundCache> OnStopSfxEvent;
        private static event Action<SoundCache> OnPauseSfxEvent;
        private static event Action<SoundCache> OnResumeSfxEvent;
        private static event Action<SoundCache> OnFinishSfxEvent;
        private static event Action OnStopAllSfxEvent;

        private static event Action<SoundData, int> OnPlayMusicEvent;
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

        private SoundCache InternalPlaySfx(SoundData soundData, int indexClip = -1)
        {
            var clip = soundData.GetAudioClip(indexClip);
            if (clip == null)
            {
                // Do not spawn: a component without a clip never completes and would stay active forever.
                Debug.LogError($"[AudioManager] SoundData '{soundData.name}' has no AudioClip");
                return null;
            }

            SoundComponent sfxComponent = soundComponentPrefab.Spawn(audioHolder);
            sfxComponent.PlayAudioClip(clip, soundData.loop, soundData.volume * SfxVolume);
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
            // IsPaused, not IsPlaying: IsPlaying is also false under AudioListener.pause (ads).
            if (soundComponent == null || soundComponent.IsPaused) return;
            soundComponent.Pause();
        }

        private void InternalResumeSfx(SoundCache soundCache)
        {
            var soundComponent = GetSoundComponent(soundCache);
            if (soundComponent == null || !soundComponent.IsPaused) return;
            soundComponent.Resume();
        }


        private void InternalFinishSfx(SoundCache soundCache)
        {
            var soundComponent = GetSoundComponent(soundCache);
            // Finish only makes sense for looping SFX; non-looping ones already have the handler.
            if (soundComponent == null || !soundComponent.IsLooping) return;
            soundComponent.OnCompleted -= OnFinishPlayingAudio; // avoid double subscription
            soundComponent.OnCompleted += OnFinishPlayingAudio;
            soundComponent.Finish();
        }

        private void InternalStopAllSfx()
        {
            var listTemp = listCacheSfx.ToList();
            for (int i = 0; i < listTemp.Count; i++)
            {
                StopAndCleanAudioComponent(listTemp[i]);
            }

            listCacheSfx.Clear();
            // Do NOT reset key: keys must increase monotonically so old SoundCache handles
            // can never match a newly played SFX.
        }

        #endregion

        #region Music Method

        /// <summary>
        /// True while <see cref="music"/> still refers to the component we spawned for music.
        /// Deliberately not based on IsPlaying: that is also false when the music is paused (Pause() or
        /// AudioListener.pause during ads), and treating paused music as "no music" leaks it.
        /// The spawn version catches the component having been despawned elsewhere (e.g. Pool.DeSpawnAll).
        /// </summary>
        private bool HasMusic => music != null && music.SpawnVersion == musicSpawnVersion;

        private void InternalPlayMusic(SoundData soundData, int indexClip = -1)
        {
            var clip = soundData.GetAudioClip(indexClip);
            if (clip == null)
            {
                Debug.LogError($"[AudioManager] SoundData '{soundData.name}' has no AudioClip");
                return;
            }

            if (!HasMusic)
            {
                music = soundComponentPrefab.Spawn(audioHolder);
                musicSpawnVersion = music.SpawnVersion;
            }

            music.FadePlayMusic(clip, soundData.loop, soundData.volume * MusicVolume,
                soundData.isMusicFadeVolume, soundData.fadeOutDuration, soundData.fadeInDuration);
            music.OnCompleted -= StopAudioMusic; // same component may be reused across PlayMusic calls
            music.OnCompleted += StopAudioMusic;
        }


        private void InternalStopMusic()
        {
            if (HasMusic)
            {
                music.Stop();
                music.gameObject.DeSpawn(); // CleanUp() also clears OnCompleted
            }

            // Drop the reference: the pooled object may be reused as an SFX afterwards.
            music = null;
        }

        private void InternalPauseMusic()
        {
            if (HasMusic && !music.IsPaused)
            {
                music.Pause();
            }
        }

        private void InternalResumeMusic()
        {
            if (HasMusic && music.IsPaused)
            {
                music.Resume();
            }
        }

        #endregion

        #region Handle

        private void OnMusicVolumeChanged(float volume)
        {
            if (HasMusic)
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
            soundComponent.OnCompleted -= OnFinishPlayingAudio; // always unsubscribe, regardless of IsLooping
            soundComponent.Stop();
            soundComponent.gameObject.DeSpawn();
            if(listCacheSfx.Contains(soundComponent)) listCacheSfx.Remove(soundComponent);
        }

        void StopAudioMusic(SoundComponent soundComponent)
        {
            soundComponent.OnCompleted -= StopAudioMusic;
            soundComponent.gameObject.DeSpawn();
            if (music == soundComponent) music = null;
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

        public static SoundCache PlaySfx(SoundData soundData, int indexClip = -1) => OnPlaySfxEvent?.Invoke(soundData, indexClip);
        public static void StopSfx(SoundCache soundCache) => OnStopSfxEvent?.Invoke(soundCache);
        public static void PauseSfx(SoundCache soundCache) => OnPauseSfxEvent?.Invoke(soundCache);
        public static void ResumeSfx(SoundCache soundCache) => OnResumeSfxEvent?.Invoke(soundCache);
        public static void FinishSfx(SoundCache soundCache) => OnFinishSfxEvent?.Invoke(soundCache);
        public static void StopAllSfx() => OnStopAllSfxEvent?.Invoke();
        public static void PlayMusic(SoundData soundData, int indexClip = -1) => OnPlayMusicEvent?.Invoke(soundData, indexClip);
        public static void StopMusic() => OnStopMusicEvent?.Invoke();
        public static void PauseMusic() => OnPauseMusicEvent?.Invoke();
        public static void ResumeMusic() => OnResumeMusicEvent?.Invoke();

        public static float SfxVolume
        {
            get => GameData.Get(KEY_SFX_VOLUME, 1f);
            set
            {
                float v = Mathf.Clamp01(value);
                GameData.Set(KEY_SFX_VOLUME, v);
                OnVolumeSfxChangedEvent?.Invoke(v);
            }
        }

        public static float MusicVolume
        {
            get => GameData.Get(KEY_MUSIC_VOLUME, 1f);
            set
            {
                float v = Mathf.Clamp01(value);
                GameData.Set(KEY_MUSIC_VOLUME, v);
                OnVolumeMusicChangedEvent?.Invoke(v);
            }
        }

        #endregion
    }
}