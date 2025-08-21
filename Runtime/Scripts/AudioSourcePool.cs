using UnityEngine;
using System.Collections.Generic;

namespace AudioManagerLite
{
    /// <summary>
    /// Manages pooling of AudioSource components for performance optimization.
    /// </summary>
    public sealed class AudioSourcePool
    {
        private readonly Queue<AudioSource> _availableSources = new Queue<AudioSource>();
        private readonly Transform _parentTransform;
        private readonly int _maxPoolSize;

        private int TotalCount { get; set; }

        public AudioSourcePool(Transform parentTransform, int maxSize)
        {
            _parentTransform = parentTransform;
            _maxPoolSize = maxSize;
        }

        /// <summary>
        /// Gets an available AudioSource from the pool.
        /// </summary>
        public AudioSource Get()
        {
            if (_availableSources.Count > 0)
            {
                return _availableSources.Dequeue();
            }

            if (TotalCount < _maxPoolSize)
            {
                return CreateNewSource();
            }

            Debug.LogWarning($"[AudioSourcePool] All {_maxPoolSize} sources are in use");
            return null;
        }

        /// <summary>
        /// Returns an AudioSource to the pool for reuse.
        /// </summary>
        public void Return(AudioSource source)
        {
            if (source == null) return;

            ResetSource(source);
            _availableSources.Enqueue(source);
        }

        /// <summary>
        /// Creates the initial pool of audio sources.
        /// </summary>
        public void Prewarm(int count)
        {
            count = Mathf.Min(count, _maxPoolSize - TotalCount);
            for (int i = 0; i < count; i++)
            {
                CreateNewSource();
            }
        }

        /// <summary>
        /// Clears all audio sources from the pool.
        /// </summary>
        public void Clear()
        {
            while (_availableSources.Count > 0)
            {
                var source = _availableSources.Dequeue();
                if (source != null && source.gameObject != null)
                {
                    Object.Destroy(source.gameObject);
                }
            }
            TotalCount = 0;
        }

        private AudioSource CreateNewSource()
        {
            var go = new GameObject($"AudioSource_{TotalCount + 1:00}");
            go.transform.SetParent(_parentTransform, false);
            
            var source = go.AddComponent<AudioSource>();
            ConfigureSource(source);
            
            _availableSources.Enqueue(source);
            TotalCount++;
            
            return source;
        }

        private static void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
        }

        private static void ResetSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.time = 0f;
        }
    }
}