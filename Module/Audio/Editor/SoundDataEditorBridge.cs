using System;
using VirtueSky.Audio;

namespace VirtueSky.AudioEditor
{
    /// <summary>
    /// Bridge để Audio.Editor assembly giao tiếp với ControlPanel.Editor assembly
    /// mà không tạo circular dependency. CPAudioDrawer sẽ inject các delegate vào đây.
    /// </summary>
    public static class SoundDataEditorBridge
    {
        /// <summary>Inject bởi CPAudioDrawer: trả về true nếu chế độ auto-preview đang bật.</summary>
        public static Func<bool> IsAutoPreviewEnabled;

        /// <summary>Inject bởi CPAudioDrawer: cập nhật SoundData vừa được play gần nhất.</summary>
        public static Action<SoundData> SetLastPlayedSoundData;

        /// <summary>Inject bởi CPAudioDrawer: lấy SoundData vừa được play gần nhất.</summary>
        public static Func<SoundData> GetLastPlayedSoundData;
    }
}