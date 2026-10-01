using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    void OnEnable()
    {
        // 창 열 때: 저장된 값을 슬라이더에 표시
        masterSlider.SetValueWithoutNotify(GameRoot.Instance.Settings.Data.masterVolume);
        bgmSlider.SetValueWithoutNotify(GameRoot.Instance.Settings.Data.bgmVolume);
        sfxSlider.SetValueWithoutNotify(GameRoot.Instance.Settings.Data.sfxVolume);
        uiSlider.SetValueWithoutNotify(GameRoot.Instance.Settings.Data.uiVolume);
    }
    public void OnMasterChanged(float value)
    {
        GameRoot.Instance.Settings.SetMasterVolume(value);
    }
    public void OnBgmChanged(float value)
    {
        // 슬라이더 움직일 때: 볼륨 변경
        GameRoot.Instance.Settings.SetBgmVolume(value);
    }
    public void OnSfxChanged(float value)
    {
        GameRoot.Instance.Settings.SetSfxVolume(value);
    }
    public void OnUiChanged(float value)
    {
        GameRoot.Instance.Settings.SetUiVolume(value);
    }

    void OnDisable()
    {
        // 창 닫을 때: 파일에 저장
        GameRoot.Instance.Settings.SaveNow();
    }
}