namespace VirtueSky.Audio
{
    public static class AudioHelper
    {
        public static SoundCache PlaySfx(this SoundData soundData, int indexClip = -1) => AudioManager.PlaySfx(soundData, indexClip);
        public static void StopSfx(this SoundCache soundCache) => AudioManager.StopSfx(soundCache);
        public static void PauseSfx(this SoundCache soundCache) => AudioManager.PauseSfx(soundCache);
        public static void ResumeSfx(this SoundCache soundCache) => AudioManager.ResumeSfx(soundCache);
        public static void FinishSfx(this SoundCache soundCache) => AudioManager.FinishSfx(soundCache);
        public static void StopAllSfx() => AudioManager.StopAllSfx();

        public static void PlayMusic(this SoundData soundData, int indexClip = -1) => AudioManager.PlayMusic(soundData, indexClip);
        public static void StopMusic() => AudioManager.StopMusic();
        public static void PauseMusic() => AudioManager.PauseMusic();
        public static void ResumeMusic() => AudioManager.ResumeMusic();
    }
}