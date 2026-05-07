using UnityEngine;

namespace Hidenet.Audio
{
    public enum SoundType
    {
        BGM,
        Ambient,
        SFX,
        Voice
    }

    [CreateAssetMenu(fileName = "SoundData", menuName = "Hidenet/Audio/Sound Data", order = 0)]
    public class SoundDataSO : ScriptableObject
    {
        [Header("Clip")]
        public AudioClip clip;
        public SoundType type = SoundType.SFX;

        [Header("Playback")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;

        [Header("Variation")]
        [Range(0f, 0.5f)] public float volumeVariation = 0f;
        [Range(0f, 0.5f)] public float pitchVariation = 0f;

        public float GetVolume()
        {
            return Mathf.Clamp01(volume + Random.Range(-volumeVariation, volumeVariation));
        }

        public float GetPitch()
        {
            return Mathf.Clamp(pitch + Random.Range(-pitchVariation, pitchVariation), 0.1f, 3f);
        }
    }
}
