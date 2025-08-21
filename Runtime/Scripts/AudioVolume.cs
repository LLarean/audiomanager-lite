using UnityEngine;
using System.Collections.Generic;

namespace AudioManagerLite
{
    /// <summary>
    /// Handles volume control and persistence for different audio categories.
    /// Provides centralized management of audio volume settings with PlayerPrefs persistence.
    /// </summary>
    public class AudioVolume
    {
        private readonly Dictionary<AudioCategory, float> _categoryVolumes = new Dictionary<AudioCategory, float>();
        private readonly Dictionary<AudioCategory, bool> _categoryMuteStates = new Dictionary<AudioCategory, bool>();
        private readonly AudioSettings _settings;

        /// <summary>
        /// Initializes a new instance of the AudioVolumeController class.
        /// </summary>
        /// <param name="settings">Audio settings configuration.</param>
        public AudioVolume(AudioSettings settings)
        {
            _settings = settings;
            InitializeVolumes();
            LoadVolumeSettings();
        }

        /// <summary>
        /// Gets the effective volume for the specified audio category (considers mute state).
        /// </summary>
        /// <param name="category">The audio category to get volume for.</param>
        /// <returns>Volume level between 0.0 and 1.0.</returns>
        public float GetCategoryVolume(AudioCategory category)
        {
            if (_categoryMuteStates.TryGetValue(category, out bool isMuted) && isMuted)
            {
                return 0f;
            }

            return _categoryVolumes.TryGetValue(category, out float volume) ? volume : 1f;
        }

        /// <summary>
        /// Sets the volume for the specified audio category.
        /// </summary>
        /// <param name="category">The audio category to set volume for.</param>
        /// <param name="volume">Volume level between 0.0 and 1.0.</param>
        public void SetCategoryVolume(AudioCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            _categoryVolumes[category] = volume;
            SaveVolumeSettings();
        }

        /// <summary>
        /// Checks if the specified audio category is muted.
        /// </summary>
        /// <param name="category">The audio category to check.</param>
        /// <returns>True if the category is muted; otherwise, false.</returns>
        public bool IsCategoryMuted(AudioCategory category)
        {
            return _categoryMuteStates.TryGetValue(category, out bool isMuted) && isMuted;
        }

        /// <summary>
        /// Sets the mute state for the specified audio category.
        /// </summary>
        /// <param name="category">The audio category to set mute state for.</param>
        /// <param name="muted">True to mute the category; false to unmute.</param>
        public void SetCategoryMute(AudioCategory category, bool muted)
        {
            _categoryMuteStates[category] = muted;
            SaveVolumeSettings();
        }

        /// <summary>
        /// Applies the current volume settings to a list of audio sources for the specified category.
        /// Preserves the relative volume levels of individual sources.
        /// </summary>
        /// <param name="category">The audio category of the sources.</param>
        /// <param name="sources">List of audio sources to update.</param>
        public void ApplyVolumeToSources(AudioCategory category, List<AudioSource> sources)
        {
            if (sources == null) return;

            float categoryVolume = GetCategoryVolume(category);
            float storedVolume = GetStoredCategoryVolume(category);

            foreach (AudioSource source in sources)
            {
                if (source == null) continue;

                float relativeVolume = storedVolume > 0f ? source.volume / storedVolume : source.volume;
                source.volume = relativeVolume * categoryVolume;
            }
        }

        private float GetStoredCategoryVolume(AudioCategory category)
        {
            return _categoryVolumes.TryGetValue(category, out float volume) ? volume : 1f;
        }

        private void InitializeVolumes()
        {
            if (_settings?.categorySettings == null) return;

            foreach (AudioSettings.CategorySettings categorySettings in _settings.categorySettings)
            {
                _categoryVolumes[categorySettings.category] = categorySettings.volume;
                _categoryMuteStates[categorySettings.category] = categorySettings.muted;
            }
        }

        private void SaveVolumeSettings()
        {
            foreach (KeyValuePair<AudioCategory, float> volumePair in _categoryVolumes)
            {
                PlayerPrefs.SetFloat("AudioManager_Volume_" + volumePair.Key, volumePair.Value);
            }

            foreach (KeyValuePair<AudioCategory, bool> mutePair in _categoryMuteStates)
            {
                PlayerPrefs.SetInt("AudioManager_Mute_" + mutePair.Key, mutePair.Value ? 1 : 0);
            }

            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            List<AudioCategory> categories = new List<AudioCategory>(_categoryVolumes.Keys);

            foreach (AudioCategory category in categories)
            {
                string volumeKey = "AudioManager_Volume_" + category;
                if (PlayerPrefs.HasKey(volumeKey))
                {
                    _categoryVolumes[category] = PlayerPrefs.GetFloat(volumeKey);
                }

                string muteKey = "AudioManager_Mute_" + category;
                if (PlayerPrefs.HasKey(muteKey))
                {
                    _categoryMuteStates[category] = PlayerPrefs.GetInt(muteKey) == 1;
                }
            }
        }
    }
}