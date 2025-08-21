using UnityEngine;
using UnityEngine.UI;

namespace AudioManagerLite
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        [Header("Sound Settings")]
        public AudioClip soundClip;
        public AudioCategory category = AudioCategory.UI;
        [Range(0f, 1f)]
        public float volume = 1f;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            if (soundClip != null)
            {
                AudioSystem.Play(soundClip, category, false, volume);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlaySound);
            }
        }
    }
}