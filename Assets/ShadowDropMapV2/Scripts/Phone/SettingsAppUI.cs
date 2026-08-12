using UnityEngine;
using UnityEngine.UI;

// 설정 앱. 우선 볼륨 조절만 구현해둠 - 필요한 설정 항목은 이 스크립트에 추가하면 됨
public class SettingsAppUI : MonoBehaviour
{
    public Slider volumeSlider;

    void OnEnable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }
}