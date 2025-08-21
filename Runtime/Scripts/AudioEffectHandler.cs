using UnityEngine;
using System.Collections;

namespace AudioManagerLite
{
    /// <summary>
    /// Handles audio effects like fading in/out
    /// </summary>
    public class AudioEffectHandler : MonoBehaviour
    {
        public void FadeIn(AudioSource source, float duration, float targetVolume)
        {
            if (source != null)
            {
                StartCoroutine(FadeInCoroutine(source, duration, targetVolume));
            }
        }

        public void FadeOut(AudioSource source, float duration)
        {
            if (source != null)
            {
                StartCoroutine(FadeOutCoroutine(source, duration));
            }
        }

        public void CrossFade(AudioSource sourceOut, AudioSource sourceIn, float duration)
        {
            if (sourceOut != null) FadeOut(sourceOut, duration);
            if (sourceIn != null) FadeIn(sourceIn, duration, sourceIn.volume);
        }

        private IEnumerator FadeInCoroutine(AudioSource source, float duration, float targetVolume)
        {
            float currentTime = 0f;
            float startVolume = source.volume;

            while (currentTime < duration && source != null)
            {
                currentTime += Time.unscaledDeltaTime;
                float progress = currentTime / duration;
                source.volume = Mathf.Lerp(startVolume, targetVolume, progress);
                yield return null;
            }

            if (source != null)
            {
                source.volume = targetVolume;
            }
        }

        private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float currentTime = 0f;

            while (currentTime < duration && source != null && source.isPlaying)
            {
                currentTime += Time.unscaledDeltaTime;
                float progress = currentTime / duration;
                source.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }

            if (source != null)
            {
                source.Stop();
                source.volume = startVolume; // Restore original volume
            }
        }
    }
}