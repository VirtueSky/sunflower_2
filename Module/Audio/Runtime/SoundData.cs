using System;
using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Inspector;
using VirtueSky.Misc;
using Random = UnityEngine.Random;

namespace VirtueSky.Audio
{
    [CreateAssetMenu(menuName = "Sunflower2/Audio/Sound Data", fileName = "sound_data")]
    [EditorIcon("scriptable_audioclip")]
    public class SoundData : ScriptableObject
    {
        public enum GetType
        {
            Random,
            Sequence
        }
        
        [GUIColor(0.8f, 1.0f, 0.6f), Space(10), SerializeField, TextArea(2, 5)]
        private string description;
        [Space] public bool loop = false;
        [Range(0f, 1f)] public float volume = 1;

        public SoundType soundType;

        [Space, Header("Fade Volume - Only Music"), Tooltip("Only Music Background")] [ShowIf(nameof(soundType), SoundType.Music)]
        public bool isMusicFadeVolume = false;

        [ShowIf(nameof(ConditionFadeMusic), true)]
        public float fadeInDuration = .5f;

        [ShowIf(nameof(ConditionFadeMusic), true)]
        public float fadeOutDuration = .5f;
        
        [Space] [SerializeField] private GetType getType = GetType.Random;
        [Space, SerializeField] private List<AudioClip> audioClips;
        public bool ConditionFadeMusic => isMusicFadeVolume && soundType == SoundType.Music;
        private int sequenceIndex = 0;
        public int NumberOfAudioClips => audioClips.Count;
        public List<AudioClip> AudioClips() => audioClips;
        public AudioClip GetAudioClip()
        {
            if (audioClips.Count > 0)
            {
                switch (getType)
                {
                    case GetType.Random:
                        return audioClips[Random.Range(0, audioClips.Count)];
                    case GetType.Sequence:
                        var clip = audioClips[sequenceIndex];
                        if (sequenceIndex < audioClips.Count - 1)
                        {
                            sequenceIndex++;
                        }
                        else
                        {
                            sequenceIndex = 0;
                        }

                        return clip;
                }
            }

            return null;
        }
        public void AddAudioClip(AudioClip audioClip)
        {
            audioClips.Add(audioClip);
        }

        public void AddAudioClips(List<AudioClip> clips)
        {
            audioClips.Adds(clips);
        }

        public void AddAudioClips(AudioClip[] clips)
        {
            audioClips.Adds(clips);
        }

        public void ClearAudioClips()
        {
            if (audioClips.IsNullOrEmpty()) return;
            audioClips.Clear();
        }
    }

    public enum SoundType
    {
        Sfx,
        Music
    }
}