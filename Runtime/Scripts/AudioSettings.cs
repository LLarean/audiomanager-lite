using UnityEngine;
using System;
using System.Collections.Generic;

namespace AudioManagerLite
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Audio Manager Lite/Audio Settings")]
    public class AudioSettings : ScriptableObject
    {
        [System.Serializable]
        public class CategorySettings
        {
            public AudioCategory category;
            [Range(0f, 1f)]
            public float volume = 1f;
            public bool muted = false;
            public int maxSimultaneousSounds = 10;
        }

        [Header("Category Settings")]
        public List<CategorySettings> categorySettings = new List<CategorySettings>();

        [Header("Pool Settings")]
        public int initialPoolSize = 10;
        public int maxPoolSize = 50;

        [Header("Default Fade Settings")]
        public float defaultFadeInTime = 0.5f;
        public float defaultFadeOutTime = 0.5f;

        private void OnValidate()
        {
            // Ensure we have all categories
            var categories = System.Enum.GetValues(typeof(AudioCategory));
            foreach (AudioCategory category in categories)
            {
                if (!categorySettings.Exists(cs => cs.category == category))
                {
                    categorySettings.Add(new CategorySettings { category = category });
                }
            }
        }

        public CategorySettings GetCategorySettings(AudioCategory category)
        {
            return categorySettings.Find(cs => cs.category == category) ?? new CategorySettings { category = category };
        }
    }
}