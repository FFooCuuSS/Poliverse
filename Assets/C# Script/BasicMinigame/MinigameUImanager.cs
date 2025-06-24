using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MinigameUImanager : MonoBehaviour
{
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private Slider timerSlider;

    private int selectedPlanet = 1;
    private float loadingTime = 2f;
    private float timerDuration;
    private float timerElapsed;
    private bool isTimerActive;

    private MiniGameBase currentMinigame;
    private Queue<string> minigameQueue = new Queue<string>();

    private int life = 10;

    void Start()
    {
        guideText.gameObject.SetActive(false);
        timerSlider.value = 0f;

        switch (selectedPlanet)
        {
            case 1:
                SetMinigameQueue("PolicePlanet", 10);
                break;
            case 2:
                SetMinigameQueue("CandyPlanet", 15);
                break;
            case 3:
                SetMinigameQueue("MafiaPlanet", 15);
                break;
            case 4:
                SetMinigameQueue("MusicPlanet", 15);
                break;
        }

        StartCoroutine(LoadNextMinigameRoutine());
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
                StartCoroutine(TimerEndFailDelay());
            }
        }
    }

    private IEnumerator TimerEndFailDelay()
    {
        yield return new WaitForSeconds(1f);
        OnMinigameFail();
    }

    private void SetMinigameQueue(string planetName, int minigameCount)
    {
        List<int> minigameIndexes = new List<int>();

        for (int i = 1; i < minigameCount; i++)
        {
            string path = $"MinigamePrefab/{planetName}/{selectedPlanet}-{i}minigame";

            if (Resources.Load<GameObject>(path) != null)
            {
                minigameIndexes.Add(i);
            }
            else
            {
                Debug.LogWarning($"미니게임이 존재하지 않음: {path} -> 건너뜀");
            }
        }

        for (int i = 0; i < minigameIndexes.Count; i++)
        {
            int randIndex = Random.Range(i, minigameIndexes.Count);
            (minigameIndexes[i], minigameIndexes[randIndex]) = (minigameIndexes[randIndex], minigameIndexes[i]);
        }

        foreach (int index in minigameIndexes)
        {
            string path = $"MinigamePrefab/{planetName}/{selectedPlanet}-{index}minigame";
            minigameQueue.Enqueue(path);
        }

        string bossPath = $"MinigamePrefab/{planetName}/{selectedPlanet}-{minigameCount}minigame";

        if (Resources.Load<GameObject>(bossPath) != null)
        {
            minigameQueue.Enqueue(bossPath);
        }
        else
        {
            Debug.LogWarning($"보스 미니게임이 존재하지 않음: {bossPath} -> 큐에 추가 안됨");
        }
    }

    private IEnumerator LoadNextMinigameRoutine()
    {
        if (minigameQueue.Count == 0)
        {
            Debug.Log("모든 미니게임 종료");
            yield break;
        }

        yield return new WaitForSeconds(loadingTime);

        string minigamePath = minigameQueue.Dequeue();
        GameObject minigameObj = Instantiate(Resources.Load<GameObject>(minigamePath));
        currentMinigame = minigameObj.GetComponent<MiniGameBase>();

        ShowGuide(currentMinigame.GetMinigameExplain, 2f);

        yield return new WaitForSeconds(0.5f);

        StartMinigame();
    }

    private void StartMinigame()
    {
        currentMinigame.OnSuccess += OnMinigameSuccess;
        currentMinigame.OnFail += OnMinigameFail;

        timerDuration = currentMinigame.GetTimerDuration;
        StartTimer(timerDuration);

        currentMinigame.StartGame();
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

    private void OnMinigameSuccess()
    {
        isTimerActive = false; // 타이머 강제 종료
        EndMinigame();
    }

    private void OnMinigameFail()
    {
        isTimerActive = false; // 타이머 강제 종료
        life--;
        EndMinigame();
    }

    private void EndMinigame()
    {
        isTimerActive = false;

        if (currentMinigame != null)
        {
            currentMinigame.OnSuccess -= OnMinigameSuccess;
            currentMinigame.OnFail -= OnMinigameFail;
            Destroy(currentMinigame.gameObject);
        }

        StartCoroutine(WaitAndLoadNext());
    }

    private IEnumerator WaitAndLoadNext()
    {
        yield return new WaitForSeconds(2f);

        if (life > 0)
        {
            StartCoroutine(LoadNextMinigameRoutine());
        }
        else
        {
            Debug.Log("게임 오버");
        }
    }
}
