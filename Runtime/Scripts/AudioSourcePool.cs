using UnityEngine;
using System.Collections.Generic;

namespace AudioManagerLite
{
    /// <summary>
    /// Manages pooling of AudioSource components for performance optimization
    /// </summary>
    public class AudioSourcePool
    {
        private readonly Queue<AudioSource> _availableSources = new Queue<AudioSource>();
        private readonly Transform _parent;
        private readonly int _maxPoolSize;
        
        public int ActiveSourcesCount => _parent.childCount - _availableSources.Count;
        public int AvailableSourcesCount => _availableSources.Count;

        public AudioSourcePool(Transform parent, int initialSize, int maxSize)
        {
            _parent = parent;
            _maxPoolSize = maxSize;
            
            PrewarmPool(initialSize);
        }

        public AudioSource GetSource()
        {
            if (_availableSources.Count == 0)
            {
                if (_parent.childCount >= _maxPoolSize)
                {
                    return RecycleOldestInactiveSource();
                }
                CreateNewSource();
            }
            
            return _availableSources.Dequeue();
        }

        public void ReturnSource(AudioSource source)
        {
            if (source == null) return;
            
            ResetSource(source);
            _availableSources.Enqueue(source);
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewSource();
            }
        }

        private AudioSource CreateNewSource()
        {
            var sourceObject = new GameObject("PooledAudioSource");
            sourceObject.transform.SetParent(_parent);
            
            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            _availableSources.Enqueue(source);
            return source;
        }

        private AudioSource RecycleOldestInactiveSource()
        {
            for (int i = 0; i < _parent.childCount; i++)
            {
                var source = _parent.GetChild(i).GetComponent<AudioSource>();
                if (source != null && !source.isPlaying)
                {
                    return source;
                }
            }
            return null; // All sources are busy
        }

        private void ResetSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.transform.localPosition = Vector3.zero;
        }
    }
}