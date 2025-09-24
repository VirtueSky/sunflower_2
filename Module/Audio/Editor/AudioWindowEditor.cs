using UnityEditor;
using VirtueSky.Audio;
using VirtueSky.UtilsEditor;

namespace VirtueSky.AudioEditor
{
    public class AudioWindowEditor : EditorWindow
    {
        // [MenuItem("Unity-Common/Audio/Sound Data", false)]
        public static void CreateSoundData()
        {
            CreateAsset.CreateScriptableAssetsOnlyName<SoundData>(
                FileExtension.DefaultRootPath + "/Audio/SoundData", "sound_data");
        }
    }
}