using System.Collections;
using Hidenet.Audio;
using Hidenet.Core;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Hidenet.Audio
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        [Header("Mixer")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;

        [Header("Pool Size")]
        [SerializeField] private int ambientPoolSize = 4;
        [SerializeField] private int sfxPoolSize = 16;
        [SerializeField] private int voicePoolSize = 4;

        [Header("Mixer Volume Parameters")]
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string bgmVolumeParam = "BGMVolume";
        [SerializeField] private string ambientVolumeParam = "AmbientVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";
        [SerializeField] private string voiceVolumeParam = "VoiceVolume";

        private AudioSource bgmSource;
        private AudioSource[] ambientPool;
        private AudioSource[] sfxPool;
        private AudioSource[] voicePool;
        private int ambientIndex;
        private int sfxIndex;
        private int voiceIndex;

        protected override void Awake()
        {
            base.Awake();
            InitSources();
        }

        private void InitSources()
        {
            bgmSource = CreateSource("BGMSource", bgmGroup, loop: true);

            ambientPool = new AudioSource[ambientPoolSize];
            for (int i = 0; i < ambientPoolSize; i++)
                ambientPool[i] = CreateSource($"AmbientSource_{i}", ambientGroup, loop: true);

            sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
                sfxPool[i] = CreateSource($"SFXSource_{i}", sfxGroup, loop: false);

            voicePool = new AudioSource[voicePoolSize];
            for (int i = 0; i < voicePoolSize; i++)
                voicePool[i] = CreateSource($"VoiceSource_{i}", voiceGroup, loop: false);
        }

        private AudioSource CreateSource(string name, AudioMixerGroup group, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = group;
            src.playOnAwake = false;
            src.loop = loop;
            return src;
        }

        // ----------------------------------------------------------------------
        // BGM
        // ----------------------------------------------------------------------

        public void PlayBGM(SoundDataSO data)
        {
            if (data == null || data.clip == null) return;

            bgmSource.clip = data.clip;
            bgmSource.volume = data.GetVolume();
            bgmSource.pitch = data.GetPitch();
            bgmSource.loop = data.loop;
            bgmSource.Play();
        }

        public void PlayBGM(string address)
        {
            StartCoroutine(LoadAndPlay<SoundDataSO>(address, data => PlayBGM(data)));
        }

        public void StopBGM() => bgmSource.Stop();
        public void PauseBGM() => bgmSource.Pause();
        public void ResumeBGM() => bgmSource.UnPause();

        // ----------------------------------------------------------------------
        // Ambient
        // ----------------------------------------------------------------------

        public AudioSource PlayAmbient(SoundDataSO data)
        {
            if (data == null || data.clip == null) return null;

            var src = ambientPool[ambientIndex];
            ambientIndex = (ambientIndex + 1) % ambientPoolSize;

            src.Stop();
            src.clip = data.clip;
            src.volume = data.GetVolume();
            src.pitch = data.GetPitch();
            src.loop = data.loop;
            src.Play();
            return src;
        }

        public void PlayAmbient(string address)
        {
            StartCoroutine(LoadAndPlay<SoundDataSO>(address, data => PlayAmbient(data)));
        }

        public void StopAllAmbient()
        {
            foreach (var src in ambientPool)
                if (src.isPlaying) src.Stop();
        }

        // ----------------------------------------------------------------------
        // SFX
        // ----------------------------------------------------------------------

        public void PlaySFX(SoundDataSO data)
        {
            if (data == null || data.clip == null) return;

            var src = sfxPool[sfxIndex];
            sfxIndex = (sfxIndex + 1) % sfxPoolSize;

            src.clip = data.clip;
            src.volume = data.GetVolume();
            src.pitch = data.GetPitch();
            src.Play();
        }

        public void PlaySFX(string address)
        {
            StartCoroutine(LoadAndPlay<SoundDataSO>(address, data => PlaySFX(data)));
        }

        public void PlaySFXAtPoint(SoundDataSO data, Vector3 position)
        {
            if (data == null || data.clip == null) return;
            AudioSource.PlayClipAtPoint(data.clip, position, data.GetVolume());
        }

        // ----------------------------------------------------------------------
        // Voice
        // ----------------------------------------------------------------------

        public void PlayVoice(SoundDataSO data)
        {
            if (data == null || data.clip == null) return;

            var src = voicePool[voiceIndex];
            voiceIndex = (voiceIndex + 1) % voicePoolSize;

            src.clip = data.clip;
            src.volume = data.GetVolume();
            src.pitch = data.GetPitch();
            src.Play();
        }

        public void PlayVoice(string address)
        {
            StartCoroutine(LoadAndPlay<SoundDataSO>(address, data => PlayVoice(data)));
        }

        public void StopAllVoice()
        {
            foreach (var src in voicePool)
                if (src.isPlaying) src.Stop();
        }

        // ----------------------------------------------------------------------
        // Volume Control (linear 0~1 -> dB)
        // ----------------------------------------------------------------------

        public void SetMasterVolume(float linear) => SetMixerVolume(masterVolumeParam, linear);
        public void SetBGMVolume(float linear) => SetMixerVolume(bgmVolumeParam, linear);
        public void SetAmbientVolume(float linear) => SetMixerVolume(ambientVolumeParam, linear);
        public void SetSFXVolume(float linear) => SetMixerVolume(sfxVolumeParam, linear);
        public void SetVoiceVolume(float linear) => SetMixerVolume(voiceVolumeParam, linear);

        private void SetMixerVolume(string parameter, float linear)
        {
            if (mixer == null || string.IsNullOrEmpty(parameter)) return;
            float dB = linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
            mixer.SetFloat(parameter, dB);
        }

        // ----------------------------------------------------------------------
        // Addressables Loading
        // ----------------------------------------------------------------------

        private IEnumerator LoadAndPlay<T>(string address, System.Action<T> onLoaded) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded(handle.Result);
            }
            else
            {
                Debug.LogWarning($"[SoundManager] Failed to load: {address}");
            }
        }
    }
}
