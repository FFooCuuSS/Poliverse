using DG.Tweening;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

    [Header("효과음")]
    [SerializeField] private AudioClip grabSound;

    [Header("미니게임")]
    [SerializeField] private Minigame3_2remake minigame;

    [Header("오브젝트")]
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject grab;

    // Grab의 자식 Target
    [SerializeField] private Transform target;

    [Header("가로 이동 설정")]
    [SerializeField] private float startX = 12f;
    [SerializeField] private float moveY = 7f;

    [Tooltip("검은 가방 위치. 노래 시간 horizontalMoveTime에 여기 도착 = CSV Input 노트 시각")]
    [SerializeField] private float firstTargetX = -7f;
    [SerializeField] private float endX = -12f;

    // 12 -> -7까지 걸리는 시간 (노래 0초 출발 → 이 시각에 가방 위 도착)
    [SerializeField] private float horizontalMoveTime = 2.4f;

    [Header("랜덤 가방")]
    [Tooltip("켜면 검은 가방(Tag=Bag)을 4자리 중 랜덤 위치로 바꾼다. 손은 어느 자리든 같은 박자(horizontalMoveTime)에 그 가방 위에 온다.")]
    [SerializeField] private bool randomizeBag = true;
    [Tooltip("가방 오브젝트 이름에 포함된 문자열 (자리 후보 찾기용)")]
    [SerializeField] private string bagNameKey = "3_2_bag";

    // 이번 판에서 잡아야 하는 가방의 x
    private float targetBagX;

    [Header("세로 이동 설정")]
    [SerializeField] private float downY = 4.4f;
    [SerializeField] private float upY = 9.5f;

    // 내려가는 시간 / 올라가는 시간
    [SerializeField] private float verticalMoveTime = 1.2f;

    [Header("(구버전) 데모 좌표 트리거 - 지금은 CSV Input 박자로 동작")]
    [SerializeField] private float demoSuccessX = -7f;
    [SerializeField] private float demoPositionTolerance = 0.05f;

    // 현재 Hand와 충돌하고 있는 Bag
    private GameObject currentBag;

    // 실제로 잡은 Bag
    private GameObject grabbedBag;

    // 이미 클릭해서 동작이 시작됐는지
    private bool isActionStarted = false;

    // Bag을 실제로 잡았는지
    private bool hasBag = false;

    // 판정 결과 (내려간 뒤 잡을지 결정)
    private bool judgedSuccess = false;
    private bool reachedBottom = false;

    private bool isDemoMode = false;

    // 노래 시간 없을 때(테스트 씬 등) 대비 로컬 시간
    private float localTime;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log("[Hand] 데모 모드: " + isDemoMode);
    }

    private void Start()
    {
        hand.SetActive(true);
        grab.SetActive(false);

        hand.transform.position = new Vector3(startX, moveY, hand.transform.position.z);

        targetBagX = firstTargetX;

        if (randomizeBag)
            ArrangeRandomBag();
    }

    // 검은 가방을 4자리 중 랜덤 자리로 옮긴다 (그 자리의 일반 가방과 위치 교환)
    private void ArrangeRandomBag()
    {
        Transform root = minigame != null ? minigame.transform : transform.root;

        GameObject black = null;
        var slots = new System.Collections.Generic.List<Transform>();

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.parent == null || !t.name.Contains(bagNameKey)) continue;
            if (t.parent.name.Contains(bagNameKey)) continue; // 가방 자식 제외

            slots.Add(t);
            if (t.CompareTag("Bag")) black = t.gameObject;
        }

        if (black == null || slots.Count == 0)
        {
            Debug.LogWarning("[Hand] 랜덤 가방: 검은 가방(Tag=Bag) 또는 자리 후보를 못 찾음");
            return;
        }

        Transform pick = slots[Random.Range(0, slots.Count)];

        if (pick.gameObject != black)
        {
            Vector3 a = black.transform.position;
            black.transform.position = pick.position;
            pick.position = a;
        }

        targetBagX = black.transform.position.x;

        Debug.Log($"[Hand] 이번 판 검은 가방 x={targetBagX:F2} (후보 {slots.Count}개)");
    }

    // 클릭 순간 손이 검은 가방 위에서 몇 초 어긋났는지 (+면 늦음)
    public float GetTimingDiffSeconds()
    {
        float speed = (startX - firstTargetX) / Mathf.Max(0.01f, horizontalMoveTime);
        return (targetBagX - hand.transform.position.x) / Mathf.Max(0.01f, speed);
    }

    public bool IsHandPastEnd => !isActionStarted && hand.transform.position.x <= endX + 0.001f;

    // 속도는 고정 (startX→firstTargetX를 horizontalMoveTime에 가는 속도).
    // 어느 자리든 노래 horizontalMoveTime(=Input 노트)에 검은 가방 위에 오도록 출발 시각만 달라진다.
    // 가까운 자리면 늦게 출발한다.
    private float GetHandX(float t)
    {
        float speed = (startX - firstTargetX) / Mathf.Max(0.01f, horizontalMoveTime);
        float x = targetBagX + speed * (horizontalMoveTime - t);
        return Mathf.Clamp(x, endX, startX);
    }

    private void Update()
    {
        if (!isActionStarted && hand.activeInHierarchy)
        {
            float t;
            double song = minigame != null ? minigame.SongTime : -1;

            if (song >= 0)
            {
                t = (float)song;
            }
            else
            {
                localTime += Time.deltaTime;
                t = localTime;
            }

            Vector3 p = hand.transform.position;
            p.x = GetHandX(t);
            hand.transform.position = p;
        }

        // Bag을 잡은 상태라면 Grab의 자식 Target 위치를 계속 따라간다.
        if (hasBag && grabbedBag != null && target != null)
            grabbedBag.transform.position = target.position;
    }

    /// <summary>
    /// 클릭(또는 시범)했을 때 호출. 손을 멈추고 내려간다.
    /// </summary>
    public void StartGrabAction()
    {
        if (isActionStarted) return;

        if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clickSound);

        isActionStarted = true;
        reachedBottom = false;

        Debug.Log($"[Hand] 클릭 - 하강 시작 x={hand.transform.position.x:F2}");

        hand.transform
            .DOMoveY(downY, verticalMoveTime)
            .SetEase(Ease.Linear)
            .OnComplete(OnReachedBottom);
    }

    /// <summary>
    /// 미니게임이 판정 결과를 알려준다. (손이 내려가는 중에 온다)
    /// </summary>
    public void SetJudgeResult(bool success)
    {
        judgedSuccess = success;

        // 이미 바닥이면 바로 반영
        if (reachedBottom && success && !hasBag)
            GrabBag(FindBlackBag());
    }

    private void OnReachedBottom()
    {
        reachedBottom = true;

        // 판정 성공이면 잡는다 (타이밍 판정과 화면 결과를 일치시킴)
        if (judgedSuccess)
        {
            GrabBag(currentBag != null ? currentBag : FindBlackBag());
        }
        else
        {
            MoveHandUp();
        }
    }

    private GameObject FindBlackBag()
    {
        if (currentBag != null) return currentBag;

        Transform root = minigame != null ? minigame.transform : transform.root;

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("Bag"))
                return t.gameObject;
        }

        return null;
    }

    private void GrabBag(GameObject bag)
    {
        if (bag == null)
        {
            MoveHandUp();
            return;
        }

        AudioClip sfx = grabSound != null ? grabSound : clickSound;
        if (sfx != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(sfx);

        hasBag = true;
        grabbedBag = bag;

        Vector3 handPosition = hand.transform.position;

        hand.transform.DOKill();
        hand.SetActive(false);

        grab.SetActive(true);
        grab.transform.position = handPosition;

        if (target != null)
            grabbedBag.transform.position = target.position;

        grab.transform
            .DOMoveY(upY, verticalMoveTime)
            .SetEase(Ease.Linear)
            .OnComplete(() => Debug.Log("[Hand] Bag 잡기 완료"));
    }

    private void MoveHandUp()
    {
        hand.transform
            .DOMoveY(upY, verticalMoveTime)
            .SetEase(Ease.Linear)
            .OnComplete(() => Debug.Log("[Hand] 빈손 복귀 완료"));
    }

    public void EnterBag(GameObject bag)
    {
        if (bag == null) return;
        currentBag = bag;
    }

    public void ExitBag(GameObject bag)
    {
        if (currentBag != bag) return;
        currentBag = null;
    }
}