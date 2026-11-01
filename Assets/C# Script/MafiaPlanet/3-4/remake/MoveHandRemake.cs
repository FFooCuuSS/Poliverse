using UnityEngine;

// 3-4 손(화살표) 이동. 노래 시간으로 위치를 직접 계산한다.
// 6.0초 등장, 6.6초부터 1.2초마다 x+2 (0.6초 이동 + 0.6초 정지)
// → 7.2 / 8.4 / 9.6 / 10.8 / 12.0초에 각 카드 위 (x = -4, -2, 0, 2, 4)
public class MoveHandRemake : MonoBehaviour
{
    public GameObject hand;

    [SerializeField] private Minigame_3_4_Remake minigame;

    [Header("위치")]
    [SerializeField] private Vector3 startPos = new Vector3(-6.0f, 2.0f, 0f);
    [SerializeField] private float stepX = 2f;
    [SerializeField] private int stepCount = 5;

    [Header("시간 (노래 시간, 초)")]
    [SerializeField] private float appearTime = 6.0f;
    [SerializeField] private float firstMoveTime = 6.6f;
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float stepInterval = 1.2f;

    private bool appeared;

    private void Awake()
    {
        if (minigame == null)
            minigame = GetComponentInParent<Minigame_3_4_Remake>();
    }

    private void Update()
    {
        if (hand == null || minigame == null) return;

        float t = minigame.SongTime;

        if (t < appearTime) return;

        if (!appeared)
        {
            appeared = true;
            hand.transform.position = startPos;
        }

        float x = startPos.x;
        float since = t - firstMoveTime;

        if (since > 0f)
        {
            int done = Mathf.Min(stepCount, Mathf.FloorToInt(since / stepInterval));
            x = startPos.x + stepX * done;

            if (done < stepCount)
            {
                float inStep = since - done * stepInterval;
                float k = Mathf.Clamp01(inStep / moveDuration);
                x += stepX * k;
            }
        }

        Vector3 p = hand.transform.position;
        p.x = x;
        p.y = startPos.y;
        hand.transform.position = p;
    }
}