using System.Collections;
using UnityEngine;

public class Minigame_4_9 : MiniGameBase
{
    [Header("Bubble")]
    [SerializeField]
    private BubbleUp[] bubblePrefabs;

    [SerializeField]
    private Transform bubbleSpawnPoint;

    [SerializeField]
    private Transform bubbleLine;

    [SerializeField]
    private float bubbleTravelDuration = 2f;

    [SerializeField]
    private float bubbleAfterLineSpeed = 2f;

    [SerializeField]
    private float bubbleDestroyY = 8f;

    [Header("Dart")]
    [SerializeField]
    private DartProjectile dartPrefab;

    [SerializeField]
    private Transform dartSpawnPoint;

    [SerializeField]
    private float dartDirectionX = 1f;

    [SerializeField]
    private float dartSpeed = 8f;

    [SerializeField]
    private float dartRotationZ = 280f;

    [SerializeField]
    private float dartCooldown = 0.1f;

    [Header("Effect Objects")]
    [SerializeField]
    private ThrowEffact throwEffact;

    [Header("Test Input")]
    [SerializeField]
    private KeyCode playerInputKey = KeyCode.Space;

    protected override bool UseRhythmJudgementScore =>
        false;

    protected override int ManualTotalNodeCount =>
        -1;

    protected override float TimerDuration =>
        15f;

    protected override string MinigameTitle =>
        "비눗방울 끄기";

    protected override string MinigameExplain =>
        "정확히 올라오는 물공기를 다트로 맞혀 터뜨리세요.";

    private bool isDartOnCooldown;

    private void Update()
    {
        if (Input.GetKeyDown(playerInputKey))
        {
            OnPlayerInput();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (action == "Show")
        {
            SpawnBubble();
        }
    }

    private void SpawnBubble()
    {
        if (bubblePrefabs == null ||
            bubblePrefabs.Length == 0 ||
            bubbleSpawnPoint == null ||
            bubbleLine == null)
        {
            Debug.LogWarning(
                "[Minigame_4_9] Bubble 관련 오브젝트가 연결되지 않았습니다."
            );

            return;
        }

        int bubbleIndex = Random.Range(
            0,
            bubblePrefabs.Length
        );

        BubbleUp selectedBubblePrefab =
            bubblePrefabs[bubbleIndex];

        BubbleUp bubble = Instantiate(
            selectedBubblePrefab,
            bubbleSpawnPoint.position,
            Quaternion.identity
        );

        bubble.Initialize(
            this,
            bubbleLine.position.y,
            bubbleTravelDuration,
            bubbleDestroyY
        );
    }

    public void OnPlayerInput()
    {
        if (IsDemoMode || IsInputLocked)
        {
            return;
        }

        if (isDartOnCooldown)
        {
            return;
        }

        ThrowDart();
        StartCoroutine(DartCooldownCoroutine());
    }

    private void ThrowDart()
    {
        if (dartPrefab == null || dartSpawnPoint == null)
        {
            Debug.LogWarning(
                "[Minigame_4_9] Dart 관련 오브젝트가 연결되지 않았습니다."
            );

            return;
        }

        Quaternion dartRotation =
            Quaternion.Euler(0f, 0f, dartRotationZ);

        DartProjectile dart = Instantiate(
            dartPrefab,
            dartSpawnPoint.position,
            dartRotation
        );

        dart.SetDirection(
            dartDirectionX,
            dartSpeed
        );

        if (throwEffact != null)
        {
            throwEffact.Throw();
        }
    }

    private IEnumerator DartCooldownCoroutine()
    {
        isDartOnCooldown = true;

        yield return new WaitForSeconds(dartCooldown);

        isDartOnCooldown = false;
    }

    public void OnBubbleHit(BubbleUp bubble)
    {
        ReportManualSuccess();

        Destroy(bubble.gameObject);
    }

    public void OnBubbleMiss(BubbleUp bubble)
    {
        ReportManualFail();

        Destroy(bubble.gameObject);
    }

    private void OnDisable()
    {
        isDartOnCooldown = false;
    }
}
