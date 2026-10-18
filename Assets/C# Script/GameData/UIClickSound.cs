using UnityEngine;
using UnityEngine.UI;

public class UIClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickClip;

    void Start()
    {
        // true: 꺼져 있는 버튼(설정창 안 버튼 등)도 포함
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(PlayClick);
        }
    }

    void PlayClick()
    {
        if (GameRoot.Instance == null) return;
        GameRoot.Instance.Audio.PlayUiSound(clickClip);
    }
}