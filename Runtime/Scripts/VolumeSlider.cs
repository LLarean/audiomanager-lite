using UnityEngine;
using UnityEngine.UI;

namespace AudioManagerLite
{
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        [Header("Volume Settings")]
        public AudioCategory targetCategory = AudioCategory.Master;

        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }

        private void Start()
        {
            // Set initial value
            slider.value = AudioSystem.GetCategoryVolume(targetCategory);
            
            // Subscribe to changes
            slider.onValueChanged.AddListener(OnVolumeChanged);
        }

        private void OnVolumeChanged(float value)
        {
            AudioSystem.SetCategoryVolume(targetCategory, value);
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnVolumeChanged);
            }
        }
    }
}