using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace AudioManagerLite
{
    public class AudioPlayer : MonoBehaviour
    {
        private static AudioPlayer _instance;
        public static AudioPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Audio Manager");
                    _instance = go.AddComponent<AudioPlayer>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [SerializeField] private AudioSettings settings;
        
        private Dictionary<AudioCategory, List<AudioSource>> activeSources = new Dictionary<AudioCategory, List<AudioSource>>();
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private Dictionary<AudioCategory, float> categoryVolumes = new Dictionary<AudioCategory, float>();
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeManager();
        }

        private void InitializeManager()
        {
            if (settings == null)
            {
                settings = Resources.Load<AudioSettings>("DefaultAudioSettings");
                if (settings == null)
                {
                    Debug.LogWarning("No AudioSettings found. Creating default settings.");
                    settings = CreateDefaultSettings();
                }
            }

            // Initialize category volumes
            foreach (var categorySettings in settings.categorySettings)
            {
                categoryVolumes[categorySettings.category] = categorySettings.volume;
                activeSources[categorySettings.category] = new List<AudioSource>();
            }

            // Pre-populate pool
            for (int i = 0; i < settings.initialPoolSize; i++)
            {
                CreateAudioSource();
            }

            LoadVolumeSettings();
        }

        private AudioSettings CreateDefaultSettings()
        {
            var defaultSettings = ScriptableObject.CreateInstance<AudioSettings>();
            return defaultSettings;
        }

        private AudioSource CreateAudioSource()
        {
            var go = new GameObject("AudioSource");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSourcePool.Enqueue(source);
            return source;
        }

        private AudioSource GetAudioSource()
        {
            if (audioSourcePool.Count == 0)
            {
                if (transform.childCount >= settings.maxPoolSize)
                {
                    // Recycle oldest inactive source
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        var source = transform.GetChild(i).GetComponent<AudioSource>();
                        if (!source.isPlaying)
                        {
                            return source;
                        }
                    }
                    return null; // All sources are busy
                }
                CreateAudioSource();
            }
            
            return audioSourcePool.Dequeue();
        }

        private void ReturnAudioSource(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.loop = false;
                source.volume = 1f;
                source.pitch = 1f;
                audioSourcePool.Enqueue(source);
            }
        }

        // Public API
        public static AudioSource Play(AudioClip clip, AudioCategory category = AudioCategory.SFX, bool loop = false, float volume = 1f)
        {
            return Instance.PlayInternal(clip, category, loop, volume);
        }

        public static AudioSource Play(string clipName, AudioCategory category = AudioCategory.SFX, bool loop = false, float volume = 1f)
        {
            var clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found in Resources folder.");
                return null;
            }
            return Play(clip, category, loop, volume);
        }

        public static AudioSource PlayAtPosition(AudioClip clip, Vector3 position, AudioCategory category = AudioCategory.SFX, float volume = 1f)
        {
            var source = Play(clip, category, false, volume);
            if (source != null)
            {
                source.transform.position = position;
                source.spatialBlend = 1f; // 3D sound
            }
            return source;
        }

        public static AudioSource PlayWithFadeIn(AudioClip clip, AudioCategory category = AudioCategory.SFX, float fadeTime = -1f)
        {
            if (fadeTime < 0) fadeTime = Instance.settings.defaultFadeInTime;
            var source = Play(clip, category, false, 0f);
            if (source != null)
            {
                Instance.StartCoroutine(Instance.FadeIn(source, fadeTime, GetCategoryVolume(category)));
            }
            return source;
        }

        public static void FadeOut(AudioSource source, float fadeTime = -1f)
        {
            if (source != null && Instance != null)
            {
                if (fadeTime < 0) fadeTime = Instance.settings.defaultFadeOutTime;
                Instance.StartCoroutine(Instance.FadeOutCoroutine(source, fadeTime));
            }
        }

        public static void SetCategoryVolume(AudioCategory category, float volume)
        {
            Instance.SetCategoryVolumeInternal(category, volume);
        }

        public static float GetCategoryVolume(AudioCategory category)
        {
            return Instance.GetCategoryVolumeInternal(category);
        }

        public static void SetCategoryMute(AudioCategory category, bool muted)
        {
            Instance.SetCategoryMuteInternal(category, muted);
        }

        public static void StopAll()
        {
            Instance.StopAllInternal();
        }

        public static void StopCategory(AudioCategory category)
        {
            Instance.StopCategoryInternal(category);
        }

        // Internal implementations
        private AudioSource PlayInternal(AudioClip clip, AudioCategory category, bool loop, float volume)
        {
            if (clip == null) return null;

            var source = GetAudioSource();
            if (source == null) return null;

            source.clip = clip;
            source.loop = loop;
            source.volume = volume * GetCategoryVolumeInternal(category);
            source.spatialBlend = 0f; // 2D sound by default

            activeSources[category].Add(source);
            source.Play();

            if (!loop)
            {
                StartCoroutine(ReturnSourceWhenDone(source, category));
            }

            return source;
        }

        private IEnumerator ReturnSourceWhenDone(AudioSource source, AudioCategory category)
        {
            yield return new WaitWhile(() => source.isPlaying);
            
            activeSources[category].Remove(source);
            ReturnAudioSource(source);
        }

        private IEnumerator FadeIn(AudioSource source, float fadeTime, float targetVolume)
        {
            float currentTime = 0f;
            float startVolume = 0f;

            while (currentTime < fadeTime)
            {
                currentTime += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeTime);
                yield return null;
            }
        }

        private IEnumerator FadeOutCoroutine(AudioSource source, float fadeTime)
        {
            float startVolume = source.volume;
            float currentTime = 0f;

            while (currentTime < fadeTime)
            {
                currentTime += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
                yield return null;
            }

            source.Stop();
        }

        private void SetCategoryVolumeInternal(AudioCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            categoryVolumes[category] = volume;

            // Update all active sources in this category
            foreach (var source in activeSources[category])
            {
                if (source != null)
                {
                    // Maintain relative volume while applying category volume
                    var relativeVolume = source.volume / GetCategoryVolumeInternal(category);
                    source.volume = relativeVolume * volume;
                }
            }

            SaveVolumeSettings();
        }

        private float GetCategoryVolumeInternal(AudioCategory category)
        {
            return categoryVolumes.ContainsKey(category) ? categoryVolumes[category] : 1f;
        }

        private void SetCategoryMuteInternal(AudioCategory category, bool muted)
        {
            var categorySettings = settings.GetCategorySettings(category);
            categorySettings.muted = muted;

            float volume = muted ? 0f : categorySettings.volume;
            SetCategoryVolumeInternal(category, volume);
        }

        private void StopAllInternal()
        {
            foreach (var categoryList in activeSources.Values)
            {
                foreach (var source in categoryList)
                {
                    if (source != null) source.Stop();
                }
                categoryList.Clear();
            }
        }

        private void StopCategoryInternal(AudioCategory category)
        {
            if (activeSources.ContainsKey(category))
            {
                foreach (var source in activeSources[category])
                {
                    if (source != null) source.Stop();
                }
                activeSources[category].Clear();
            }
        }

        private void SaveVolumeSettings()
        {
            foreach (var kvp in categoryVolumes)
            {
                PlayerPrefs.SetFloat($"AudioManager_{kvp.Key}_Volume", kvp.Value);
            }
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            foreach (var category in categoryVolumes.Keys)
            {
                string key = $"AudioManager_{category}_Volume";
                if (PlayerPrefs.HasKey(key))
                {
                    categoryVolumes[category] = PlayerPrefs.GetFloat(key);
                }
            }
        }
    }
}