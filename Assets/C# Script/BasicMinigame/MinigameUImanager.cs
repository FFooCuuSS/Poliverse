using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MinigameUImanager : MonoBehaviour
{
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TMP_Text resultText;

    private float timerDuration;
    private float timerElapsed;
    private bool isTimerActive;

    void Start()
    {
        guideText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        timerSlider.value = 1f;
    }

    void Update()
    {
        if (isTimerActive)
        {
            timerElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(1f - (timerElapsed / timerDuration));
            timerSlider.value = t;

            if (timerElapsed >= timerDuration)
            {
                isTimerActive = false;
                // 타이머는 시각적으로만 끝남 (판정은 미니게임 쪽에서)
            }
        }
    }

    public void ShowGuide(string text, float duration)
    {
        StartCoroutine(ShowGuideCoroutine(text, duration));
    }

    private IEnumerator ShowGuideCoroutine(string text, float duration)
    {
        guideText.text = text;
        guideText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        guideText.gameObject.SetActive(false);
    }

    public void StartTimer(float duration)
    {
        timerDuration = duration;
        timerElapsed = 0f;
        isTimerActive = true;
        timerSlider.value = 1f;
    }

    public void ShowResult(string result)
    {
        resultText.text = result;
        resultText.color = result == "성공!" ? Color.green : Color.red;
        resultText.gameObject.SetActive(true);
    }

    public void HideResult()
    {
        resultText.gameObject.SetActive(false);
    }
}