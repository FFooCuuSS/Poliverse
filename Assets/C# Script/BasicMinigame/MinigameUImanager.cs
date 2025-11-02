using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MinigameUIManager : MonoBehaviour
{
    // UI, 배치 외부참조
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private GameObject blockInputPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject lifeManager;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject standingArea;

    [Header("Standing Sprites")]
    [SerializeField] private Sprite playerStandingSprite;
    [SerializeField] private Sprite[] enemyStandingSprites;
    [SerializeField] private Sprite[] enemyVictorySprites;

    // 라운드 흐름 내부 변수
    private int selectedPlanet;
    private float loadingTime = 2f;
    private float timerDuration;
    private float timerElapsed;
    private bool isTimerActive;
    private int currentStage = 1;
    private int bossStageIndex = 0;
    private string bossMinigamePath = "";
    private Coroutine failCoroutine;

    // 성공, 실패 관련 연출
    private SpriteRenderer playerRenderer;
    private SpriteRenderer enemyRenderer;
    private Sprite originalEnemySprite;
    private Sprite originalPlayerSprite;
    private SuccessFailPanel successFailPanel;

    // 기타 미니게임 관리
    private MiniGameBase currentMinigame;
    private LifeNumber lifeNumber;
    private Queue<string> minigameQueue = new Queue<string>();

    private int life = 4;

    // 사운드 및 폴리싱
    [SerializeField] private AudioClip successBGM;
    [SerializeField] private AudioClip failureBGM;
    public AudioSource audioSource;

    void Awake()
    {
        selectedPlanet = CameraScrollController.selectedPlanetIndex + 1;
    }

    void Start()
    {
        UpdateStageText();

        resultPanel.SetActive(false);
        timerSlider.gameObject.SetActive(false);
        guideText.gameObject.SetActive(false);
        blockInputPanel.SetActive(false);
        timerSlider.value = 0f;

        successFailPanel = resultPanel.GetComponent<SuccessFailPanel>();
        lifeNumber = lifeManager.GetComponent<LifeNumber>();
        playerRenderer = standingArea.transform.GetChild(0).GetComponent<SpriteRenderer>();
        enemyRenderer = standingArea.transform.GetChild(1).GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerRenderer.sprite = playerStandingSprite;
        enemyRenderer.sprite = enemyStandingSprites[selectedPlanet - 1];
        PlayBounceAnimation(playerRenderer.transform);
        PlayBounceAnimation(enemyRenderer.transform);

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
            default:
                return;
        }

        StartCoroutine(LoadNextMinigameRoutine());
        audioSource.Play();
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

    // 미니게임 세팅
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
    }

    // 디음 미니게임 코루틴
    private IEnumerator LoadNextMinigameRoutine()
    {
        if (minigameQueue.Count == 0)
        {
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("LobbyScene");

            yield break;
        }

        yield return new WaitForSeconds(loadingTime);

        timerSlider.gameObject.SetActive(true);
        string minigamePath = minigameQueue.Dequeue();
        GameObject minigameObj = Instantiate(Resources.Load<GameObject>(minigamePath));
        currentMinigame = minigameObj.GetComponent<MiniGameBase>();

        ShowGuide(currentMinigame.GetMinigameExplain, 1f);

        yield return new WaitForSeconds(0.5f);

        StartMinigame();
    }

    // 미니게임 상태 및 시간 초기화
    private void StartMinigame()
    {
        currentMinigame.OnSuccess += OnMinigameSuccess;
        currentMinigame.OnFail += OnMinigameFail;

        timerDuration = currentMinigame.GetTimerDuration;
        StartTimer(timerDuration);

        currentMinigame.StartGame();
    }

    // 가이드 텍스트
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

    // 제한시간 관리
    public void StartTimer(float duration)
    {
        timerDuration = duration;
        timerElapsed = 0f;
        isTimerActive = true;
        timerSlider.value = 1f;
    }

    // 미니게임 성공/실패
    private void OnMinigameSuccess()
    {
        isTimerActive = false;

        if (failCoroutine != null)
        {
            StopCoroutine(failCoroutine);
            failCoroutine = null;
        }

        resultPanel.SetActive(true);
        successFailPanel.SuccessPanel();
        Invoke("PlayWinEffect",1f);

        currentStage++;
        if (currentStage >= bossStageIndex) currentStage = bossStageIndex;
        StartCoroutine(DelayAndEndMinigame());
    }

    private void OnMinigameFail()
    {
        isTimerActive = false;
        life--;
        lifeNumber.LoseLife();

        resultPanel.SetActive(true);
        successFailPanel.FailurePanel();
        Invoke("PlayLoseEffect", 1f);

        if (currentStage == bossStageIndex)
        {
            if (life <= 0)
            {
                Invoke("GameOver", 1f);
                return;
            }

            StartCoroutine(RetryCurrentMinigame());
        }
        else
        {
            currentStage++;

            StartCoroutine(DelayAndEndMinigame());
        }
    }

    // 승리 실패 연출
    private void PlayWinEffect()
    {
        if (originalPlayerSprite == null)
            originalPlayerSprite = playerRenderer.sprite;

        StartCoroutine(PlayStandingEffect(playerRenderer, originalPlayerSprite, originalPlayerSprite, -5f, 1.5f));
        StartCoroutine(PlayDefeatedEffect(enemyRenderer, -10f, 1.0f));

        audioSource.clip = successBGM;
    }
    private void PlayLoseEffect()
    {
        if (originalEnemySprite == null)
            originalEnemySprite = enemyRenderer.sprite;

        StartCoroutine(PlayStandingEffect(enemyRenderer, enemyVictorySprites[selectedPlanet - 1], originalEnemySprite, 5f, -1.5f));
        StartCoroutine(PlayDefeatedEffect(playerRenderer, 10f, -1.0f));

        audioSource.clip = failureBGM;
    }
    void GameOver()
    {
        CameraScrollController.selectedPlanetIndex = 0;
        SceneManager.LoadScene("LobbyScene");
    }

    private IEnumerator DelayAndEndMinigame()
    {
        UpdateStageText();
        blockInputPanel.SetActive(true);

        yield return new WaitForSeconds(1f);

        if (currentMinigame != null)
        {
            DOTween.Kill(currentMinigame.transform);

            currentMinigame.OnSuccess -= OnMinigameSuccess;
            currentMinigame.OnFail -= OnMinigameFail;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            Destroy(currentMinigame.gameObject);
        }

        audioSource.Play();
        blockInputPanel.SetActive(false);
        timerSlider.gameObject.SetActive(false);
        StartCoroutine(WaitAndLoadNext());
    }

    private IEnumerator RetryCurrentMinigame()
    {
        blockInputPanel.SetActive(true);
        yield return new WaitForSeconds(1f);

        if (currentMinigame != null)
        {
            currentMinigame.OnSuccess -= OnMinigameSuccess;
            currentMinigame.OnFail -= OnMinigameFail;
            Destroy(currentMinigame.gameObject);
        }

        blockInputPanel.SetActive(false);
        timerSlider.gameObject.SetActive(false);

        yield return new WaitForSeconds(loadingTime);

        timerSlider.gameObject.SetActive(true);

        GameObject minigameObj = Instantiate(Resources.Load<GameObject>(bossMinigamePath));
        currentMinigame = minigameObj.GetComponent<MiniGameBase>();

        ShowGuide(currentMinigame.GetMinigameExplain, 1f);

        yield return new WaitForSeconds(0.5f);

        StartMinigame();
    }

    private IEnumerator WaitAndLoadNext()
    {
        yield return new WaitForSeconds(2f);
        resultPanel.SetActive(false);


        if (life > 0)
        {
            StartCoroutine(LoadNextMinigameRoutine());
        }
        else
        {

        }
    }

    // UI 추가 관리
    private void UpdateStageText()
    {
        stageText.text = $"{currentStage}";
    }

    // idle 모션
    private void PlayBounceAnimation(Transform target)
    {
        Sequence seq = DOTween.Sequence();

        float scaleX = 1.01f; 
        float scaleY = 0.99f; 
        float moveY = -0.01f; 
        float duration = 0.15f; 

        Vector3 originalScale = target.localScale;
        Vector3 squashedScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
        Vector3 stretchedScale = new Vector3(originalScale.x * 0.95f, originalScale.y * 1.05f, originalScale.z);
        Vector3 originalPosition = target.localPosition;

        seq.Append(target.DOScale(squashedScale, duration).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalMoveY(originalPosition.y + moveY, duration).SetEase(Ease.OutQuad));

        seq.Append(target.DOScale(stretchedScale, duration).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalMoveY(originalPosition.y - moveY, duration).SetEase(Ease.OutQuad));

        seq.Append(target.DOScale(originalScale, duration).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalMoveY(originalPosition.y, duration).SetEase(Ease.OutQuad));

        seq.AppendInterval(0.15f);

        seq.SetLoops(-1); // 무한 반복
    }

    // 승리 모션
    private IEnumerator PlayStandingEffect(SpriteRenderer targetRenderer, Sprite victorySprite, Sprite originalSprite, float rotationAmount, float moveXAmount)
    {
        Vector3 originalPos = targetRenderer.transform.localPosition;
        Vector3 originalRotation = targetRenderer.transform.localEulerAngles;

        // 기존 점프
        targetRenderer.transform.DOLocalMoveY(originalPos.y + 0.15f, 0.15f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.15f);
        targetRenderer.transform.DOLocalMoveY(originalPos.y, 0.15f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.15f);

        if (victorySprite != null)
        {
            targetRenderer.sprite = victorySprite;

            Sequence seq = DOTween.Sequence();
            Vector3 targetPos = new Vector3(originalPos.x + moveXAmount, originalPos.y + 0.2f, originalPos.z);

            seq.Append(targetRenderer.transform.DOLocalMove(targetPos, 0.1f).SetEase(Ease.OutQuad));
            seq.Join(targetRenderer.transform.DOLocalRotate(new Vector3(0f, 0f, rotationAmount), 0.1f).SetEase(Ease.OutQuad));

            yield return seq.WaitForCompletion();

            yield return new WaitForSeconds(2f);

            Sequence resetSeq = DOTween.Sequence();
            resetSeq.Append(targetRenderer.transform.DOLocalMove(originalPos, 0.1f).SetEase(Ease.InQuad));
            resetSeq.Join(targetRenderer.transform.DOLocalRotate(originalRotation, 0.1f).SetEase(Ease.InQuad));

            yield return resetSeq.WaitForCompletion();

            targetRenderer.sprite = originalSprite;
        }
    }

    // 패배 모션
    private IEnumerator PlayDefeatedEffect(SpriteRenderer targetRenderer, float rotationAmount, float moveXAmount)
    {
        Vector3 originalPos = targetRenderer.transform.localPosition;
        Vector3 originalRotation = targetRenderer.transform.localEulerAngles;

        targetRenderer.transform.DOLocalMoveY(originalPos.y + 0.15f, 0.15f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.15f);
        targetRenderer.transform.DOLocalMoveY(originalPos.y, 0.15f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.15f);

        Sequence seq = DOTween.Sequence();
        Vector3 targetPos = new Vector3(originalPos.x + moveXAmount, originalPos.y + 0.2f, originalPos.z);

        seq.Append(targetRenderer.transform.DOLocalMove(targetPos, 0.1f).SetEase(Ease.OutQuad));
        seq.Join(targetRenderer.transform.DOLocalRotate(new Vector3(0f, 0f, rotationAmount), 0.1f).SetEase(Ease.OutQuad));

        yield return seq.WaitForCompletion();

        yield return new WaitForSeconds(2f);

        Sequence resetSeq = DOTween.Sequence();
        resetSeq.Append(targetRenderer.transform.DOLocalMove(originalPos, 0.1f).SetEase(Ease.InQuad));
        resetSeq.Join(targetRenderer.transform.DOLocalRotate(originalRotation, 0.1f).SetEase(Ease.InQuad));

        yield return resetSeq.WaitForCompletion();
    }


}
