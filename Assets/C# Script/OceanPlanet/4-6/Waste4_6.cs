using UnityEngine;

public class Waste4_6 : MonoBehaviour
{
    public double ArriveDsp => startDsp + arriveTime;
    public int PressCount => pressCount;

    [SerializeField] private int requiredPresses = 1;
    private int pressCount;
    private int score;

    [SerializeField] private Sprite[] stageSprites;

    private SpriteRenderer sr;
    private Vector3 startPos;
    private double startDsp;
    private float speed;
    private float endX;
    private float stopX;
    private bool released;
    private double releaseDsp;
    private float arriveTime;
    private float yOffset;

    public int Stage { get; private set; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(float moveSpeed, float leftEndX, float stopPointX)
    {
        speed = moveSpeed;
        endX = leftEndX;
        stopX = stopPointX;
        startPos = transform.position;
        startDsp = AudioSettings.dspTime;
        arriveTime = Mathf.Max(0f, (startPos.x - stopX) / speed);
        Debug.Log($"[4-6] 도착까지 {arriveTime:F3}초");
        Stage = 0;
        ApplySprite();
    }

    public void Release()
    {
        if (released) return;
        released = true;
        releaseDsp = AudioSettings.dspTime;
    }

    public void AddStage()
    {
        Stage = Mathf.Min(Stage + 1, stageSprites.Length - 1);
        ApplySprite();
    }

    private void ApplySprite()
    {
        Sprite s = stageSprites[Stage];
        sr.sprite = s;
        yOffset = (stageSprites[0].bounds.min.y - s.bounds.min.y) * sr.transform.lossyScale.y;
        Debug.Log($"[4-6] 폐기물 결과: 점수 {score}/{requiredPresses * 2} → 상태 {Stage}");
    }

    private void Update()
    {
        float t = (float)(AudioSettings.dspTime - startDsp);
        float x;
        if (t < arriveTime) x = startPos.x - speed * t;
        else if (!released) x = stopX;
        else
        {
            double leaveStart = System.Math.Max(releaseDsp, startDsp + arriveTime);
            x = stopX - speed * (float)(AudioSettings.dspTime - leaveStart);
        }

        Vector3 p = startPos;
        p.x = x;
        p.y += yOffset;
        transform.position = p;

        if (x <= endX) Destroy(gameObject);
    }

    public bool AddJudgement(int points)
    {
        pressCount++;
        score += points;
        if (pressCount < requiredPresses)
        {
            if (points > 0)
            {
                Stage = 1;
                ApplySprite();
            }
            return false;
        }

        if (score >= requiredPresses * 2) Stage = 2;
        else if (score <= 0) Stage = 0;
        else Stage = 1;
        ApplySprite();
        return true;
    }
}