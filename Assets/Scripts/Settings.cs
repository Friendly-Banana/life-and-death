using UnityEngine;
using UnityEngine.UI;
namespace LoD
{
    public class Settings : MonoBehaviour
    {
        public const string VOLUME_KEY = "volume";
        public Slider volume;
        [Range(0, 1)]
        public float muteBelow = 0.01f;

        // Start is called before the first frame update
        void Start()
        {
            AudioListener.pause = PlayerPrefs.GetFloat(VOLUME_KEY, 1) < muteBelow;
            AudioListener.volume = volume.value = PlayerPrefs.GetFloat(VOLUME_KEY, 0.5f);
        }

        public void SetVolume(float value)
        {
            AudioListener.volume = value;
            AudioListener.pause = value < muteBelow;
            PlayerPrefs.SetFloat(VOLUME_KEY, value);
        }
    }
}