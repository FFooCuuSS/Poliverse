using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Scrollbar scrollbar;
    public float duration = 20f;

    private float currentTime;

    void Start()
    {
        // �ڱ� �ڽ��� Scrollbar ������Ʈ ��������
        scrollbar = GetComponent<Scrollbar>();

        currentTime = duration;
        scrollbar.size = 1f; // ���۰�
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
