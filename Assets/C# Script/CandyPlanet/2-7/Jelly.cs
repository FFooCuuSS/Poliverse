using UnityEngine;

public class Jelly : MonoBehaviour
{
    public JellyType data;

    [SerializeField] private float gravity = 9.8f; // 낙하 가속도. bounceForce와 함께 주기를 자동 계산

    private JellySpawner spawner;
    private SpriteRenderer sr;

    private Vector3 spawnPos;
    private float groundY;
    private float spriteHalfHeight; // 피벗이 Center인 스프라이트 보정용 (아랫면이 groundY에 닿게)
    private float verticalTimer; // 높이 계산 전용 (스폰 시 정점에서 시작해서 자연스럽게 첫 착지)
    private float moveTimer;     // x이동/xStep 판정 전용 (스폰 시 0부터 시작)
    private float cyclePeriod;
    private bool triggeredNextSpawn;
    private float currentLogicalHeight; // 시각적 오프셋과 별개로, 터치 판정에 쓰는 순수 높이

    public void Init(JellySpawner sp, float groundY)
    {
        spawner = sp;
        this.groundY = groundY;
        spawnPos = transform.position;
        triggeredNextSpawn = false;

        // bounceForce 하나로 높이/주기가 결정되고, 매 사이클 동일 공식으로 재계산되므로
        // Rigidbody 없이도 감쇠(에너지 손실) 없이 항상 같은 높이로 튐
        cyclePeriod = (data != null && data.bounceForce > 0f)
            ? (2f * data.bounceForce / gravity)
            : 1f;

        // 스폰 시 바로 땅(높이 0)에서 시작하면 첫 프레임에 "방금 닿았다"고 오판정되므로,
        // 정점(사이클 절반 지점)에서 시작해서 위->아래로 자연스럽게 떨어지며 첫 착지하게 함
        verticalTimer = cyclePeriod / 2f;
        moveTimer = 0f;

        if (sr != null && data != null && data.sr != null)
            sr.sprite = data.sr;

        // 스프라이트 피벗이 Center라면, 중심이 아니라 아랫면이 바닥에 닿아 보이도록 절반 높이만큼 보정
        spriteHalfHeight = (sr != null && sr.sprite != null) ? sr.bounds.extents.y : 0f;

        // 첫 프레임에 땅에서 순간이동하듯 튀는 걸 방지하기 위해, 시작 높이를 미리 반영
        float startPhaseTime = verticalTimer % cyclePeriod;
        float startHeight = Mathf.Max(0f, data.bounceForce * startPhaseTime - 0.5f * gravity * startPhaseTime * startPhaseTime);
        Vector3 startPos = transform.position;
        currentLogicalHeight = startHeight;
        startPos.y = groundY + spriteHalfHeight + startHeight;
        transform.position = startPos;

        if (data != null)
        {
            if (data.xStep <= 0f)
                Debug.LogWarning($"[Jelly] '{data.name}'의 xStep이 {data.xStep}입니다. 스폰 직후 거의 즉시 다음 젤리로 넘어갑니다. 인스펙터에서 값을 확인하세요.");

            if (data.bounceForce <= 0f)
                Debug.LogWarning($"[Jelly] '{data.name}'의 bounceForce가 {data.bounceForce}입니다. 점프 높이가 0이 됩니다.");
        }
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (data == null || spawner == null) return;

        verticalTimer += Time.deltaTime;
        moveTimer += Time.deltaTime;

        // 감쇠 없는 포물선 바운스 공식
        float phaseTime = verticalTimer % cyclePeriod;
        float height = data.bounceForce * phaseTime - 0.5f * gravity * phaseTime * phaseTime;
        height = Mathf.Max(0f, height);
        currentLogicalHeight = height; // 터치 판정은 이 값 기준 (시각 보정과 무관)

        // 종류별 흔들림(파형) 오버레이
        float wobble = Mathf.Sin(moveTimer * data.waveSpeed) * data.waveAmount;

        Vector3 pos = transform.position;
        pos.y = groundY + spriteHalfHeight + height; // 스프라이트 아랫면이 groundY에 닿도록 보정
        pos.x = spawnPos.x + data.moveSpeed * moveTimer + wobble;
        transform.position = pos;

        // 스폰 지점 기준 xStep 이상 이동하면 다음 젤리 스폰 트리거
        if (!triggeredNextSpawn && Mathf.Abs(pos.x - spawnPos.x) >= data.xStep)
        {
            triggeredNextSpawn = true;
            spawner.Spawn();
        }
    }

    // Stone이 닿았는지 판정할 때 쓰는, 바닥 기준 현재 높이 
    public float CurrentHeightAboveGround => currentLogicalHeight;
}