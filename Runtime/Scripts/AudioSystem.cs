using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace AudioManagerLite
{
    /// <summary>
    /// Main audio system that orchestrates all audio playback functionality
    /// </summary>
    public class AudioSystem : MonoBehaviour
    {
        private static AudioSystem _instance;
        public static AudioSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var audioSystemObject = new GameObject("[AudioSystem]");
                    _instance = audioSystemObject.AddComponent<AudioSystem>();
                    DontDestroyOnLoad(audioSystemObject);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private AudioSettings _settings;

        private AudioSourcePool _sourcePool;
        private AudioVolumeController _volumeController;
        private AudioEffectHandler _effectHandler;
        private readonly Dictionary<AudioCategory, List<AudioSource>> _activeSources = new Dictionary<AudioCategory, List<AudioSource>>();

        public AudioSettings Settings => _settings;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystem();
        }

        #endregion

        #region Initialization

        private void InitializeSystem()
        {
            LoadSettings();
            InitializeComponents();
            InitializeCategories();
        }

        private void LoadSettings()
        {
            if (_settings == null)
            {
                _settings = Resources.Load<AudioSettings>("DefaultAudioSettings");
                if (_settings == null)
                {
                    Debug.LogWarning("[AudioSystem] No AudioSettings found. Creating default settings.");
                    _settings = CreateDefaultSettings();
                }
            }
        }

        private void InitializeComponents()
        {
            _sourcePool = new AudioSourcePool(transform, _settings.maxPoolSize);
            _sourcePool.Prewarm(_settings.initialPoolSize);
            
            _volumeController = new AudioVolumeController(_settings);
            _effectHandler = gameObject.AddComponent<AudioEffectHandler>();
        }

        private void InitializeCategories()
        {
            foreach (var categorySettings in _settings.categorySettings)
            {
                _activeSources[categorySettings.category] = new List<AudioSource>();
            }
        }

        private AudioSettings CreateDefaultSettings()
        {
            return ScriptableObject.CreateInstance<AudioSettings>();
        }

        #endregion

        #region Public Static API

        public static AudioSource Play(AudioClip clip, AudioCategory category = AudioCategory.SFX, bool loop = false, float volume = 1f)
        {
            return Instance.PlayInternal(clip, category, loop, volume);
        }

        public static AudioSource Play(string clipName, AudioCategory category = AudioCategory.SFX, bool loop = false, float volume = 1f)
        {
            var clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioSystem] Audio clip '{clipName}' not found in Resources folder.");
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
            if (fadeTime < 0) 
                fadeTime = Instance._settings.defaultFadeInTime;
            
            var source = Play(clip, category, false, 0f);
            if (source != null)
            {
                float targetVolume = Instance._volumeController.GetCategoryVolume(category);
                Instance._effectHandler.FadeIn(source, fadeTime, targetVolume);
            }
            return source;
        }

        public static void FadeOut(AudioSource source, float fadeTime = -1f)
        {
            if (source == null || Instance == null) return;
            
            if (fadeTime < 0) 
                fadeTime = Instance._settings.defaultFadeOutTime;
                
            Instance._effectHandler.FadeOut(source, fadeTime);
        }

        public static void SetCategoryVolume(AudioCategory category, float volume)
        {
            Instance._volumeController.SetCategoryVolume(category, volume);
            Instance.UpdateActiveSources(category);
        }

        public static float GetCategoryVolume(AudioCategory category)
        {
            return Instance._volumeController.GetCategoryVolume(category);
        }

        public static void SetCategoryMute(AudioCategory category, bool muted)
        {
            Instance._volumeController.SetCategoryMute(category, muted);
            Instance.UpdateActiveSources(category);
        }

        public static bool IsCategoryMuted(AudioCategory category)
        {
            return Instance._volumeController.IsCategoryMuted(category);
        }

        public static void StopAll()
        {
            Instance.StopAllInternal();
        }

        public static void StopCategory(AudioCategory category)
        {
            Instance.StopCategoryInternal(category);
        }

        public static void PauseCategory(AudioCategory category)
        {
            Instance.PauseCategoryInternal(category);
        }

        public static void ResumeCategory(AudioCategory category)
        {
            Instance.ResumeCategoryInternal(category);
        }

        #endregion

        #region Internal Implementation

        private AudioSource PlayInternal(AudioClip clip, AudioCategory category, bool loop, float volume)
        {
            if (clip == null) return null;

            var source = _sourcePool.Get();
            if (source == null) 
            {
                Debug.LogWarning("[AudioSystem] No available audio sources. Consider increasing pool size.");
                return null;
            }

            ConfigureAudioSource(source, clip, loop, volume, category);
            
            _activeSources[category].Add(source);
            source.Play();

            if (!loop)
            {
                StartCoroutine(ReturnSourceWhenFinished(source, category));
            }

            return source;
        }

        private void ConfigureAudioSource(AudioSource source, AudioClip clip, bool loop, float volume, AudioCategory category)
        {
            source.clip = clip;
            source.loop = loop;
            source.volume = volume * _volumeController.GetCategoryVolume(category);
            source.spatialBlend = 0f; // 2D by default
        }

        private IEnumerator ReturnSourceWhenFinished(AudioSource source, AudioCategory category)
        {
            yield return new WaitWhile(() => source != null && source.isPlaying);
            
            if (source != null)
            {
                _activeSources[category].Remove(source);
                _sourcePool.Return(source);
            }
        }

        private void UpdateActiveSources(AudioCategory category)
        {
            if (_activeSources.ContainsKey(category))
            {
                _volumeController.ApplyVolumeToSources(category, _activeSources[category]);
            }
        }

        private void StopAllInternal()
        {
            foreach (var categoryList in _activeSources.Values)
            {
                StopSourcesInList(categoryList);
            }
        }

        private void StopCategoryInternal(AudioCategory category)
        {
            if (_activeSources.ContainsKey(category))
            {
                StopSourcesInList(_activeSources[category]);
            }
        }

        private void PauseCategoryInternal(AudioCategory category)
        {
            if (_activeSources.ContainsKey(category))
            {
                foreach (var source in _activeSources[category])
                {
                    if (source != null && source.isPlaying)
                    {
                        source.Pause();
                    }
                }
            }
        }

        private void ResumeCategoryInternal(AudioCategory category)
        {
            if (_activeSources.ContainsKey(category))
            {
                foreach (var source in _activeSources[category])
                {
                    if (source != null)
                    {
                        source.UnPause();
                    }
                }
            }
        }

        private void StopSourcesInList(List<AudioSource> sources)
        {
            foreach (var source in sources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
            sources.Clear();
        }

        #endregion

        // #region Debug Info
        //
        // [System.Diagnostics.Conditional("UNITY_EDITOR")]
        // public void LogDebugInfo()
        // {
        //     Debug.Log($"[AudioSystem] Active sources: {_sourcePool.ActiveSourcesCount}, Available: {_sourcePool.AvailableSourcesCount}");
        //     
        //     foreach (var kvp in _activeSources)
        //     {
        //         Debug.Log($"[AudioSystem] {kvp.Key}: {kvp.Value.Count} active sources");
        //     }
        // }
        //
        // #endregion
    }
}