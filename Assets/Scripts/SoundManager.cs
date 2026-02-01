// SoundManager.cs
using UnityEngine;
using System.Collections.Generic;

namespace Game.Audio
{
    [DisallowMultipleComponent]
    public class SoundManager : MonoBehaviour, ISoundManager
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource _audioSourcePrefab;
        [SerializeField] private SoundData _soundData;

        private Queue<AudioSource> _audioSourcePool;
        private const int InitialPoolSize = 5;

        private static SoundManager _instance;
        public static SoundManager Instance => _instance;

        private void Awake()
        {
            // Enforce single instance
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            //DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            if (_soundData == null)
            {
                Debug.LogError("SoundData is not assigned in SoundManager!");
            }

            _audioSourcePool = new Queue<AudioSource>(InitialPoolSize);
            for (int i = 0; i < InitialPoolSize; i++)
            {
                AddAudioSourceToPool();
            }
        }

        // 🔊 Play SFX (one-shot)
        public void PlaySound(string soundName)
        {
            if (!TryGetSound(soundName, out AudioClip clip, out float volume, out float pitch))
                return;

            AudioSource source = GetAudioSource();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = false;

            source.Play();
            StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
        }

        // 🔊 Play looped sound (engine, wind, etc.)
        public AudioSource PlaySound(string soundName, bool loop)
        {
            if (!TryGetSound(soundName, out AudioClip clip, out float volume, out float pitch))
                return null;

            AudioSource source = GetAudioSource();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;

            source.Play();

            if (!loop)
                StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));

            return source;
        }

        // ⛔ Stop a looped sound
        public void StopSound(UnityEngine.AudioSource source)
        {
            if (source == null) return;

            source.Stop();
            source.clip = null;
            source.loop = false;

            if (!_audioSourcePool.Contains(source))
                _audioSourcePool.Enqueue(source);
        }

        // 🎵 Sound lookup helper
        private bool TryGetSound(string soundName, out AudioClip clip, out float volume, out float pitch)
        {
            // Assign default values first
            clip = null;
            volume = 1f;
            pitch = 1f;

            if (_soundData == null || !_soundData.TryGetSound(soundName, out AudioClip dataClip, out float dataVolume, out float dataPitch))
            {
                Debug.LogWarning($"No sound found for '{soundName}'");
                return false;
            }

            // Assign values from data
            clip = dataClip;
            volume = dataVolume;
            pitch = dataPitch;
            return true;
        }

        // 🎚 Pool handling
        private AudioSource GetAudioSource()
        {
            if (_audioSourcePool.Count == 0)
                AddAudioSourceToPool();

            return _audioSourcePool.Dequeue();
        }

        private AudioSource AddAudioSourceToPool()
        {
            AudioSource source = Instantiate(_audioSourcePrefab, transform);
            source.playOnAwake = false;
            _audioSourcePool.Enqueue(source);
            return source;
        }

        private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            source.Stop();
            source.clip = null;
            _audioSourcePool.Enqueue(source);
        }
    }
}