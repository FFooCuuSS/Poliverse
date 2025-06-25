using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MinigameUImanager : MonoBehaviour
{
    // 외부 참조
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private GameObject blockInputPanel; // 입력 막는 패널
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject lifeManager;

    // 관리용 변수
    private int selectedPlanet = 1;
    private float loadingTime = 2f;
    private float timerDuration;
    private float timerElapsed;
    private bool isTimerActive;
    private int currentStage = 1;
    private int bossStageIndex = 0;
    private string bossMinigamePath = "";
    private Coroutine failCoroutine;

    // 자주 사용되는 지역 변수
    private MiniGameBase currentMinigame;
    private LifeNumber lifeNumber;
    private Queue<string> minigameQueue = new Queue<string>();

    private int life = 4;

    void Start()
    {
        UpdateStageText();
        timerSlider.gameObject.SetActive(false);
        timerSlider.value = 0f;
        guideText.gameObject.SetActive(false);
        blockInputPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        timerSlider.value = 0f;
        lifeNumber = lifeManager.GetComponent<LifeNumber>();

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

                if (failCoroutine == null)
                    failCoroutine = StartCoroutine(TimerEndFailDelay());
            }
        }
    }

    private IEnumerator TimerEndFailDelay()
    {
        yield return new WaitForSeconds(1f);
        currentMinigame.Fail();

        failCoroutine = null;
    }

    // 미니게임 행성별 세팅
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
            bossStageIndex = minigameIndexes.Count + 1;
            bossMinigamePath = bossPath;
        }
        else
        {
            Debug.LogWarning($"보스 미니게임이 존재하지 않음: {bossPath} -> 큐에 추가 안됨");
        }
    }

    // 미니게임 시작 루틴
    private IEnumerator LoadNextMinigameRoutine()
    {
        if (minigameQueue.Count == 0)
        {
            Debug.Log("모든 미니게임 종료");
            gameplayPanel.SetActive(false);
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("LobbyScene");

            yield break;
        }

        yield return new WaitForSeconds(loadingTime);

        timerSlider.gameObject.SetActive(true);
        gameplayPanel.SetActive(true);
        string minigamePath = minigameQueue.Dequeue();
        GameObject minigameObj = Instantiate(Resources.Load<GameObject>(minigamePath));
        currentMinigame = minigameObj.GetComponent<MiniGameBase>();

        ShowGuide(currentMinigame.GetMinigameExplain, 2f);

        yield return new WaitForSeconds(0.5f);

        StartMinigame();
    }

    // 이벤트 및 제한시간 초기화
    private void StartMinigame()
    {
        currentMinigame.OnSuccess += OnMinigameSuccess;
        currentMinigame.OnFail += OnMinigameFail;

        timerDuration = currentMinigame.GetTimerDuration;
        StartTimer(timerDuration);

        currentMinigame.StartGame();
    }

    // 게임설명
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

    // 제한시간
    public void StartTimer(float duration)
    {
        timerDuration = duration;
        timerElapsed = 0f;
        isTimerActive = true;
        timerSlider.value = 1f;
    }

    // 게임 종료 이후 판정 및 다음 회차 지속
    private void OnMinigameSuccess()
    {
        isTimerActive = false;

        // 실패 코루틴 강제 종료
        if (failCoroutine != null)
        {
            StopCoroutine(failCoroutine);
            failCoroutine = null;
        }

        currentStage++;
        if (currentStage >= bossStageIndex) currentStage = bossStageIndex;
        StartCoroutine(DelayAndEndMinigame());
    }

    private void OnMinigameFail()
    {
        isTimerActive = false;
        life--;
        lifeNumber.LoseLife();

        if (currentStage == bossStageIndex)
        {
            if (life <= 0)
            {
                Invoke("GameOver", 1f);
                return;
            }

            Debug.Log("보스 재도전");

            StartCoroutine(RetryCurrentMinigame());
        }
        else
        {
            currentStage++;

            StartCoroutine(DelayAndEndMinigame());
        }
    }

    void GameOver()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    private IEnumerator DelayAndEndMinigame()
    {
        UpdateStageText();
        blockInputPanel.SetActive(true); // 입력 막기
        yield return new WaitForSeconds(1f);

        if (currentMinigame != null)
        {
            currentMinigame.OnSuccess -= OnMinigameSuccess;
            currentMinigame.OnFail -= OnMinigameFail;
            Destroy(currentMinigame.gameObject);
        }

        gameplayPanel.SetActive(false);
        blockInputPanel.SetActive(false); // 입력 해제
        timerSlider.gameObject.SetActive(false);
        StartCoroutine(WaitAndLoadNext());
    }

    private IEnumerator RetryCurrentMinigame()
    {
        blockInputPanel.SetActive(true); // 입력 막기
        yield return new WaitForSeconds(1f);

        if (currentMinigame != null)
        {
            currentMinigame.OnSuccess -= OnMinigameSuccess;
            currentMinigame.OnFail -= OnMinigameFail;
            Destroy(currentMinigame.gameObject);
        }

        gameplayPanel.SetActive(false);
        blockInputPanel.SetActive(false); // 입력 해제
        timerSlider.gameObject.SetActive(false);

        yield return new WaitForSeconds(loadingTime);

        // 여기서 바로 미니게임 재로드
        timerSlider.gameObject.SetActive(true);
        gameplayPanel.SetActive(true);

        GameObject minigameObj = Instantiate(Resources.Load<GameObject>(bossMinigamePath));
        currentMinigame = minigameObj.GetComponent<MiniGameBase>();

        ShowGuide(currentMinigame.GetMinigameExplain, 2f);

        yield return new WaitForSeconds(0.5f);

        StartMinigame();
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

    // 추가 UI 관리
    private void UpdateStageText()
    {
        stageText.text = $"{currentStage}";
    }
}
