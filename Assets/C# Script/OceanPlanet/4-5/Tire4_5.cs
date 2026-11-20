using UnityEngine;

public class Tire4_5 : MonoBehaviour
{
    [Tooltip("움직이는 동안 초당 회전 각도(도). 0이면 회전 안 함")]
    public float spinSpeed = 30f;

    [Header("Hop (패턴의 클릭 간격과 같게)")]
    [Tooltip("등장부터 왼쪽 복어에 닿을 때까지 = 첫 클릭 시각 - Show 시각")]
    public float hop1Duration = 0.5f;
    [Tooltip("왼쪽 복어에서 오른쪽 복어에 닿을 때까지 = 두 번째 클릭 시각 - 첫 클릭 시각")]
    public float hop2Duration = 1f;
    public float hop1Arc = 0.8f;
    public float hop2Arc = 1.5f;

    private readonly bool[] sideDone = new bool[2];
    public bool Failed { get; private set; }
    public bool Resolved { get; set; }
    public int HopCount { get; private set; }

    private Vector3 startPos;
    private Vector3 endPos;
    private double startDsp;
    private float duration = 1f;
    private float gravity;
    private float vy;
    private bool isFall;
    private bool linear;
    private bool destroyAtEnd = true;
    private bool moving;

    public bool IsSideDone(int side) => sideDone[side];

    // side: 0 = 왼쪽, 1 = 오른쪽, -1 = 모르면(자동 Miss) 아직 안 끝난 쪽부터. 둘 다 끝나면 true
    public bool AddJudgement(int side, MiniGameBase.JudgementResult result)
    {
        if (side < 0) side = sideDone[0] ? 1 : 0;

        sideDone[side] = true;
        if (result == MiniGameBase.JudgementResult.Miss) Failed = true;

        return sideDone[0] && sideDone[1];
    }

    // 복어 위로 튀어서 착지 (사라지지 않음)
    public void Hop(Vector3 land, float dur, float apexHeight)
    {
        HopCount++;
        MoveTo(land, dur, apexHeight, false, false);
    }

    // 이미 떨어진 타이어를 정리 (낙하 연출이 끝날 시간을 주고 삭제)
    public void Retire()
    {
        Destroy(gameObject, 1f);
    }

    public void MoveTo(Vector3 end, float dur, float apexHeight, bool fall, bool destroyAtEnd = true)
    {
        startPos = transform.position;
        endPos = end;
        duration = Mathf.Max(0.01f, dur);
        isFall = fall;
        this.destroyAtEnd = destroyAtEnd;
        startDsp = AudioSettings.dspTime;
        moving = true;

        // 가장 높은 점이 시작/도착 중 높은 쪽보다 apexHeight만큼 위가 되도록 중력과 초기 속도를 계산
        float top = Mathf.Max(startPos.y, end.y) + Mathf.Max(0f, apexHeight);
        float hUp = top - startPos.y;
        float hDown = top - end.y;
        float s = Mathf.Sqrt(hUp) + Mathf.Sqrt(hDown);

        linear = s < 0.0001f;
        gravity = linear ? 0f : 2f * s * s / (duration * duration);
        vy = Mathf.Sqrt(2f * gravity * hUp);
    }

    private void Update()
    {
        if (!moving) return;

        float t = (float)((AudioSettings.dspTime - startDsp) / duration);
        if (t >= 1f) { transform.position = endPos; moving = false; if (destroyAtEnd) Destroy(gameObject); return; }

        float tau = t * duration;
        Vector3 p = Vector3.Lerp(startPos, endPos, t);

        if (isFall) p.y = startPos.y + (endPos.y - startPos.y) * t * t;
        else if (!linear) p.y = startPos.y + vy * tau - 0.5f * gravity * tau * tau;

        transform.position = p;

        if (spinSpeed != 0f) transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }
}