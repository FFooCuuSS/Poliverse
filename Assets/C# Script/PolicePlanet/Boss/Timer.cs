using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Scrollbar scrollbar;
    public float duration = 20f;

    private float currentTime;

    void Start()
    {
        // 자기 자신의 Scrollbar 컴포넌트 가져오기
        scrollbar = GetComponent<Scrollbar>();

        currentTime = duration;
        scrollbar.size = 1f; // 시작값
    }

    void Update()
    {
        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / duration);
            scrollbar.size = progress;
        }
    }
}
