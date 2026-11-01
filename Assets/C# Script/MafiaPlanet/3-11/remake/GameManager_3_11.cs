using System.Collections;
using UnityEngine;

// 3-11 진행 / 판정 (모든 시각은 노래 시간 기준)
// 물체 8개가 1.2초 간격으로 날아와 박스(중앙)에 도착한다. 도착 시각 = 소환 + travelTime.
// 판정(물체마다 1개, 총 8개):
//  - 서류(Target): 도착 ±goodWindow 안에 터치 → 성공 / 안 누르고 지나감 → 실패
//  - 폭탄(Bomb)  : 도착 ±goodWindow 안에 터치 → 실패 / 안 누르고 지나감 → 성공
//  - 어떤 물체 도착 범위에도 해당 안 되는 터치는 무시
// 점수는 Minigame_3_11_remake(진짜 미니게임)로 보고한다.
//
// ※ 예전에는 이 스크립트도 MiniGameBase였는데, 그러면 점수가 엉뚱한 오브젝트에 쌓이므로 MonoBehaviour로 바꿨다.
public class GameManager_3_11 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip bombSound;

    [Header("효과음")]
    [SerializeField] private AudioClip successSound;

    [Header("연결할 스크립트")]
    [SerializeField] private Minigame_3_11_remake minigame;   // 비우면 자동 탐색
    [SerializeField] private ObjectSpawner_3_11 objectSpawner;
    [SerializeField] private SliceZone_3_11 sliceZone;         // (이제 판정에 안 씀)

    [Header("점수 설정 (구버전 호환)")]
    [SerializeField] private int targetScore = 1;
    [SerializeField] private int bombPenalty = 1;

    [SerializeField] private GameObject normalBox;
    [SerializeField] private GameObject bombEffect;
    [SerializeField] private GameObject successEffect;

    [Header("판정 범위 (초) - 박스 도착 기준")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private int currentScore = 0;
    private bool isPlaying = false;
    private bool isDemoMode;
    private bool[] resolved;

    public int CurrentScore => currentScore;
    public bool IsPlaying => isPlaying;
    public int TotalCount => objectSpawner != null ? objectSpawner.TotalSpawnCount : 0;

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log("[3-11 GameManager] 데모 모드: " + isDemoMode);
    }

    private void Awake()
    {
        if (minigame == null)
            minigame = GetComponentInParent<Minigame_3_11_remake>();
        if (minigame == null)
            minigame = FindObjectOfType<Minigame_3_11_remake>();
    }

    private void Start()
    {
        if (normalBox != null) normalBox.SetActive(true);
        if (bombEffect != null) bombEffect.SetActive(false);
        if (successEffect != null) successEffect.SetActive(false);

        StartGame();
    }

    public void StartGame()
    {
        currentScore = 0;
        isPlaying = true;
        resolved = new bool[Mathf.Max(0, TotalCount)];

        if (sliceZone != null)
            sliceZone.ClearZone();

        if (objectSpawner != null)
            objectSpawner.BeginSpawn();
        else
            Debug.LogWarning("[3-11] ObjectSpawner_3_11이 연결되지 않았습니다.");

        Debug.Log("[3-11] 게임 시작");
    }

    private void Update()
    {
        if (!isPlaying || objectSpawner == null || minigame == null || resolved == null)
            return;

        float t = minigame.SongTime;
        if (t < 0f) return;

        if (isDemoMode)
        {
            // 시범: 서류가 박스에 도착하는 순간 터치 (폭탄은 무시)
            for (int i = 0; i < resolved.Length; i++)
            {
                if (!resolved[i] && !objectSpawner.IsBomb(i) && t >= objectSpawner.ArrivalTime(i))
                {
                    HandleTouch(t);
                    break;
                }
            }
        }
        else if (!minigame.IsInputLocked && Input.GetMouseButtonDown(0))
        {
            HandleTouch(t);
        }

        // 지나간 물체 정리
        for (int i = 0; i < resolved.Length; i++)
        {
            if (!resolved[i] && t > objectSpawner.ArrivalTime(i) + goodWindow)
            {
                if (objectSpawner.IsBomb(i))
                    Resolve(i, true, "폭탄 피함");
                else
                    Resolve(i, false, "서류 놓침");
            }
        }
    }

    private void HandleTouch(float t)
    {
        // 도착 시각이 가장 가까운 미판정 물체
        int best = -1;
        float bestDiff = float.MaxValue;

        for (int i = 0; i < resolved.Length; i++)
        {
            if (resolved[i]) continue;

            float diff = t - objectSpawner.ArrivalTime(i);
            if (Mathf.Abs(diff) <= goodWindow && Mathf.Abs(diff) < Mathf.Abs(bestDiff))
            {
                best = i;
                bestDiff = diff;
            }
        }

        if (best < 0)
        {
            JudgeLog($"[3-11 Judge] 터치 무시 (도착 범위 아님) song={t:F3}");
            return;
        }

        bool isBomb = objectSpawner.IsBomb(best);
        string grade = Mathf.Abs(bestDiff) <= perfectWindow ? "Perfect" : "Good";

        // 화면에서 물체 제거
        FlyingObject_3_11 obj = objectSpawner.GetSpawned(best);
        if (obj != null)
        {
            if (sliceZone != null) sliceZone.RemoveObject(obj);
            obj.ProcessObject();
        }

        if (isBomb)
            Resolve(best, false, $"폭탄 터치 diff={bestDiff * 1000:F0}ms");
        else
            Resolve(best, true, $"서류 담음 diff={bestDiff * 1000:F0}ms → {grade}", Mathf.Abs(bestDiff) <= perfectWindow);
    }

    // perfect: 서류를 담았을 때의 등급. 폭탄을 피한 건 타이밍이 없으므로 Perfect로 친다.
    private void Resolve(int i, bool success, string reason, bool perfect = true)
    {
        resolved[i] = true;

        bool touchedBomb = !success && objectSpawner.IsBomb(i);

        if (success)
        {
            minigame.ReportHit(perfect);

            // 서류를 담았을 때만 성공 연출 (폭탄 피한 건 조용히)
            if (!objectSpawner.IsBomb(i))
            {
                currentScore += targetScore;
                PlaySuccessEffect();
            }
        }
        else
        {
            minigame.ReportMiss();

            if (touchedBomb)
            {
                currentScore -= bombPenalty;
                PlayBombEffect();
            }
        }

        JudgeLog($"[3-11 Judge] {i + 1}번째 {(objectSpawner.IsBomb(i) ? "폭탄" : "서류")} {(success ? "성공" : "실패")} ({reason})");
    }

    // 미니게임이 중간에 끝날 때: 남은 물체 판정
    public void ResolveAllPending()
    {
        if (resolved == null || objectSpawner == null) return;

        for (int i = 0; i < resolved.Length; i++)
        {
            if (resolved[i]) continue;

            if (objectSpawner.IsBomb(i))
                Resolve(i, true, "미니게임 종료 (폭탄 안 건드림)");
            else
                Resolve(i, false, "미니게임 종료 (못 담음)");
        }
    }

    public void PlayBombEffect()
    {
        if (bombSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(bombSound);

        if (isActiveAndEnabled)
            StartCoroutine(BombEffectCoroutine());
    }

    private IEnumerator BombEffectCoroutine()
    {
        if (normalBox != null) normalBox.SetActive(false);
        if (bombEffect != null) bombEffect.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if (bombEffect != null) bombEffect.SetActive(false);
        if (normalBox != null) normalBox.SetActive(true);
    }

    public void PlaySuccessEffect()
    {
        if (successSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(successSound);

        if (isActiveAndEnabled)
            StartCoroutine(SuccessEffectCoroutine());
    }

    private IEnumerator SuccessEffectCoroutine()
    {
        if (normalBox != null) normalBox.SetActive(false);
        if (successEffect != null) successEffect.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if (successEffect != null) successEffect.SetActive(false);
        if (normalBox != null) normalBox.SetActive(true);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
    }

    public void SubtractScore(int amount)
    {
        currentScore -= amount;
    }

    public void StopGame()
    {
        if (!isPlaying) return;

        isPlaying = false;

        if (objectSpawner != null) objectSpawner.StopSpawn();
        if (sliceZone != null) sliceZone.ClearZone();

        Debug.Log("[3-11] 게임 정지 / 최종 점수: " + currentScore);
    }

    public void ResetScore()
    {
        currentScore = 0;
    }
}