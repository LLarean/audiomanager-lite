using UnityEngine;
using System.Collections.Generic;

namespace AudioManagerLite
{
    /// <summary>
    /// Handles volume control and persistence for different audio categories
    /// </summary>
    public class AudioVolumeController
    {
        private readonly Dictionary<AudioCategory, float> _categoryVolumes = new Dictionary<AudioCategory, float>();
        private readonly Dictionary<AudioCategory, bool> _categoryMuteStates = new Dictionary<AudioCategory, bool>();
        private readonly AudioSettings _settings;

        private const string VOLUME_PREFS_PREFIX = "AudioManager_Volume_";
        private const string MUTE_PREFS_PREFIX = "AudioManager_Mute_";

        public AudioVolumeController(AudioSettings settings)
        {
            _settings = settings;
            InitializeVolumes();
            LoadVolumeSettings();
        }

        public float GetCategoryVolume(AudioCategory category)
        {
            if (_categoryMuteStates.ContainsKey(category) && _categoryMuteStates[category])
                return 0f;
                
            return _categoryVolumes.ContainsKey(category) ? _categoryVolumes[category] : 1f;
        }

        public void SetCategoryVolume(AudioCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            _categoryVolumes[category] = volume;
            SaveVolumeSettings();
        }

        public bool IsCategoryMuted(AudioCategory category)
        {
            return _categoryMuteStates.ContainsKey(category) && _categoryMuteStates[category];
        }

        public void SetCategoryMute(AudioCategory category, bool muted)
        {
            _categoryMuteStates[category] = muted;
            SaveVolumeSettings();
        }

        public void ApplyVolumeToSources(AudioCategory category, List<AudioSource> sources)
        {
            float categoryVolume = GetCategoryVolume(category);
            
            foreach (var source in sources)
            {
                if (source != null)
                {
                    // Preserve the relative volume of each source
                    float relativeVolume = source.volume / GetStoredCategoryVolume(category);
                    source.volume = relativeVolume * categoryVolume;
                }
            }
        }

        private float GetStoredCategoryVolume(AudioCategory category)
        {
            return _categoryVolumes.ContainsKey(category) ? _categoryVolumes[category] : 1f;
        }

        private void InitializeVolumes()
        {
            foreach (var categorySettings in _settings.categorySettings)
            {
                _categoryVolumes[categorySettings.category] = categorySettings.volume;
                _categoryMuteStates[categorySettings.category] = categorySettings.muted;
            }
        }

        private void SaveVolumeSettings()
        {
            foreach (var kvp in _categoryVolumes)
            {
                PlayerPrefs.SetFloat(VOLUME_PREFS_PREFIX + kvp.Key, kvp.Value);
            }
            
            foreach (var kvp in _categoryMuteStates)
            {
                PlayerPrefs.SetInt(MUTE_PREFS_PREFIX + kvp.Key, kvp.Value ? 1 : 0);
            }
            
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            var categories = new List<AudioCategory>(_categoryVolumes.Keys);
            
            foreach (var category in categories)
            {
                string volumeKey = VOLUME_PREFS_PREFIX + category;
                if (PlayerPrefs.HasKey(volumeKey))
                {
                    _categoryVolumes[category] = PlayerPrefs.GetFloat(volumeKey);
                }

                string muteKey = MUTE_PREFS_PREFIX + category;
                if (PlayerPrefs.HasKey(muteKey))
                {
                    _categoryMuteStates[category] = PlayerPrefs.GetInt(muteKey) == 1;
                }
            }
        }
    }
}