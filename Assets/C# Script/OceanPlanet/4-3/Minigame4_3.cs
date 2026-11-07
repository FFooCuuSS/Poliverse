using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame4_3 : MiniGameBase
{
    [Header("Submarine")]
    public Transform submarine;
    [Tooltip("초당 상하 이동 속도. 맵 스크롤 속도와 같게 하면 45도로 진행")]
    public float verticalSpeed = 4f;

    [Header("Rotation")]
    [Tooltip("누르는 동안(상승) 돌릴 Z 각도. 떼면 0도(원래 이미지)로 돌아옴")]
    public float holdAngle = 90f;
    [Tooltip("초당 회전 속도(도). 크게 할수록 빨리 꺾임")]
    public float rotateSpeed = 360f;

    [Header("Bounds")]
    [Tooltip("화면 위아래 가장자리에서 띄울 여백 (월드 단위)")]
    [SerializeField] private float edgePadding = 0f;

    [Header("Debug")]
    [SerializeField] private bool holdLog = true;

    protected override float TimerDuration => 20f;
    protected override string MinigameTitle => "웨이브 잠수함";
    protected override string MinigameExplain => "화면을 누르면 위로, 떼면 아래로 진행합니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해서 오른쪽으로 계속 나아가세요."
    };

    public bool IsHolding { get; private set; }

    private bool playing;
    private Camera cam;
    private SpriteRenderer subRenderer;

    public override void StartGame()
    {
        base.StartGame();
        playing = true;
        SetHolding(false);

        cam = Camera.main;
        subRenderer = submarine != null ? submarine.GetComponentInChildren<SpriteRenderer>() : null;

        if (submarine != null)
            submarine.rotation = Quaternion.identity;
    }

    public override ScoreResult FinalizeScoreSession()
    {
        playing = false;
        SetHolding(false);
        return base.FinalizeScoreSession();
    }

    private void Update()
    {
        if (!playing) return;

        // 입력 잠금 중에는 홀드로 보지 않고 움직임도 멈춘다
        if (IsInputLocked)
        {
            SetHolding(false);
            return;
        }

        // 마우스 클릭과 모바일 터치 둘 다 GetMouseButton으로 잡힌다
        SetHolding(Input.GetMouseButton(0));

        if (submarine == null) return;

        // 상하 이동
        float dir = IsHolding ? 1f : -1f;
        submarine.position += Vector3.up * (dir * verticalSpeed * Time.deltaTime);

        // 회전: 누르는 동안만 holdAngle, 떼면 0도
        float target = IsHolding ? holdAngle : 0f;
        float z = Mathf.MoveTowardsAngle(submarine.eulerAngles.z, target, rotateSpeed * Time.deltaTime);
        submarine.rotation = Quaternion.Euler(0f, 0f, z);

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (cam == null) return;

        // 직교 카메라 기준: orthographicSize가 화면 높이의 절반(월드 단위)
        float half = subRenderer != null ? subRenderer.bounds.extents.y : 0f;

        float top = cam.transform.position.y + cam.orthographicSize - half - edgePadding;
        float bottom = cam.transform.position.y - cam.orthographicSize + half + edgePadding;

        Vector3 p = submarine.position;
        p.y = Mathf.Clamp(p.y, bottom, top);
        submarine.position = p;
    }

    private void SetHolding(bool value)
    {
        if (IsHolding == value) return;
        IsHolding = value;

        if (holdLog)
            Debug.Log(value ? "[4-3] 홀드 시작 (상승)" : "[4-3] 홀드 끝 (하강)");
    }
}
